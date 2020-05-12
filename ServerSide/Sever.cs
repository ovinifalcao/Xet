using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using CommunicationModels;

namespace ServerSide
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class ChatServer
    {
        //vamos substituir o digitar IP de entrada por um recurso estático

        //Isso aqui poderia ser um dicionário?
        public static Dictionary<TcpClient, string> ConnectionsInfo = new Dictionary<TcpClient, string>();


        private IPAddress ipAdress;
        private TcpClient tcpClient;
        private Thread listenerProcess;
        private TcpListener tcpClientListener;

        //tenho impressão de que não precisaremos desse cógido
        bool ServRodando = false;
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;
        // #

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


        public static void EnviaMensagemAdmin(string Mensagem)
        {
            StreamWriter swSenderSender;

            // Não vamos ficar printando nada no console
            e = new StatusChangedEventArgs("Administrador: " + Mensagem);
            OnStatusChanged(e);
            //#

            //Aqui a gente vai ter que fazer uma pequena mágica de encapsulamento
            //pra mandar a mensagem só para quem se espera e não para todo mundo


            // Cria um array de clientes TCPs do tamanho do numero de clientes existentes
            TcpClient[] tcpClientes = new TcpClient[ChatServidor.htUsuarios.Count];
            // Copia os objetos TcpClient no array
            ChatServidor.htUsuarios.Values.CopyTo(tcpClientes, 0);
            // Percorre a lista de clientes TCP
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                // Tenta enviar uma mensagem para cada cliente
                try
                {
                    // Se a mensagem estiver em branco ou a conexão for nula sai...
                    if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    // Envia a mensagem para o usuário atual no laço
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine("Administrador: " + Mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Se houver um problema , o usuário não existe , então remove-o
                {
                    RemoveUsuario(tcpClientes[i]);
                }
            }
        }

        public static MessageAddressee GetAddressee(string Message)
        {
            var messageData = new MessageAddressee();
            int stopPoint = Message.IndexOf(';');

            var addresseeUsersNames =  Message.Substring(0, stopPoint -1);
            foreach (string UsName in addresseeUsersNames.Split(','))
            {
                messageData.tcp.Add(ChatServer.ConnectionsInfo.FirstOrDefault(t => t.Value == addresseeUsersNames).Key);
            }

            var senderDateTime = Message.Substring(stopPoint, 14).Split(',');
            messageData.SendMoment = new DateTime
                (
                 int.Parse(senderDateTime[0]),
                 int.Parse(senderDateTime[1]),
                 int.Parse(senderDateTime[2]),
                 int.Parse(senderDateTime[3]),
                 int.Parse(senderDateTime[4]),
                 int.Parse(senderDateTime[5])
                );

            messageData.Message = Message.Substring(addresseeUsersNames.Length + 15);

            return messageData;
        }

    }
}
