using System;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace ServerSide
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"//////////// Initalizing Server \\\\\\\\\\\");
            Console.WriteLine();
            Console.WriteLine();
            ChatServer mainServer = new ChatServer();
            Console.WriteLine("Type the IP adress: ");
            IPAddress enderecoIP = IPAddress.Parse(Console.ReadLine());
            mainServer.SetServerOnline(enderecoIP);
        }

        static void OnClosing()
        {
            if (!Directory.Exists(@"c:\SocktesApp")) Directory.CreateDirectory(@"c:\SocktesApp");

            var ActivesGroups = ChatServer.DicOfGroups.ToList();
            using (var StWriter = new StreamWriter(@"c:\SocktesApp\GroupsInfo.json"))
            {
                StWriter.Write(JsonConvert.SerializeObject(ActivesGroups));
            }
                

        }

    }
}
