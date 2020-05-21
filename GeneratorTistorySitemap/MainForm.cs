using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

namespace GeneratorTistorySitemap
{
    public partial class Form1 : Form
    {
        private List<Blog> blogs = null;
        private String token = null;
        private String changeFreg = null;
        private String priority = null;
        private String client_id = null;
        private int port = 0;
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = System.IO.Directory.GetCurrentDirectory() + "\\sitemap.xml";
            this.changeFreg = ConfigurationManager.AppSettings["changeFreg"];
            this.priority = ConfigurationManager.AppSettings["priority"];
            this.client_id = ConfigurationManager.AppSettings["clientId"];
            try
            {
                port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
                if (port == 0)
                {
                    port = 31233;
                }
            }
            catch
            {
                port = 31233;
            }
            if (String.IsNullOrEmpty(priority) || String.IsNullOrEmpty(changeFreg) || String.IsNullOrEmpty(client_id))
            {
                MessageBox.Show("The Configuration was wrong.");
                this.Close();
                Application.Exit();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var form = new AuthForm(this.port, this.client_id);
            form.ShowDialog();

            this.token = form.Result;
            if (String.IsNullOrEmpty(this.token))
            {
                return;
            }
            int pos = token.IndexOf("#access_token=") + 14;
            int epos = token.IndexOf("&", pos);
            if (epos < 0)
            {
                epos = token.Length;
            }
            this.token = token.Substring(pos, epos - pos);
            String json = GetRequest("https://www.tistory.com/apis/blog/info", new Dictionary<String, Object>()
            {
                { "access_token", token},
                { "output", "json" }
            });
            var info = JsonConvert.DeserializeObject<JsonInfo>(json);

            this.blogs = info?.Tistory?.Item?.Blogs;
            if (this.blogs != null)
            {
                InvokeControl(comboBox1, () =>
                {
                    foreach (var blog in blogs)
                    {
                        comboBox1.Items.Add(blog.Name);
                    }
                    comboBox1.SelectedIndex = 0;
                    comboBox1.Enabled = true;
                });
                InvokeControl(button1, () =>
                {
                    button1.Enabled = false;
                });
                InvokeControl(button2, () =>
                {
                    button2.Enabled = true;
                });
            }
        }

        private string GetRequest(String url, IDictionary<String, Object> param = null)
        {
            string paramStr = param != null ? CombineParameter(param) : null;
            url += (url.IndexOf("?") != -1) ? "&" : "?" + paramStr;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = HttpMethod.Get.ToString();
            request.ContentType = "application/x-www-form-urlencoded";
            // 프록시가 있을 때는 사용
            //request.Proxy = new WebProxy("****", ****);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private string CombineParameter(IDictionary<string, Object> param)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (String key in param.Keys)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append("&");
                }
                buffer.Append(key).Append("=").Append(param[key]);
            }
            return buffer.ToString();
        }

        private static void InvokeControl(Control ctl, Action func)
        {
            lock (ctl)
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

        private void button2_Click(object sender, EventArgs e)
        {
            InvokeControl(comboBox1, () =>
            {
                comboBox1.Enabled = false;
            });
            InvokeControl(button1, () =>
            {
                button1.Enabled = false;
            });
            InvokeControl(button2, () =>
            {
                button2.Enabled = false;
            });
            InvokeControl(button3, () =>
            {
                button3.Enabled = false;
            });
            ThreadPool.QueueUserWorkItem((c) =>
            {
                Blog blog = null;
                InvokeControl(comboBox1, () =>
                {
                    blog = this.blogs[comboBox1.SelectedIndex];
                });
                int page = 1;
                int count = 0;
                int total = 1;
                var posts = new List<Post>();

                while (count * (page - 1) < total)
                {
                    String json = GetRequest("https://www.tistory.com/apis/post/list", new Dictionary<String, Object>()
                    {
                        { "access_token", token},
                        { "output", "json" },
                        { "blogName", blog.Name },
                        { "page",page }
                    });

                    var info = JsonConvert.DeserializeObject<JsonInfo>(json);
                    if (info?.Tistory?.Item?.Posts == null || info?.Tistory?.Item?.Posts.Count < 1)
                    {
                        break;
                    }
                    page = info.Tistory.Item.Page;
                    count = info.Tistory.Item.Count;
                    total = info.Tistory.Item.TotalCount;
                    posts.AddRange(info?.Tistory?.Item?.Posts);
                    page++;
                    InvokeControl(progressBar1, () =>
                    {
                        progressBar1.Maximum = total;
                        int progress = (page - 1) * count;
                        progressBar1.Value = total > progress ? progress : total;
                    });
                }
                CreateSitemap(textBox1.Text, posts);
                InvokeControl(progressBar1, () =>
                {
                    progressBar1.Maximum = total;
                    progressBar1.Value = total;
                });
                MessageBox.Show("Completed successfully!");
            });
        }
        private void CreateSitemap(String savePath, List<Post> list)
        {
            StringBuilder sitemap = new StringBuilder();
            sitemap.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (var post in list.Where(x => x.Visibility == 20).OrderByDescending(x=> x.Date))
            {
                sitemap.Append("<url>");
                sitemap.Append("<loc>");
                sitemap.Append(post.Url);
                sitemap.Append("</loc>");
                sitemap.Append("<lastmod>");
                sitemap.Append(post.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
                sitemap.Append("</lastmod>");
                sitemap.Append("<changefreq>");
                switch (this.changeFreg)
                {
                    case "1": sitemap.Append("never"); break;
                    case "2": sitemap.Append("yearly"); break;
                    case "3": sitemap.Append("monthly"); break;
                    case "4": sitemap.Append("weekly"); break;
                    case "5": sitemap.Append("daily"); break;
                    case "6": sitemap.Append("hourly"); break;
                    default: sitemap.Append("always"); break;
                }

                sitemap.Append("</changefreq>");
                sitemap.Append("<priority>");
                switch (this.priority)
                {
                    case "1": sitemap.Append("0.1"); break;
                    case "2": sitemap.Append("0.2"); break;
                    case "3": sitemap.Append("0.3"); break;
                    case "4": sitemap.Append("0.4"); break;
                    case "5": sitemap.Append("0.5"); break;
                    case "6": sitemap.Append("0.6"); break;
                    case "7": sitemap.Append("0.7"); break;
                    case "8": sitemap.Append("0.8"); break;
                    case "9": sitemap.Append("0.9"); break;
                    case "10": sitemap.Append("1.0"); break;
                    default: sitemap.Append("0.0"); break;
                }

                sitemap.Append("</priority>");
                sitemap.Append("</url>");
            }
            sitemap.Append("</urlset>");
            byte[] data = Encoding.UTF8.GetBytes(sitemap.ToString());
            using (FileStream writer = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                writer.Write(data, 0, data.Length);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://nowonbun.tistory.com");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath + "sitemap.xml";
            }
        }
    }
}
