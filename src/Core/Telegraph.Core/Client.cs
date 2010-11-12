using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Addins;

namespace Telegraph
{
    [TypeExtensionPoint]
    public abstract class Client
    {
        public abstract void OnInitialize(Core core, out string name);
        public abstract void OnConnectUser(UserAccount user);
        public abstract void OnDisconnectUser(UserAccount user);
        public abstract void OnUpdate(UserAccount user);
        public abstract void OnTerminate();
    }
}