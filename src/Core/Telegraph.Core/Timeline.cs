using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Telegraph
{
    public class Timeline
    {
        AutoResetEvent _free_event;

        private Core _core;

        public Timeline(Core core)
        {
            _free_event = new AutoResetEvent(true);
            _core = core;
        }

        public void OnNewMessage(Message msg)
        {
            if (_free_event.WaitOne(5000))
            {
                _core.Frontend.OnNewMessage(msg);
                _free_event.Set();
            }
        }
    }
}
