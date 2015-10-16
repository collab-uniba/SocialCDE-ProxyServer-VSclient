using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class NewUser : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "POST")
            {
                SaveUsers();
            }
        }

        private void SaveUsers()
        {
            ConnectorDataContext db = new ConnectorDataContext();

            XmlDocument requestXml = new XmlDocument();
            requestXml.Load(new XmlTextReader(new StreamReader(Request.InputStream)));

            List<string> mailError = new List<string>();

            foreach (XmlNode item in requestXml.SelectNodes("//users/user"))
            {
                try
                {
                    String passwd = Membership.GeneratePassword(10, 2);
                    User user = new User()
                    {
                        username = item.InnerText,
                        email = item.InnerText,
                        password = db.Encrypt(passwd)
                    };
                    Stopwatch w = Stopwatch.StartNew();
                    db.Users.InsertOnSubmit(user);
                    w.Stop();
                    ILog log = LogManager.GetLogger("QueryLogger");
                    log.Info(" Elapsed time: " + w.Elapsed + ", insert the user in a pending state");

                    if (WebUtility.SendEmail(item.InnerText, "SocialCDE invitation", GetBody(item.InnerText, passwd), true))
                    {
                        Stopwatch w1 = Stopwatch.StartNew();
                        db.SubmitChanges();
                        w1.Stop();
                        ILog log1 = LogManager.GetLogger("QueryLogger");
                        log1.Info(" Elapsed time: " + w1.Elapsed + ", send mail for registration");
                    }
                    else
                        mailError.Add(item.InnerText);
                }
                catch
                {
                    mailError.Add(item.InnerText);
                }
            }

            XElement root = new XElement("Root");
            foreach (string item in mailError)
                root.Add(new XElement("NotSent", item));

            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(new XDocument(root));
            Response.End();
        }

        private String GetBody(string email, string passwd)
        {
            String body = string.Empty;
            body += "<p>You have been invited to join the SocialCDE community.</p>";
            body += "<p>You can download the latest version of the SocialCDE plugin for Visual Studio and Eclipse from this address: <a href=\"";
            body += Request.Url.AbsoluteUri.Replace("NewUser", "DownloadClient") + "\">";
            body += Request.Url.AbsoluteUri.Replace("NewUser", "DownloadClient") + "</a></p>";
            body += "<p>Sign up using the following information.</p>";
            body += "<table>";
            body += String.Format("<tr><td>Proxy server host:</td><td style=\"font-weight:bold\">http://{0}</td></tr>", Request.Url.Authority);
            body += String.Format("<tr><td>Email:</td><td style=\"font-weight:bold\">{0}</td></tr>", email);
            body += String.Format("<tr><td>Invitation code:</td><td style=\"font-weight:bold\">{0}</td></tr>", passwd);
            body += "<tr><td>Username:</td><td style=\"font-weight:bold\">&lt;choose your username&gt;</td></tr>";
            body += "<tr><td>Password:</td><td style=\"font-weight:bold\">&lt;choose your password&gt;</td></tr>";
            body += "</table>";
            body += "<br/><p>Regards,<br/>";
            body += "The SocialCDE Admin</p>";
            return body;
        }
    }
}