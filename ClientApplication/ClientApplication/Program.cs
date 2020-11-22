using ClientApplication.ChatClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace ClientApplication
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                var ip = appSettings.Get("IP");
                var port = int.Parse(appSettings.Get("Port"));

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var clientForm = new ClientForm();
                var client = new Client(clientForm, ip, port);
                Application.Run(clientForm);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
