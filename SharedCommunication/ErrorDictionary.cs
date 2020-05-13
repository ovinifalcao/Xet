using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCommunication
{
    public class ErrorDictionary
    {
        public static Dictionary<int, string> KnownErrors = new Dictionary<int, string>()
        {
            [601] = "Connection failure, user already exists"
        };

    }
}
