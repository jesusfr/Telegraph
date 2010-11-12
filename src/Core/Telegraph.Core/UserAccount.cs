using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph
{
    public class UserAccount
    {
        public UserAccount(string name, string password, Client plugin)
        {
            _name = name;
            _plugin = plugin;
            Password = password;
        }

        string _name;
        public string Name { get { return _name; } }

        public string Password { get; set; }

        Client _plugin;
        public Client Plugin { get { return _plugin; } }
    }
}
