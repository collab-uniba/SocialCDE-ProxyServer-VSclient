using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public static class WebUtility
    {

        /// <summary>
        /// Check if the user stored in the session have the necessary credentials.
        /// </summary>
        /// <param name="page">The web page that contain the session.</param>
        public static void CheckCredentials(Page page)
        {
            if (page.Session["username"] == null)
                page.Response.Redirect("Login.aspx?type=error&message=Please, login as administrator");
        }

        /// <summary>
        /// Send an email.
        /// </summary>
        /// <param name="to">Addressee.</param>
        /// <param name="subject">Sunject.</param>
        /// <param name="body">Message.</param>
        /// <param name="isBodyHtml">True if the message is wrote in HTML.</param>
        /// <returns>True if the email is correctly sended, false otherwise.</returns>
        public static bool SendEmail(String to, String subject, String body, bool isBodyHtml)
        {
            try
            {
                ConnectorDataContext db = new ConnectorDataContext();

                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(to));
                Stopwatch w = Stopwatch.StartNew();
                message.From = new MailAddress(db.Settings.Where(s => s.key == "MailAddress").Single().value, "SocialTFS");
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", select the sending mail address");
                message.Subject = subject;
                message.IsBodyHtml = isBodyHtml;
                message.Body = body;
                Stopwatch w1 = Stopwatch.StartNew();
                SmtpClient smtp = new SmtpClient(db.Settings.Where(s => s.key == "SmtpServer").Single().value, Int32.Parse(db.Settings.Where(s => s.key == "SmtpPort").Single().value));
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", select the value of 'SmtpServer' key");
                Stopwatch w2 = Stopwatch.StartNew();
                String smtpSec = db.Settings.Where(s => s.key == "SmtpSecurity").Single().value;
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", select the value of 'SmtpSecurity' key");
                switch (smtpSec)
                {
                    case "None":
                        break;
                    case "SSL/TLS":
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;
                        Stopwatch w3 = Stopwatch.StartNew();
                        smtp.Credentials = new NetworkCredential(db.Settings.Where(s => s.key == "MailAddress").Single().value, db.EncDecRc4("key", db.Settings.Where(s => s.key == "MailPassword").Single().value));
                        w3.Stop();
                        ILog log3 = LogManager.GetLogger("QueryLogger");
                        log3.Info(" Elapsed time: " + w3.Elapsed + ", select smtp credentials(SSL/TLS)");
                        break;
                    case "STARTTLS":
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;
                        Stopwatch w4 = Stopwatch.StartNew();
                        smtp.Credentials = new NetworkCredential(db.Settings.Where(s => s.key == "MailAddress").Single().value, db.EncDecRc4("key", db.Settings.Where(s => s.key == "MailPassword").Single().value), "");
                        w4.Stop();
                        ILog log4 = LogManager.GetLogger("QueryLogger");
                        log4.Info(" Elapsed time: " + w4.Elapsed + ", select smtp credentials(STARTTLS)");
                        break;
                }
                smtp.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}