// This file uses code written by Matt Valerio, taken on November 11th, 2010 from
// http://thevalerios.net/matt/2008/05/use-threadpoolqueueuserworkitem-with-anonymous-types/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mono.Addins;

[assembly: AddinRoot("Telegraph", "0.1")]

namespace Telegraph
{
    internal static class ThreadPoolHelper
    {
        public static bool QueueUserWorkItem<T>(T state, Action<T> callback)
        {
            return ThreadPool.QueueUserWorkItem(s => callback((T) s), state);
        }
    }

    public enum UpdateModes
    {
        Sequential,
        Parallel
    }

    public class Core
    {
        private List<UserAccount> _users;

        private Dictionary<string, Client> _plugins;

        public Core(IFrontend frontend)
        {
            _update_mode = UpdateModes.Parallel;
            _frontend = frontend;

            _users = new List<UserAccount>();
            _plugins = new Dictionary<string, Client>();
            _timeline = new Timeline(this);
        }

        public void Start()
        {
            AddinManager.Initialize();
            AddinManager.Registry.Update();

            foreach (Client plugin in AddinManager.GetExtensionObjects<Client>())
            {
                string name;
                plugin.OnInitialize(this, out name);
                _plugins.Add(name, plugin);
            }

            _users = new List<UserAccount>();

            {
                Client plugin = _plugins["Dumb"];

                {
                    UserAccount user = new UserAccount("lrgar", "dumb", plugin);
                    _users.Add(user);
                    plugin.OnConnectUser(user);
                }

                {
                    UserAccount user = new UserAccount("jesusfr", "dumb", plugin);
                    _users.Add(user);
                    plugin.OnConnectUser(user);
                }
            }
        }

        public void End()
        {
            foreach (var user in _users)
                user.Plugin.OnDisconnectUser(user);

            foreach (var plugin in _plugins)
                plugin.Value.OnTerminate();
        }

        public void Update()
        {
            switch (_update_mode)
            {
                case UpdateModes.Sequential:
                    foreach (var user in _users)
                        user.Plugin.OnUpdate(user);
                    break;

                case UpdateModes.Parallel:
                    ManualResetEvent[] events = new ManualResetEvent[_users.Count];

                    int i = 0;
                    foreach (var user in _users)
                    {
                        ManualResetEvent ev = new ManualResetEvent(false);

                        events[i++] = ev;

                        ThreadPoolHelper.QueueUserWorkItem(
                            new { User = user, Event = ev },
                            (data) =>
                            {
                                UserAccount u = data.User;
                                u.Plugin.OnUpdate(u);
                                data.Event.Set();
                            }
                        );
                    }

                    WaitHandle.WaitAll(events);

                    break;
            }
        }

        private IFrontend _frontend;
        public IFrontend Frontend { get { return _frontend; } }

        private Timeline _timeline;
        public Timeline Timeline { get { return _timeline; } }

        private UpdateModes _update_mode;
        public UpdateModes UpdateMode { get { return _update_mode; } }
    }
}
