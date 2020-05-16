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
        public static Dictionary<string, Byte[]> DicOfProfiles = new Dictionary<string, Byte[]>();
        public static Dictionary<string, List<string>> DicOfGroups = new Dictionary<string, List<string>>();

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

        public static void AddUser(TcpClient tcpUser, string userName, Byte[] UserProfileImg)
        {
            var ConnectionsBeforeAdding = DicOfConnections.ToList();
            var ProfilesBeforeAdding = DicOfProfiles.ToList();
            ChatServer.DicOfConnections.Add(userName, tcpUser);
            ChatServer.DicOfProfiles.Add(userName, UserProfileImg);

            foreach (KeyValuePair<string, TcpClient> Dic in ConnectionsBeforeAdding)
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
                                UserAddedPhoto = UserProfileImg
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
                                    (from us in ProfilesBeforeAdding.ToList()
                                     select new Tuple<string, Byte[]>(us.Key, us.Value)).ToList()
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
                DicOfProfiles.Remove(LoggofInfo.Client);
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

                    case ComnModel.Actions.SetGroup:
                        SetANewGroup(MessageObj);
                        break;

                    case ComnModel.Actions.SendUserIsDisconnecting:
                        RemoveUser(MessageObj);
                        break;

                    case ComnModel.Actions.SendTextGroup:
                        SendMessegeToAGroupOfAddresses(MessageObj);
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: SendingMessage - " + ex.ToString());
            }  
        }

        private static void SendMessegeToAGroupOfAddresses(ComnModel messageObj)
        {
            var MsgToGroup = JsonConvert.DeserializeObject<ContentSendTextGroup>(messageObj.Content);

            foreach (string st in DicOfGroups[MsgToGroup.GroupName])
            {
                if (st != MsgToGroup.Sender)
                {
                    SendMessegeToTheAddressee(new ComnModel()
                    {
                        Addresee = st,
                        ContentAction = ComnModel.Actions.SendText,
                        Moment = DateTime.Now,
                        Content = JsonConvert.SerializeObject(
                        new ContentSendText()
                        {
                            MessageContent = MsgToGroup.Message,
                            SenderUserName = MsgToGroup.GroupName
                        })
                    });
                }
            }
        }

        private static void SetANewGroup(ComnModel messageObj)
        {
            var GroupInfo = JsonConvert.DeserializeObject<ContentSetGroup>(messageObj.Content);

            DicOfGroups.Add(GroupInfo.GroupName, GroupInfo.ParticipantsNames);
            foreach (string st in GroupInfo.ParticipantsNames)
            {
                messageObj.Addresee = st;
                SendMessegeToTheAddressee(messageObj);
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

        public static void WriteMessageOnStream(TcpClient tcpClient, string message)
        {
            var swSenderSender = new StreamWriter(tcpClient.GetStream());
            swSenderSender.WriteLine(message);
            swSenderSender.Flush();
        }

    }
}
