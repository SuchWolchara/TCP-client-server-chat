using NLog;
using ServerApplication.ChatServer;
using System;

namespace ServerApplication.InfoWriting
{
    public class InfoWriter
    {
        private Logger _logger;
        private static InfoWriter _instance;

        private InfoWriter()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public static InfoWriter GetInstance()
        {
            if (_instance == null)
                _instance = new InfoWriter();

            return _instance;
        }

        // Пишем информацию в консоль и лог (ServerApplication.log, находится в папке сборки)
        public void Write(InfoType infoType, string message)
        {
            var now = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var type = infoType.ToName();

            Console.WriteLine(string.Concat(now, ChatConstants.Separator, type, ChatConstants.Separator, message));
            
            switch (infoType)
            {
                case InfoType.Info:
                    _logger.Info(message);
                    break;
                case InfoType.Error:
                    _logger.Error(message);
                    break;
                default:
                    break;
            }
        }
    }
}
