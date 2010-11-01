using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph
{
    public interface IFrontend
    {
        void OnNewMessage(Message msg);
    }
}
