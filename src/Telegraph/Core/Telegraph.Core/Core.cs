using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Addins;

[assembly: AddinRoot("Telegraph", "0.1")]

namespace Telegraph
{
    public class Core
    {
        List<UserAccount> _users;

        Dictionary<string, IPlugin> _plugins;

        public Core(IFrontend frontend)
        {
            _frontend = frontend;

            _users = new List<UserAccount>();
            _plugins = new Dictionary<string, IPlugin>();
            _timeline = new Timeline(this);
        }

        public void Start()
        {
            AddinManager.Initialize();
            AddinManager.Registry.Update();

            foreach (IPlugin plugin in AddinManager.GetExtensionObjects<IPlugin>())
            {
                string name;
                plugin.OnInitialize(this, out name);
                _plugins.Add(name, plugin);
            }

            {
                IPlugin plugin = _plugins["Dumb"];

                _users = new List<UserAccount>();

                UserAccount user = new UserAccount("lrgar", "dumb", plugin);
                _users.Add(user);
                plugin.OnNewUser(user);
            }
        }

        public void End()
        {
            foreach (var plugin in _plugins)
                plugin.Value.OnTerminate();
        }

        public void Update()
        {
            foreach (var user in _users)
                user.Plugin.OnUpdate(user);
        }

        private IFrontend _frontend;
        public IFrontend Frontend { get { return _frontend; } }

        private Timeline _timeline;
        public Timeline Timeline { get { return _timeline; } }
    }
}
