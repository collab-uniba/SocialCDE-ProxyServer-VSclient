using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.UI;
using System.Web.Security;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class PasswordRecovering : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            String token = Request.QueryString["token"];
            Setting recoveringToken = null;
            Setting recoveringTime = null;

            try
            {
                Stopwatch w = Stopwatch.StartNew();
                recoveringTime = db.Settings.Where(s => s.key == "RecoveringTime").Single();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", select the 'recovering time' key from settings");
                Stopwatch w1 = Stopwatch.StartNew();
                recoveringToken = db.Settings.Where(s => s.key == "RecoveringToken").Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", select the 'recovering token' key from settings");
            }
            catch { }

            if (Request.RequestType == "GET")
            {
                if (String.IsNullOrEmpty(token))
                {
                    if (recoveringTime == null || DateTime.Parse(recoveringTime.value) < DateTime.UtcNow - new TimeSpan(0, 5, 0))
                    {
                        String newToken = GenerateToken();

                        Stopwatch w2 = Stopwatch.StartNew();
                        String to = db.Users.Where(u => u.isAdmin).Single().email;
                        w2.Stop();
                        ILog log2 = LogManager.GetLogger("QueryLogger");
                        log2.Info(" Elapsed time: " + w2.Elapsed + ", select the admin's email");

                        if (WebUtility.SendEmail(to, "Password recovering", GetBody(newToken), true))
                        {
                            if (recoveringToken != null)
                            {
                                recoveringToken.value = newToken;
                                recoveringTime.value = DateTime.UtcNow.ToString();
                            }
                            else
                            {
                                Stopwatch w3 = Stopwatch.StartNew();
                                db.Settings.InsertAllOnSubmit(
                                    new List<Setting>(){
                                new Setting () {
                                    key = "RecoveringToken",
                                    value = newToken
                                },
                                new Setting () {
                                    key = "RecoveringTime",
                                    value = DateTime.UtcNow.ToString()
                                }});
                                w3.Stop();
                                ILog log3 = LogManager.GetLogger("QueryLogger");
                                log3.Info(" Elapsed time: " + w3.Elapsed + ", insert new setting in a pending state(password recovering)");
                            }
                            Stopwatch w4 = Stopwatch.StartNew();
                            db.SubmitChanges();
                            w4.Stop();
                            ILog log4 = LogManager.GetLogger("QueryLogger");
                            log4.Info(" Elapsed time: " + w4.Elapsed + ", insert new settings");
                            Response.Redirect("Login.aspx?type=confirm&message=Email sent, check your email inbox.");
                        }
                        else
                            Response.Redirect("Login.aspx?type=error&message=Is not possible recover the password, the smtp server is not set.");
                    }
                    else
                        Response.Redirect("Login.aspx?type=error&message=You have sent a request less than 5 minutes ago. Please, try again later.");
                }
                else
                {
                    if (recoveringToken == null || recoveringToken.value != token)
                        Response.Redirect("Login.aspx?type=error&message=Wrong token.");
                }
            }
            else if (Request.RequestType == "POST")
            {
                Stopwatch w5 = Stopwatch.StartNew();
                db.Users.Where(u => u.isAdmin).Single().password = db.Encrypt(Request.Params["ctl00$MainContent$PasswordTB"]);
                w5.Stop();
                ILog log5 = LogManager.GetLogger("QueryLogger");
                log5.Info(" Elapsed time: " + w5.Elapsed + ", select admin's password");
                Stopwatch w6 = Stopwatch.StartNew();
                db.Settings.DeleteAllOnSubmit(db.Settings.Where(s => s.key == "RecoveringToken" || s.key == "RecoveringTime"));
                db.SubmitChanges();
                w6.Stop();
                ILog log6 = LogManager.GetLogger("QueryLogger");
                log6.Info(" Elapsed time: " + w6.Elapsed + ", password changed");
                Response.Redirect("Login.aspx?type=confirm&message=Password changed successfully.");
            }
        }

        private string GenerateToken()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[50];
            Random random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new String(stringChars);
        }

        private String GetBody(string token)
        {
            String body = string.Empty;
            body += "<p>SocialTFS has received a request for password recovery.</p>";
            body += "<p>At this address you can find the password recovery service:<br/>";
            body += String.Format("<a href=\"{0}\">{0}</a></p>", Request.Url.AbsoluteUri + "?token=" + token);
            body += "<p>If you did not request the password recovery, please ignore this message.</p>";
            body += "<p>Regards,<br/>";
            body += "SocialTFS Admin</p>";
            return body;
        }
    }
}