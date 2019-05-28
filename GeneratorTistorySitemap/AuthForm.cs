using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace GeneratorTistorySitemap
{
    public partial class AuthForm : Form
    {
        private WebBrowser webbrowser;
        private int port = 9999;
        private string client_id = "";
        public String Result
        {
            private set;
            get;
        }

        public AuthForm(int port, string client_id)
        {
            this.port = port;
            this.client_id = client_id;
            Result = "";
            InitializeComponent();
            webbrowser = new WebBrowser();
            webbrowser.Dock = DockStyle.Fill;
            this.Controls.Add(webbrowser);
            WaitingRedirect();
            webbrowser.Navigate("https://www.tistory.com/oauth/authorize?client_id=" + client_id + "&redirect_uri=http://localhost:" + port + "&response_type=token");
            webbrowser.DocumentCompleted += Webbrowser_DocumentCompleted;
        }

        private void Webbrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            String buffer = webbrowser.Url.ToString();
            if(buffer.IndexOf("#access_token=") > 0)
            {
                this.Result = buffer;
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (String.IsNullOrEmpty(Result))
            {
                try
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
                    {
                        socket.Connect("127.0.0.1", port);
                    }
                }
                catch { }
            }
        }

        private void WaitingRedirect()
        {
            ThreadPool.QueueUserWorkItem((c) =>
            {
                try
                {
                    using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
                    {
                        server.Bind(new IPEndPoint(IPAddress.Any, port));
                        server.Listen(1);
                        using (Socket client = server.Accept())
                        {
                            byte[] data = new byte[1024];
                            client.Receive(data, 1024, SocketFlags.None);
                            client.Send(Encoding.Default.GetBytes("\r\n"));
                            client.Send(Encoding.Default.GetBytes("\r\n"));
                            client.Close();
                        }
                    }
                }
                catch { }
            });
        }
        private static void InvokeControl(Control ctl, Action func)
        {
            if (ctl.InvokeRequired)
            {
                ctl.Invoke(func);
            }
            else
            {
                func();
            }
        }
    }
}