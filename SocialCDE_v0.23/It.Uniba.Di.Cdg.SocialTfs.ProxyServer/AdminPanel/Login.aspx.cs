using System;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Clear();

            string username = Request.Form["ctl00$MainContent$UsernameTB"];
            string password = Request.Form["ctl00$MainContent$PasswordTB"];

            if (username != null)
            {
                SocialTFSEntities db = new SocialTFSEntities();
                if (Signup(db, username, password))
                {
                    Session["Username"] = username;
                    if (db.Setting.Any(s => s.key == "SmtpServer"))
                        Response.Redirect("Default.aspx");
                    else
                        Response.Redirect("Settings.aspx");
                }
                else
                {
                    errorLB.Attributes.Add("class", "error");
                    errorLB.InnerText = "The username or the password is not correct";
                }
            }
            else if (Request.QueryString["type"] == "error")
            {
                errorLB.Attributes.Add("class", "error");
                errorLB.InnerHtml = Request.QueryString["message"];
            }
            else if (Request.QueryString["type"] == "confirm")
            {
                errorLB.Attributes.Add("class", "confirm");
                errorLB.InnerText = Request.QueryString["message"];
            }
        }

        private bool Signup(SocialTFSEntities db, string username, string password)
        {
            
            IEnumerable<User> users = db.User.Where(u => u.isAdmin && u.username == username && u.password == (password));

            if (users.Count() >= 1)
                return true;
            else
                return false;
        }
    }
}