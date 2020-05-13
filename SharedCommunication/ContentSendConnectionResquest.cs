using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCommunication
{
    public class ContentSendConnectionResquest : ContentModel
    {
        public string UserName { get; set; }
        public Byte[] UserPhoto { get; set; }
    }
}
