using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Configuration;

namespace RecipesMVC.Core
{
    /*public class AppConfig
    {
        public Models.ConfigModel Configuration;
        private static XmlDocument configDoc = new XmlDocument();
        private Thread thread;
        public bool Stop { get { return _Stop; } }
        private bool _Stop = false;
        public AppConfig()
        {
            Configuration = new Models.ConfigModel();
            thread = new Thread(() => StartThread());
            thread.Name = "App Configuration Thread";
            thread.Priority = ThreadPriority.AboveNormal;
        }
        public void Start()
        {
            thread.Start();
        }
        private void StartThread()
        {
            while (!_Stop)
            {
                DirectoryInfo info = new DirectoryInfo(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/"));
                FileInfo[] files = info.GetFiles("*.config.xml");
                if (!(files.Length > 0))
                {
                    throw new IOException("Can't find configuration file. Make sure: (1) It's in App_Data Folder (2) It have the pattern *.config.xml (3) The program have acces to the resource.");
                }
                else
                {
                    configDoc.Load(files[0].FullName);
                    XmlNodeList configNodes = configDoc.SelectNodes("//config/child::*");
                    //TODO: Assign To Configuartion Shit.
                    Configuration = new Models.ConfigModel()
                    {
                        RefreshTimeout = 10
                    }
                }
                Thread.Sleep(Configuration.RefreshTimeout);
            }
        }
        public void Dispose()
        {
            _Stop = true;
        }
    }*/

    public class EmailMessaging : IDisposable
    {
        public MailAddress DestinationAddr { get; set; }
        protected MailAddress SenderAddress = new MailAddress(ConfigurationManager.AppSettings["SmtpUser"]);
        public MailMessage msg { get; set; }
        private SmtpClient client;
        public enum MessageType
        {
            ResetPassword,
            Test
        }
        public EmailMessaging(string address, MessageType type, string[] MessageData)
        {
            DestinationAddr = new MailAddress(address);
            client = new SmtpClient()
            {
                Host = ConfigurationManager.AppSettings["SmtpServer"],
                Port = 25,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SmtpUser"], ConfigurationManager.AppSettings["SmtpPassword"])
            };
            msg = new MailMessage(SenderAddress, DestinationAddr);
            msg.Subject = "Hi " + MessageData[1] + ", Here's You reset password link";
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = true;
            string b = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Mails/ResetPwd/ResetPwd.html")).Replace("{WebSiteDomain}", MessageData[2]).Replace("{UserName}", MessageData[1]).Replace("{ResetGUIDcode}", MessageData[0]);
            msg.Body = b;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
        }
        public void Send()
        {
            client.Send(msg);
        }
        public void SendAsyncAndDispose()
        {
            Task.Run(() => Send());
        }
        public void Dispose()
        {
            client.Dispose();
            msg.Dispose();
        }
    }
}