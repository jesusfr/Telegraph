using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mono.Addins;

using Telegraph;

[assembly: Addin]
[assembly: AddinDependency("Telegraph", "0.1")]

namespace Telegraph.Clients.Dumb
{
    [Extension]
    public class Client : Telegraph.Client
    {
        Core _core;

        public override void OnInitialize(Core core, out string name)
        {
            _core = core;
            name = "Dumb";
        }

        public override void OnConnectUser(UserAccount user)
        {
        }

        public override void OnDisconnectUser(UserAccount user)
        {
        }

        public override void OnUpdate(UserAccount user)
        {
            {
                Random rnd = new Random(((int) (DateTime.Now.Ticks)) >> user.Name.Length);
                Thread.Sleep(rnd.Next(2000));
            }

            Message msg = new Message("Hello world!", user, DateTime.Now, null);
            _core.Timeline.OnNewMessage(msg);
        }

        public override void OnTerminate()
        {
        }
    }
}
