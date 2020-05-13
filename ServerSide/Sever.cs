using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using SharedCommunication;
using Newtonsoft.Json;

namespace ServerSide
{
    class ChatServer
    {
        public static Dictionary<TcpClient, string> DicOfConnections = new Dictionary<TcpClient, string>();
        public static Dictionary<TcpClient, string> DicOfGroups = new Dictionary<TcpClient, string>();

        private Thread listenerProcess;
        private TcpListener tcpClientListener;
        private bool IsSeverRunning = false;

        public void SetServerOnline(IPAddress ipAdress)
        {
            try
            {
                tcpClientListener = new TcpListener(ipAdress, 4040);
                tcpClientListener.Start();
                IsSeverRunning = true;
                listenerProcess = new Thread(KeepRunning);
                listenerProcess.Start();

                Console.WriteLine("Sucess: The Server is online " + DateTime.Now.ToString());
                Console.WriteLine(string.Format("Server runing on  {0} : {1}", ipAdress, 4040));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: SendingMessage - " + ex.ToString());
            }
        }

        private void KeepRunning()
        {
            while (IsSeverRunning == true)
            {
                var tcpClient = tcpClientListener.AcceptTcpClient();
                Connection newConnection = new Connection(tcpClient);
            }
        }

        public static void AddUser(TcpClient tcpUser, string userName)
        {
            ChatServer.DicOfConnections.Add(tcpUser, userName);
            //TODO: CRIAR UMA FORMA DE MOSTRAR SISTEMATICAMENTE QUE ALGUÉM CHEGOU
        }

        public static void RemoveUser(TcpClient tcpUser)
        {
            if (DicOfConnections[tcpUser] != null)
            {
                //TODO: CRIAR UMA FORMA DE MOSTRAR SISTEMATICAMENTE QUE ALGUÉM SAIU
                DicOfConnections.Remove(tcpUser);
            }
        }



        public static void SenderActionRouter(string message)
        {
            try
            {
                ComnModel MessageObj = BuildModel(message);

                if ((int)MessageObj.ContentAction > 2)
                {
                    SendMessageToASingleAddresee(MessageObj);
                }
                else if ((int)MessageObj.ContentAction > 4)
                {
                    UseMessageAsSettings(MessageObj);
                }
                else
                {
                    SendMesseToAGroup(MessageObj);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: SendingMessage - " + ex.ToString());
            }  
        }

        public static ComnModel BuildModel(string Message)
        {
           return JsonConvert.DeserializeObject<ComnModel>(Message);
        }

        private static void SendMessageToASingleAddresee(ComnModel messageObj)
        {
            WriteMessageOnStream
                (DicOfConnections.FirstOrDefault(C => C.Value == messageObj.Addresee).Key,
                ((ContentSendMessage)messageObj.Content).Message);
        }

        private static void UseMessageAsSettings(ComnModel messageObj)
        {
            throw new NotImplementedException();
        } 

        private static void SendMesseToAGroup(ComnModel messageObj)
        {
            var tcpOnGroup = (from t in DicOfGroups
                              where t.Value == messageObj.Addresee
                              select t.Key).ToList();

            foreach (TcpClient client in tcpOnGroup)
            {
                WriteMessageOnStream(client, JsonConvert.SerializeObject(messageObj));
            }
        }

        public static void WriteMessageOnStream(TcpClient tcpClient, string message)
        {
            using (var swSenderSender = new StreamWriter(tcpClient.GetStream()))
            {
                swSenderSender.WriteLine(message);
                swSenderSender.Flush();
            }
        }

    }
}
