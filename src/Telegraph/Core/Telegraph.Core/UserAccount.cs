using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph
{
    public class UserAccount
    {
        public UserAccount(string name, string password, IPlugin plugin)
        {
            _name = name;
            _plugin = plugin;
            Password = password;
        }

        string _name;
        public string Name { get { return _name; } }

        public string Password { get; set; }

        IPlugin _plugin;
        public IPlugin Plugin { get { return _plugin; } }
    }
}
