using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using CommunicationModels;
using Newtonsoft.Json;

namespace ServerSide
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class ChatServer
    {
        //vamos substituir o digitar IP de entrada por um recurso estático

        //Isso aqui poderia ser um dicionário?
        public static Dictionary<TcpClient, string> ConnectionsInfo = new Dictionary<TcpClient, string>();
        public static Dictionary<TcpClient, string> GroupsInfo = new Dictionary<TcpClient, string>();

        private IPAddress ipAdress;
        private TcpClient tcpClient;
        private Thread listenerProcess;
        private TcpListener tcpClientListener;

        //tenho impressão de que não precisaremos desse cógido
        bool ServRodando = false;
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;
        // #

        public void SetServerOnline()
        {
            try
            {
                IPAddress ipaLocal = 25;
                tcpClientListener = new TcpListener(ipaLocal, 2502);
                tcpClientListener.Start();

                ServRodando = true;
                listenerProcess = new Thread(KeepRunning);
                listenerProcess.Start();
                Console.WriteLine("Sucess: The Server is online");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: SendingMessage - " + ex.ToString());
            }
        }

        private void KeepRunning()
        {
            while (ServRodando == true)
            {
                tcpClient = tcpClientListener.AcceptTcpClient();
                Connection newConnection = new Connection(tcpClient);
            }
        }

        public static void AddUser(TcpClient tcpUser, string userName)
        {
            ChatServer.ConnectionsInfo.Add(tcpUser, userName);
            //TODO: CRIAR UMA FORMA DE MOSTRAR SISTEMATICAMENTE QUE ALGUÉM CHEGOU
            // EnviaMensagemAdmin(htConexoes[tcpUsuario] + " entrou..");
        }

        public static void RemoveUser(TcpClient tcpUser)
        {
            if (ConnectionsInfo[tcpUser] != null)
            {
                //TODO: CRIAR UMA FORMA DE MOSTRAR SISTEMATICAMENTE QUE ALGUÉM SAIU
                //EnviaMensagemAdmin(htConexoes[tcpUsuario] + " saiu...");
                ConnectionsInfo.Remove(tcpUser);
            }
        }

        //CUidado isso é um handler e ele pode estar na classe errada
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                statusHandler(null, e);
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
                (ConnectionsInfo.FirstOrDefault(C => C.Value == messageObj.Addresee).Key,
                ((ContentSendMessage)messageObj.Content).Message);
        }

        private static void UseMessageAsSettings(ComnModel messageObj)
        {
            throw new NotImplementedException();
        }

        private static void SendMesseToAGroup(ComnModel messageObj)
        {
            var tcpOnGroup = (from t in GroupsInfo
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
