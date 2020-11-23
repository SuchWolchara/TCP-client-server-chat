using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServerApplication.ChatServer
{
    public static class ChatLogic
    {
        private static Dictionary<ChatCommands, string> ChatCommandAnswersDict = CreateChatCommandAnswersDict();
        private static List<UserModel> _users = new List<UserModel>();

        public static void AddUser(UserModel user)
        {
            _users.Add(user);
        }

        public static void RemoveUser(UserModel user)
        {
            _users.Remove(user);
        }

        public static List<UserModel> GetAllUsers()
        {
            return new List<UserModel>(_users);
        }

        // Получаем ник подключившегося клиента
        public static string GetUsernameFromTcpClient(TcpClient tcpClient)
        {
            var reg = new Regex("^[а-яА-Яa-zA-Z0-9]*$");

            var message = "Enter your username";
            var msg = Encoding.Unicode.GetBytes(message);
            SendMessageToTcpClient(tcpClient, msg);

            var stream = tcpClient.GetStream();
            var buffer = new byte[2048];
            string data = null;
            int i;

            while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                message = null;
                data = Encoding.Unicode.GetString(buffer, 0, i);

                if (reg.IsMatch(data) == false)
                    message = "Invalid symbols";

                if (_users.Select(x => x.Username).Contains(data))
                    message = "Existing user";

                if (string.IsNullOrEmpty(message))
                    break;

                data = null;
                message += ". Please, try again";
                msg = Encoding.Unicode.GetBytes(message);
                SendMessageToTcpClient(tcpClient, msg);
            }

            return data ?? string.Empty;
        }

        // Отправляем сообщение пользователю и получаем ответ от него, если ответа нет, то отключаем его
        public static bool SendMessageToUser(UserModel user, string message)
        {
            var msg = Encoding.Unicode.GetBytes(message);
            SendMessageToTcpClient(user.TcpClient, msg);
            return ReceiveOkMessageFromTcpClient(user.TcpClient);
        }

        // Бесконечный цикл получания сообщений от конкретного пользователя
        public static void ReceiveMessagesFromUser(UserModel user)
        {
            var stream = user.TcpClient.GetStream();
            var buffer = new byte[2048];
            string data = null;
            int i;

            while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                data = Encoding.Unicode.GetString(buffer, 0, i);

                SendMessageToAllUsers(user.Username, data);

                if (AnswerToChatCommand(user, data) == false)
                {
                    Task.Delay(3000).Wait();
                    break;
                }
            }
        }

        // Рассылка сообщения об изменении статуса подключения пользователя
        public static void UserStateChanged(string username, bool connected)
        {
            var state = connected ? ChatConstants.Connected : ChatConstants.Disconnected;
            var message = string.Concat("User ", username, " ", state);
            SendMessageToAllUsers(ChatConstants.Server, message);
        }

        // Рассылка сообщения всем пользователям
        private static void SendMessageToAllUsers(string username, string message)
        {
            var now = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var fullMessage = string.Concat(now, ChatConstants.Separator, username, ChatConstants.Separator, message);
            var msg = Encoding.Unicode.GetBytes(fullMessage);
            var tasks = new List<Task>();
            foreach (var user in _users)
                tasks.Add(Task.Run(() => SendMessageToTcpClient(user.TcpClient, msg)));
            Task.WaitAll(tasks.ToArray());
        }

        // Отправка сообщения клиенту
        private static void SendMessageToTcpClient(TcpClient tcpClient, byte[] msg)
        {
            var stream = tcpClient.GetStream();
            stream.Write(msg, 0, msg.Length);
        }

        // Ожидание получения ответа от пользователя
        private static bool ReceiveOkMessageFromTcpClient(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            var buffer = new byte[2048];
            var i = stream.Read(buffer, 0, buffer.Length);
            var data = Encoding.Unicode.GetString(buffer, 0, i);
            return IsOkMessage(data);
        }

        // Проверка ответа от пользователя на совпадение с шаблоном
        private static bool IsOkMessage(string data)
        {
            var ok = string.Concat(ChatConstants.Separator, ChatConstants.Ok, ChatConstants.Separator);
            return string.Compare(data, ok, true) == 0;
        }

        // Сравнение сообщения от пользователя с командами чата и ответ на них
        private static bool AnswerToChatCommand(UserModel user, string data)
        {
            foreach (ChatCommands command in Enum.GetValues(typeof(ChatCommands)))
            {
                if (string.Compare(data, command.ToName(), true) == 0)
                {
                    var message = string.Format(ChatCommandAnswersDict[command], user.Username);
                    SendMessageToAllUsers(ChatConstants.Server, message);

                    if (command == ChatCommands.Bye)
                        return false;

                    return true;
                }
            }

            return true;
        }

        private static Dictionary<ChatCommands, string> CreateChatCommandAnswersDict()
        {
            var dict = new Dictionary<ChatCommands, string>();

            dict[ChatCommands.Bye] = "Bye, {0}";
            dict[ChatCommands.HelloWorld] = "Hello, {0}!";
            dict[ChatCommands.HowAreYou] = "I'm fine, thanks";
            dict[ChatCommands.WhatIsYourName] = "My name is Server";
            dict[ChatCommands.YouAreBreathtaking] = "No, you are breathtaking!";

            return dict;
        }
    }
}
