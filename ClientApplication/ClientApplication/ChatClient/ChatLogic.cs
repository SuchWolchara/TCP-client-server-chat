using System.Net.Sockets;
using System.Text;

namespace ClientApplication.ChatClient
{
    public static class ChatLogic
    {
        public delegate void ConnectionEvent();
        public static event ConnectionEvent Disconnect;

        public delegate void ChatEvent(string data, bool e);
        public static event ChatEvent SetMessage;

        private static TcpClient _tcpClient;

        public static void SetTcpClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        // Отправка сообщения
        public static void SendMessage(string message)
        {
            if (_tcpClient != null)
            {
                var stream = _tcpClient.GetStream();
                var msg = Encoding.Unicode.GetBytes(message);
                stream.Write(msg, 0, msg.Length);
            }
        }

        // Установка ника при подключении, в конце ожидается ответ от сервера об успешном подключении
        public static void SetUsername()
        {
            if (_tcpClient != null)
            {
                var stream = _tcpClient.GetStream();
                var buffer = new byte[2048];
                string data = null;
                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.Unicode.GetString(buffer, 0, i);

                    if (IsOkMessage(data))
                    {
                        SendOkMessage();
                        break;
                    }

                    SetMessage?.Invoke(data, true);
                }

                if (NeedDisconnect(i))
                    Disconnect?.Invoke();
            }
        }

        // Получение всех онлайн пользователей
        public static string GetOnlineUsers()
        {
            if (_tcpClient != null)
            {
                var stream = _tcpClient.GetStream();
                var buffer = new byte[2048];
                var i = stream.Read(buffer, 0, buffer.Length);

                if (NeedDisconnect(i))
                {
                    Disconnect?.Invoke();
                    return string.Empty;
                }

                var data = Encoding.Unicode.GetString(buffer, 0, i);
                SendOkMessage();

                return data;
            }

            return string.Empty;
        }

        // Получение всех команд чата
        public static string GetChatCommands()
        {
            if (_tcpClient != null)
            {
                var stream = _tcpClient.GetStream();
                var buffer = new byte[2048];
                var i = stream.Read(buffer, 0, buffer.Length);

                if (NeedDisconnect(i))
                {
                    Disconnect?.Invoke();
                    return string.Empty;
                }

                var data = Encoding.Unicode.GetString(buffer, 0, i);
                SendOkMessage();

                return data;
            }

            return string.Empty;
        }

        // Бесконечный цикл получения сообщений
        public static void ReceiveMessages()
        {
            if (_tcpClient != null)
            {
                var stream = _tcpClient.GetStream();
                var buffer = new byte[2048];
                string data = null;
                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.Unicode.GetString(buffer, 0, i);
                    SetMessage?.Invoke(data, false);
                }
            }
        }

        // Проверка ответа от сервера
        private static bool IsOkMessage(string data)
        {
            var ok = string.Concat(ChatConstants.Separator, ChatConstants.Ok, ChatConstants.Separator);
            return string.Compare(data, ok, true) == 0;
        }

        // Отправка ответа серверу
        private static void SendOkMessage()
        {
            var message = string.Concat(ChatConstants.Separator, ChatConstants.Ok, ChatConstants.Separator);
            SendMessage(message);
        }

        // Проверка соединения
        private static bool NeedDisconnect(int i)
        {
            return i == 0;
        }
    }
}
