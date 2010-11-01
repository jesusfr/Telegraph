using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph
{
    public class Timeline
    {
        private Core _core;

        public Timeline(Core core)
        {
            _core = core;
        }

        public void OnNewMessage(Message msg)
        {
            _core.Frontend.OnNewMessage(msg);
        }
    }
}
