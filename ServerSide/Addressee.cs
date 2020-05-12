using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ServerSide
{
    public class MessageAddressee
    {
        public List<TcpClient> tcp { get; set; }
        public DateTime SendMoment { get; set; }
        public string Message { get; set; }

    }
}
