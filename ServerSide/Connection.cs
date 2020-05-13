using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SharedCommunication;
using Newtonsoft.Json;

namespace ServerSide
{
    public class Connection
    {
        TcpClient tcpClient;
        private Thread thrSender;
        private StreamReader streamReciver;
        private StreamWriter streamSender;
        private string CurrentUser;
        private string Resquest;

        public Connection(TcpClient tcpCon)
        {
            this.tcpClient = tcpCon;
            this.thrSender = new Thread(AcceptClient);
            thrSender.Start();
        }

        private void AcceptClient()
        {
            streamReciver = new StreamReader(tcpClient.GetStream());
            streamSender = new StreamWriter(tcpClient.GetStream());

            CurrentUser = streamReciver.ReadLine();

            var objMsg = new ComnModel()
            {
                Moment = DateTime.Now,
                Addresee = CurrentUser,
                ContentAction = ComnModel.Actions.SendConnectionSuccessful,
                Content = null
            };

            if (!string.IsNullOrEmpty(CurrentUser) &&
                ChatServer.DicOfConnections.ContainsValue(CurrentUser) == false)
            {
                ChatServer.WriteMessageOnStream(
                    tcpClient, JsonConvert.SerializeObject(objMsg));
                WaitForMessege();
            }
            else
            {
                objMsg.ContentAction = ComnModel.Actions.SendError;
                objMsg.Content = new ContentSendError() { errorCod = 601 };
              
                ChatServer.WriteMessageOnStream(
                    tcpClient, JsonConvert.SerializeObject(objMsg));

                CloseConnection();
            }
        }

        private void CloseConnection()
        {
            tcpClient.Close();
            streamReciver.Close();
            streamSender.Close();
        }

        private void WaitForMessege()
        {
            try
            {
                Resquest = streamReciver.ReadLine();
                while (!string.IsNullOrEmpty(Resquest))
                {
                    ChatServer.SenderActionRouter(Resquest);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error - Waiting for messages: " + ex.ToString());
                //remover usuário???
            }
        }

    }
}
