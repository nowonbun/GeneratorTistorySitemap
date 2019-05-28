using System;
using System.Windows.Forms;
using System.Configuration;

namespace GeneratorTistorySitemap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Func<string, bool> checkConfiguration = (key) =>
            {
                if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[key]))
                {
                    return false;
                }
                return true;
            };
            if (checkConfiguration("changeFreg") && checkConfiguration("priority") && checkConfiguration("clientId") && checkConfiguration("port"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("설정 파일을 확인해 주세요.");
                Application.Exit();
            }
        }
    }
}
