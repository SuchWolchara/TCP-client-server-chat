using System.Net.Sockets;

namespace ServerApplication.ChatServer
{
    public class UserModel
    {
        public string Username { get; set; }
        public TcpClient TcpClient { get; set; }
    }
}
