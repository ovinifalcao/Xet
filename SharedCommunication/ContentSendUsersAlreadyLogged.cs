using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCommunication
{
    public class ContentSendUsersAlreadyLogged
    {
        public List<Tuple<string, Byte[]>> AlreadyLoggedUsers { get; set; }
    }
}
