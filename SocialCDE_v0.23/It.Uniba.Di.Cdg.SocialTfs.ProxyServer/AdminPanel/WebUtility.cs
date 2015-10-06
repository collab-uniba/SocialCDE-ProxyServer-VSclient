using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Net.Mail;
using System.Net;
using System.Data.Entity;

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
                SocialTFSEntities db = new SocialTFSEntities();

                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(to));
                message.From = new MailAddress(db.Setting.Where(s => s.key == "MailAddress").Single().value,"SocialTFS");
                message.Subject = subject;
                message.IsBodyHtml = isBodyHtml;
                message.Body = body;
                SmtpClient smtp = new SmtpClient(db.Setting.Where(s => s.key == "SmtpServer").Single().value, Int32.Parse(db.Setting.Where(s => s.key == "SmtpPort").Single().value));
                switch (db.Setting.Where(s => s.key == "SmtpSecurity").Single().value)
                {
                    case "None":
                        break;
                    case "SSL/TLS":
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;
                        //smtp.Credentials = new NetworkCredential(db.Setting.Where(s => s.key == "MailAddress").Single().value, db.EncDecRc4("key",db.Setting.Where(s => s.key == "MailPassword").Single().value));
                        smtp.Credentials = new NetworkCredential(db.Setting.Where(s => s.key == "MailAddress").Single().value, db.Setting.Where(s => s.key == "MailPassword").Single().value);
                        break;
                    case "STARTTLS":
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;
                        //smtp.Credentials = new NetworkCredential(db.Setting.Where(s => s.key == "MailAddress").Single().value, db.EncDecRc4("key",db.Setting.Where(s => s.key == "MailPassword").Single().value), "");
                        smtp.Credentials = new NetworkCredential(db.Setting.Where(s => s.key == "MailAddress").Single().value, db.Setting.Where(s => s.key == "MailPassword").Single().value);
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