using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Addins;

namespace Telegraph
{
    [TypeExtensionPoint]
    public interface IPlugin
    {
        void OnInitialize(Core core, out string name);
        void OnNewUser(UserAccount user);
        void OnUpdate(UserAccount user);
        void OnTerminate();
    }
}
