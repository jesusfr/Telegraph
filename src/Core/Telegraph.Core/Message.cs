using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph
{
    public class Message
    {
        public Message(string contents, UserAccount user, DateTime sent, object tag)
        {
            _contents = contents;
            _user = user;
            _sent = sent;
            Tag = tag;
        }

        private string _contents;

        public string Contents
        {
            get { return _contents; }
        }

        private UserAccount _user;

        public UserAccount User
        {
            get { return _user; }
        }

        private DateTime _sent;

        public DateTime SentDate
        {
            get { return _sent; }
        }

        public object Tag { get; set; }
    }
}
