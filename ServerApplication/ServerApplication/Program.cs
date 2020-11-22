using System;
using System.Configuration;
using ServerApplication.InfoWriting;
using ServerApplication.ChatServer;
using System.Runtime.InteropServices;

namespace ServerApplication
{
    public class Program
    {
        private static Server _server;

        public static void Main()
        {
            try
            {
                Console.Title = "ServerApplication";

                SetConsoleCtrlHandler(Handler, true);

                var appSettings = ConfigurationManager.AppSettings;
                var ip = appSettings.Get("IP");
                var port = int.Parse(appSettings.Get("Port"));

                _server = new Server(ip, port);
                _server.TryDoRun();
            }
            catch (Exception e)
            {
                InfoWriter.GetInstance().Write(InfoType.Error, string.Format("Exception: {0}", e));
            }
        }

        #region CloseEventHandler

        // Тут обрабатывается событие закрытия приложения

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_CLOSE_EVENT = 2
        }

        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_C_EVENT:
                    _server.Dispose();
                    Environment.Exit(0);
                    return false;
                case CtrlType.CTRL_CLOSE_EVENT:
                    _server.Dispose();
                    Environment.Exit(0);
                    return false;
                default:
                    return false;
            }
        }

        #endregion
    }
}
