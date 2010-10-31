using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Addins;

using Telegraph;

[assembly: Addin]
[assembly: AddinDependency("Telegraph", "0.1")]

namespace Telegraph.Plugins.Dumb
{
    [Extension]
    public class Plugin : Telegraph.IPlugin
    {
        Core _core;

        public void OnInitialize(Core core, out string name)
        {
            _core = core;
            name = "Dumb";
        }

        public void OnNewUser(UserAccount user)
        {
        }

        public void OnUpdate(UserAccount user)
        {
            Message msg = new Message("Hello world!", user, DateTime.Now, null);
            _core.Timeline.OnNewMessage(msg);
        }

        public void OnTerminate()
        {
        }
    }
}
