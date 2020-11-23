using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApplication.ChatClient
{
    public class Client : IDisposable
    {
        private TcpClient _tcpClient;
        private ClientForm _clientForm;

        private string _ip;
        private int _port;

        private bool _isConnected;

        public Client(ClientForm clientForm, string ip, int port)
        {
            _tcpClient = new TcpClient();
            _clientForm = clientForm;

            _ip = ip;
            _port = port;

            _isConnected = false;

            SignEvents();
        }

        public void TryDoRun()
        {
            try
            {
                ConnectTcpClient();
                ConnectionServiceAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Dispose()
        {
            UnsignEvents();
            DisconnectTcpClient(true);
        }

        // Асинхронное обслуживание подключения
        private void ConnectionServiceAsync()
        {
            Task.Run(() =>
            {
                ChatLogic.SetUsername();
                _clientForm.ClearMessages();

                var data = ChatLogic.GetOnlineUsers();
                if (string.IsNullOrEmpty(data) == false)
                    SetOnlineUsers(data);

                data = ChatLogic.GetChatCommands();
                if (string.IsNullOrEmpty(data) == false)
                    SetChatCommands(data);

                ChatLogic.ReceiveMessages();
                DisconnectTcpClient();
            });
        }

        private void SignEvents()
        {
            _clientForm.ButtonSendClicked += ClientForm_ButtonSendClicked;
            _clientForm.ButtonConnectClicked += ClientForm_ButtonConnectClicked;
            _clientForm.ChatCommandClicked += ClientForm_ChatCommandClicked;

            ChatLogic.SetMessage += ChatLogic_SetMessage;
            ChatLogic.Disconnect += ChatLogic_Disconnect;

            Application.ApplicationExit += Application_ApplicationExit;
        }

        private void UnsignEvents()
        {
            _clientForm.ButtonSendClicked -= ClientForm_ButtonSendClicked;
            _clientForm.ButtonConnectClicked -= ClientForm_ButtonConnectClicked;
            _clientForm.ChatCommandClicked -= ClientForm_ChatCommandClicked;

            ChatLogic.SetMessage -= ChatLogic_SetMessage;
            ChatLogic.Disconnect -= ChatLogic_Disconnect;

            Application.ApplicationExit -= Application_ApplicationExit;
        }

        private void ClientForm_ButtonSendClicked(string data, bool e)
        {
            if (data.Contains(ChatConstants.Separator))
            {
                MessageBox.Show("Your message has invalid symbols!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ChatLogic.SendMessage(data);
        }

        private void ClientForm_ButtonConnectClicked()
        {
            if (_isConnected)
                DisconnectTcpClient();
            else
                TryDoRun();
        }

        private void ClientForm_ChatCommandClicked(string data, bool e)
        {
            ChatLogic.SendMessage(data);
        }

        private void ChatLogic_SetMessage(string data, bool clear)
        {
            if (clear)
                _clientForm.ClearMessages();
            else
                TrySetOnlineUser(data);

            _clientForm.SetMessage(data);
        }

        private void ChatLogic_Disconnect()
        {
            DisconnectTcpClient();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            Dispose();
        }

        // Установка онлайн пользователей
        private void SetOnlineUsers(string data)
        {
            var sep = string.Concat(ChatConstants.Separator, ChatConstants.Server, ChatConstants.Separator);

            if (data.Contains(sep) == false)
            {
                var usernames = data.Split(ChatConstants.Separator, StringSplitOptions.RemoveEmptyEntries);
                _clientForm.SetOnlineUsers(usernames);
            }
        }

        // Установка команд чата
        private void SetChatCommands(string data)
        {
            var commands = data.Split(ChatConstants.Separator, StringSplitOptions.RemoveEmptyEntries);
            _clientForm.SetChatCommands(commands);
        }

        // Поиск в полученном сообщении информации об изменении состояния подключения какого нибудь пользователя
        private void TrySetOnlineUser(string data)
        {
            var sep = string.Concat(ChatConstants.Separator, ChatConstants.Server, ChatConstants.Separator);

            if (data.Contains(sep) && (data.Contains(ChatConstants.Connected) || data.Contains(ChatConstants.Disconnected)))
            {
                var str = data.Split(sep);
                var username = str[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                _clientForm.SetOnlineUser(username);
            }
        }

        // Установка соединения
        private void ConnectTcpClient()
        {
            if (_isConnected == false)
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(_ip, _port);
                ChatLogic.SetTcpClient(_tcpClient);
                _isConnected = true;
                _clientForm.SetIsConnected(_isConnected);
            }
        }

        // Разрыв соединения
        private void DisconnectTcpClient(bool disposing = false)
        {
            if (_isConnected)
            {
                _tcpClient.GetStream().Close();
                _tcpClient.Close();
                ChatLogic.SetTcpClient(null);
                _isConnected = false;
                if (disposing == false)
                    _clientForm.SetIsConnected(_isConnected);
            }
        }
    }
}
