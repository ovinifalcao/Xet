using System.Collections.Generic;
using System.Net.Sockets;

namespace SharedCommunication
{
    public class ContentSendUserIsDisconnecting
    {
        public string Client { get; set; }
        public List<string> GroupsWithTheUSer { get; set; }
    }
}
