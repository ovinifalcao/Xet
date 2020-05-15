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
        public static Dictionary<string, TcpClient> DicOfConnections = new Dictionary<string, TcpClient>();
        public static Dictionary<string, TcpClient> DicOfGroups = new Dictionary<string, TcpClient>();

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
            var UsersBeforeAdding = DicOfConnections.ToList();
            ChatServer.DicOfConnections.Add(userName, tcpUser);

            foreach (KeyValuePair<string, TcpClient> Dic in UsersBeforeAdding)
            {
                WriteMessageOnStream(Dic.Value,
                    JsonConvert.SerializeObject(new ComnModel()
                    {
                        Addresee = Dic.Key,
                        ContentAction = ComnModel.Actions.SendNewUSerConneted,
                        Moment = DateTime.Now,
                        Content = JsonConvert.SerializeObject(
                            new ContentSendNewUserConnected()
                            {
                                UserAddedName = userName,
                                UserAddedPhoto = null
                            })
                    }));
            }

            WriteMessageOnStream(tcpUser,
                JsonConvert.SerializeObject(new ComnModel()
                {
                    Addresee = null,
                    ContentAction = ComnModel.Actions.SendUsersAlreadyLogged,
                    Moment = DateTime.Now,
                    Content = JsonConvert.SerializeObject(
                            new ContentSendUsersAlreadyLogged()
                            {
                                AlreadyLoggedUsers =
                                    (from us in UsersBeforeAdding
                                     select us.Key).ToList()
                            })
                }));
        }

        public static void RemoveUser(ComnModel request)
        {
            var LoggofInfo = JsonConvert.DeserializeObject<ContentSendUserIsDisconnecting>(request.Content);

            var tcp = DicOfConnections[LoggofInfo.Client];
            if (DicOfConnections[LoggofInfo.Client] != null)
            {
                DicOfConnections.Remove(LoggofInfo.Client);
                WriteMessageOnStream(tcp, "");
            }

            foreach (KeyValuePair<string, TcpClient> Kvp in DicOfConnections)
            {
                WriteMessageOnStream(Kvp.Value, JsonConvert.SerializeObject(request));
            }
 
        }

        public static void SenderActionRouter(string message)
        {
            try
            {
                ComnModel MessageObj = JsonConvert.DeserializeObject<ComnModel>(message);
                switch (MessageObj.ContentAction)
                {
                    case ComnModel.Actions.SendText:
                        SendMessegeToTheAddressee(MessageObj);
                        break;

                    case ComnModel.Actions.SendUserIsDisconnecting:
                        RemoveUser(MessageObj);
                        break;

                    default:
                        Console.WriteLine("Nada pra fazer");
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: SendingMessage - " + ex.ToString());
            }  
        }

        private static void SendMessegeToTheAddressee(ComnModel messageObj)
        {
            WriteMessageOnStream
                (DicOfConnections[messageObj.Addresee],
                JsonConvert.SerializeObject(messageObj));
        }



        private static void UseMessageAsSettings(ComnModel messageObj)
        {
            throw new NotImplementedException();
        } 

        private static void SendMesseToAGroup(ComnModel messageObj)
        {
            var tcpOnGroup = (from t in DicOfGroups
                              where t.Key == messageObj.Addresee
                              select t.Value).ToList();

            foreach (TcpClient client in tcpOnGroup)
            {
                WriteMessageOnStream(client, JsonConvert.SerializeObject(messageObj));
            }
        }

        public static void WriteMessageOnStream(TcpClient tcpClient, string message)
        {
            var swSenderSender = new StreamWriter(tcpClient.GetStream());
            swSenderSender.WriteLine(message);
            swSenderSender.Flush();
        }

    }
}
