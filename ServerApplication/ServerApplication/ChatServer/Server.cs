using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ServerApplication.InfoWriting;

namespace ServerApplication.ChatServer
{
    // Небольшое пояснение по поводу клиентов и пользователей: 
    // их коллекции не пересекаются, клиент - подключенный tcp клиент без ника
    // когда от клиента приходит ник, он становится пользователем и начинает принимать сообщения от других пользователей,
    // видит всех пользователей онлайн и команды чата
    // при его отключении все остальные пользователи информируются об этом, при отключении клиента - нет
    public class Server : IDisposable
    {
        private TcpListener _tcpServer;
        private InfoWriter _infoWriter;
        private List<TcpClient> _tcpClients;

        private bool _isConnected;

        public Server(string ip, int port)
        {
            _tcpServer = new TcpListener(IPAddress.Parse(ip), port);
            _infoWriter = InfoWriter.GetInstance();
            _tcpClients = new List<TcpClient>();

            _infoWriter.Write(InfoType.Info, string.Concat("IP: ", ip, " Port: ", port));
        }

        public void TryDoRun()
        {
            try
            {
                StartTcpServer();

                while (true)
                {
                    var tcpClient = _tcpServer.AcceptTcpClient();
                    ConnectionServiceAsync(tcpClient);
                }
            }
            catch (Exception e)
            {
                if ((e is SocketException se && se.SocketErrorCode == SocketError.Interrupted) == false)
                    _infoWriter.Write(InfoType.Error, string.Format("Exception: {0}", e));
            }
            finally
            {
                StopTcpServer();
            }
        }

        public void Dispose()
        {
            var users = ChatLogic.GetAllUsers();
            var tcpClients = _tcpClients.Union(users.Select(x => x.TcpClient)).ToList();

            foreach (var tcpClient in tcpClients)
                DisconnectTcpClient(tcpClient);

            StopTcpServer();
        }

        // Асинхронное обслуживание конкретного подключения
        private void ConnectionServiceAsync(TcpClient tcpClient)
        {
            Task.Run(() =>
            {
                var user = ConnectTcpClient(tcpClient);

                if (user == null)
                {
                    DisconnectTcpClient(tcpClient);
                    return;
                }

                ChatLogic.ReceiveMessagesFromUser(user);
                DisconnectUser(user);
            });
        }

        // Обработка подключенного клиента и регистрация пользователя
        private UserModel ConnectTcpClient(TcpClient tcpClient)
        {
            var ip = tcpClient.Client.RemoteEndPoint.ToString();
            _infoWriter.Write(InfoType.Info, string.Concat(ChatConstants.Client, " ", ip, " ", ChatConstants.Connected));

            _tcpClients.Add(tcpClient);
            var username = ChatLogic.GetUsernameFromTcpClient(tcpClient);

            if (string.IsNullOrEmpty(username))
                return null;

            var user = new UserModel()
            {
                Username = username,
                TcpClient = tcpClient
            };

            if (ConnectUser(user) == false)
                return null;

            return user;
        }

        // Обработка подключенного пользователя и отправка ему всей нужной информации
        private bool ConnectUser(UserModel user)
        {
            var ok = string.Concat(ChatConstants.Separator, ChatConstants.Ok, ChatConstants.Separator);
            var isConnected = ChatLogic.SendMessageToUser(user, ok);

            if (isConnected)
            {
                var users = ChatLogic.GetAllUsers();
                string usernames = null;

                if (users.Count == 0)
                    usernames = string.Concat(ChatConstants.Separator, ChatConstants.Server, ChatConstants.Separator);
                else
                    usernames = string.Join(ChatConstants.Separator, users.Select(x => x.Username));

                isConnected = ChatLogic.SendMessageToUser(user, usernames);
            }

            if (isConnected)
            {
                var commands = string.Empty;

                foreach (ChatCommands command in Enum.GetValues(typeof(ChatCommands)))
                    commands += string.Concat(ChatConstants.Separator, command.ToName());

                isConnected = ChatLogic.SendMessageToUser(user, commands);
            }

            if (isConnected)
            {
                _tcpClients.Remove(user.TcpClient);
                ChatLogic.AddUser(user);
                ChatLogic.UserStateChanged(user.Username, true);
            }

            return isConnected;
        }

        // Отключение пользователя
        private void DisconnectUser(UserModel user)
        {
            ChatLogic.RemoveUser(user);
            ChatLogic.UserStateChanged(user.Username, false);
            DisconnectTcpClient(user.TcpClient);
        }

        // Отключение клиента
        private void DisconnectTcpClient(TcpClient tcpClient)
        {
            _tcpClients.Remove(tcpClient);
            var ip = tcpClient.Client.RemoteEndPoint.ToString();
            tcpClient.GetStream().Close();
            tcpClient.Close();
            _infoWriter.Write(InfoType.Info, string.Concat(ChatConstants.Client, " ", ip, " ", ChatConstants.Disconnected));
        }

        // Запуск сервера
        private void StartTcpServer()
        {
            if (_isConnected == false)
            {
                _isConnected = true;
                _tcpServer.Start();
                _infoWriter.Write(InfoType.Info, string.Concat(ChatConstants.Server, " started"));
            }
        }

        // Остановка сервера
        private void StopTcpServer()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _tcpServer.Stop();
                _infoWriter.Write(InfoType.Info, string.Concat(ChatConstants.Server, " stopped"));
            }
        }
    }
}
