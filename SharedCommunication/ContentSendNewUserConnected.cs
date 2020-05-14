using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCommunication
{
    public class ContentSendNewUserConnected
    {
        public string UserAddedName { get; set; }
        public Byte[] UserAddedPhoto { get; set; }
    }
}
