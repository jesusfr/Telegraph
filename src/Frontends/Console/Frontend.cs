using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Telegraph;

namespace Telegraph.Frontends.Console
{
    class Frontend : Telegraph.IFrontend
    {
        public void OnNewMessage(Telegraph.Message msg)
        {
            System.Console.WriteLine(msg.Contents);
            System.Console.WriteLine(String.Format("sent by {0} at {1:ddd, MMM dd, yyyy - hh:mm:ss}", msg.User.Name, msg.SentDate));
        }
    }
}
