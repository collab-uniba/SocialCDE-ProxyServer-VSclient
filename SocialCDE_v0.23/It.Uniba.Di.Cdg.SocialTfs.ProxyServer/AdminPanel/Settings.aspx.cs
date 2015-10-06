using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Settings : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);

            if (Request.RequestType == "GET")
            {
                if (!string.IsNullOrEmpty(Request.Params["username"]))
                    CheckUsername(Request.Params["username"]);
                if (!string.IsNullOrEmpty(Request.Params["email"]))
                    CheckEmail(Request.Params["email"]);
            }
            else if (Request.RequestType == "POST")
            {
                switch (Request.Params["ctl00$MainContent$SettingSE"])
                {
                    case "Admin settings":
                        ChangeAdminSettings();
                        break;
                    case "Mail settings":
                        ChangeSmtpSettings();
                        break;
                }
            }

            FillAdminSettings();
            FillSmtpSettings();
        }

        private void CheckUsername(string username)
        {
            SocialTFSEntities db = new SocialTFSEntities();
            XDocument xml = new XDocument(
                     new XElement("Root",
                         new XElement("IsAviable", !db.User.Any(u => u.username == username && !u.isAdmin))));
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(xml);
            Response.End();
        }

        private void CheckEmail(string email)
        {
            SocialTFSEntities db = new SocialTFSEntities();
            XDocument xml = new XDocument(
                     new XElement("Root",
                         new XElement("IsAviable", !db.User.Any(u => u.email == email && !u.isAdmin))));
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(xml);
            Response.End();
        }

        private void ChangeSmtpSettings()
        {
            SocialTFSEntities db = new SocialTFSEntities();

            try
            {
                bool changePassword = true;
                if (ChangeMailPasswordCB.Checked)
                    if (Request.Params["ctl00$MainContent$MailPasswordTB"].Equals(Request.Params["ctl00$MainContent$MailConfirmTB"]))
                        //db.Setting.Where(s => s.key == "MailPassword").Single().value = db.EncDecRc4("key",Request.Params["ctl00$MainContent$MailPasswordTB"]);
                        db.Setting.Where(s => s.key == "MailPassword").Single().value = Request.Params["ctl00$MainContent$MailPasswordTB"];
                    else
                    {
                        ErrorPA.Attributes.Add("class", "error");
                        ErrorPA.InnerText = "Passwords do not match.";
                        changePassword = false;
                    }

                if (changePassword)
                {
                    db.Setting.Where(s => s.key == "SmtpServer").Single().value = Request.Params["ctl00$MainContent$SmtpServerTB"];
                    db.Setting.Where(s => s.key == "SmtpPort").Single().value = Request.Params["ctl00$MainContent$SmtpPortTB"];
                    db.Setting.Where(s => s.key == "SmtpSecurity").Single().value = Request.Params["ctl00$MainContent$SmtpSecuritySE"];
                    db.Setting.Where(s => s.key == "MailAddress").Single().value = Request.Params["ctl00$MainContent$MailAddressTB"];

                    db.SaveChanges();
                    ErrorPA.Attributes.Add("class", "confirm");
                    ErrorPA.InnerText = "Data stored.";
                }
            }
            catch
            {
                try
                {
                    var list = new List<Setting>(){
                        new Setting () {
                            key = "SmtpServer",
                            value = Request.Params["ctl00$MainContent$SmtpServerTB"]
                        },
                        new Setting () {
                            key = "SmtpPort",
                            value = Request.Params["ctl00$MainContent$SmtpPortTB"]
                        },
                        new Setting () {
                            key = "SmtpSecurity",
                            value = Request.Params["ctl00$MainContent$SmtpSecuritySE"]
                        },
                        new Setting () {
                            key = "MailAddress",
                            value = Request.Params["ctl00$MainContent$MailAddressTB"]
                        },
                        new Setting () {
                            key = "MailPassword",
                            //value = db.EncDecRc4("key",Request.Params["ctl00$MainContent$MailPasswordTB"])
                            value = Request.Params["ctl00$MainContent$MailPasswordTB"]
                        }
                    };
                    foreach(Setting s in list)
                    {
                        db.Setting.AddObject(s);
                    }
                    /*
                    db.Setting.InsertAllOnSubmit(new List<Setting>(){
                        new Setting () {
                            key = "SmtpServer",
                            value = Request.Params["ctl00$MainContent$SmtpServerTB"]
                        },
                        new Setting () {
                            key = "SmtpPort",
                            value = Request.Params["ctl00$MainContent$SmtpPortTB"]
                        },
                        new Setting () {
                            key = "SmtpSecurity",
                            value = Request.Params["ctl00$MainContent$SmtpSecuritySE"]
                        },
                        new Setting () {
                            key = "MailAddress",
                            value = Request.Params["ctl00$MainContent$MailAddressTB"]
                        },
                        new Setting () {
                            key = "MailPassword",
                            value = db.EncDecRc4("key",Request.Params["ctl00$MainContent$MailPasswordTB"])
                        }
                    });
                     * */
                    db.SaveChanges();
                    ErrorPA.Attributes.Add("class", "confirm");
                    ErrorPA.InnerText = "Data stored.";
                }
                catch
                {
                    ErrorPA.Attributes.Add("class", "error");
                    ErrorPA.InnerText = "Something was wrong. Please try again later.";
                }
            }
        }

        private void FillSmtpSettings()
        {
            SocialTFSEntities db = new SocialTFSEntities();
            try
            {
                SmtpServerTB.Value = db.Setting.Where(s => s.key == "SmtpServer").Single().value;
                SmtpPortTB.Value = db.Setting.Where(s => s.key == "SmtpPort").Single().value;
                SmtpSecuritySE.Value = db.Setting.Where(s => s.key == "SmtpSecurity").Single().value;
                MailAddressTB.Value = db.Setting.Where(s => s.key == "MailAddress").Single().value;
            }
            catch { }
        }

        private void ChangeAdminSettings()
        {
            string username = Request.Params["ctl00$MainContent$AdminUsernameTB"];
            string email = Request.Params["ctl00$MainContent$AdminEmailTB"];
            string password = Request.Params["ctl00$MainContent$PasswordTB"];
            string confirm = Request.Params["ctl00$MainContent$ConfirmTB"];

            SocialTFSEntities db = new SocialTFSEntities();

            User admin = db.User.Where(u => u.isAdmin).Single();
            bool changePassword = true;

            if (ChangePasswordCB.Checked)
                if (password.Equals(confirm))
                    admin.password = (password);
                else
                {
                    ErrorPA.Attributes.Add("class", "error");
                    ErrorPA.InnerText = "Passwords do not match.";
                    changePassword = false;
                }

            if (changePassword)
            {
                if (!db.User.Any(u => (u.username == username || u.email == email) && !u.isAdmin))
                {
                    admin.username = username;
                    admin.email = email;

                    db.SaveChanges();
                    ErrorPA.Attributes.Add("class", "confirm");
                    ErrorPA.InnerText = "Data stored";
                }
                else
                {
                    ErrorPA.Attributes.Add("class", "error");
                    ErrorPA.InnerText = "Username or email already exist.";
                }
            }
        }

        private void FillAdminSettings()
        {
            SocialTFSEntities db = new SocialTFSEntities();
            User admin = db.User.Where(u => u.isAdmin).Single();
            AdminUsernameTB.Value = admin.username;
            AdminEmailTB.Value = admin.email;
        }
    }
}