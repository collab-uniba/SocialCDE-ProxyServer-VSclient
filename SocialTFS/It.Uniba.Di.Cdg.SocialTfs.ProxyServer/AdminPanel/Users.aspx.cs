using System;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Web.UI;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Users : Page
    {
        private int userPerPage = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "GET")
            {
                if (!String.IsNullOrEmpty(Request.QueryString["page"]) && Int32.Parse(Request.QueryString["page"]) > 0)
                    LoadPage(Int32.Parse(Request.QueryString["page"]));
                else
                    LoadPage(1);
            }
            else if (Request.RequestType == "POST")
                DeleteUser(Int32.Parse(Request.Params["id"]));
        }

        private void LoadPage(int page)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            Stopwatch w = Stopwatch.StartNew();
            var users =
                (from usr in db.Users
                 where !usr.isAdmin
                 orderby usr.username
                 select usr).Skip((page - 1) * userPerPage).Take(userPerPage);
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", select all users");

            Stopwatch w11 = Stopwatch.StartNew();
            bool utente=users.Any();
            w11.Stop();
            ILog log11 = LogManager.GetLogger("QueryLogger");
            log11.Info(" Elapsed time: " + w11.Elapsed + ", check if the select user query returns at least one user");
            if (!utente && page > 1)
                Response.Redirect("Users.aspx?page=" + (page - 1).ToString());

            foreach (var item in users)
            {
                HtmlTableCell username = new HtmlTableCell();
                HtmlTableCell email = new HtmlTableCell();
                HtmlTableCell active = new HtmlTableCell();
                HtmlTableCell statuses = new HtmlTableCell();
                HtmlTableCell followings = new HtmlTableCell();
                HtmlTableCell followers = new HtmlTableCell();
                HtmlTableCell delete = new HtmlTableCell();

                username.InnerText = item.username;
                email.InnerText = item.email;
                HtmlImage img = new HtmlImage();
                img.Alt = item.active.ToString();
                img.Src = item.active ? "Images/yes.png" : "Images/no.png";
                active.Attributes.Add("class", "center");
                active.Controls.Add(img);
                statuses.Attributes.Add("class", "center");
                Stopwatch w1 = Stopwatch.StartNew();
                statuses.InnerText = db.Posts.Where(p => p.ChosenFeature.user == item.id).Count().ToString();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", count the number of posts of an user");
                followings.Attributes.Add("class", "center");
                Stopwatch w2 = Stopwatch.StartNew();
                followings.InnerText = db.StaticFriends.Where(sf => sf.User == item).Count().ToString();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", count the number of static friend of an user");
                followers.Attributes.Add("class", "center");
                Stopwatch w3 = Stopwatch.StartNew();
                followers.InnerText = db.StaticFriends.Where(sf => sf.Friend == item).Count().ToString();
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", count the number of users friend of an user");

                HtmlInputButton deleteBT = new HtmlInputButton();
                deleteBT.Attributes.Add("title", "Delete " + item.username);
                deleteBT.Attributes.Add("class", "delete");
                deleteBT.Value = item.id.ToString();
                delete.Attributes.Add("class", "center");
                delete.Controls.Add(deleteBT);

                HtmlTableRow tr = new HtmlTableRow();
                tr.Cells.Add(username);
                tr.Cells.Add(email);
                tr.Cells.Add(active);
                tr.Cells.Add(statuses);
                tr.Cells.Add(followings);
                tr.Cells.Add(followers);
                tr.Cells.Add(delete);

                UserTable.Rows.Add(tr);
            }

            Stopwatch w4 = Stopwatch.StartNew();
            var alphabet =
                (from usr in db.Users
                 where !usr.isAdmin
                 orderby usr.username
                 group usr by usr.username.ToUpper().Substring(0, 1) into userGroup
                 select new { firstLetter = userGroup.Key, user = userGroup })
                 .ToDictionary(firstLetter => firstLetter.firstLetter, firstLetter => firstLetter.user.Count());
            w4.Stop();
            ILog log4 = LogManager.GetLogger("QueryLogger");
            log4.Info(" Elapsed time: " + w4.Elapsed + ", select a dictionary of first letters from users");

            int sum = 0;

            for (char c = 'A'; c <= 'Z'; c++)
            {
                HtmlTableCell letter = new HtmlTableCell();
                if (alphabet.Keys.Contains(c.ToString()))
                {
                    HtmlInputButton but = new HtmlInputButton();
                    but.Attributes.Add("title", "Go to page where users start with " + c.ToString());
                    but.Value = c.ToString();
                    but.ID = ((sum / userPerPage) + 1).ToString();
                    but.Attributes.Add("class", "letters");
                    letter.Controls.Add(but);
                    sum += alphabet[c.ToString()];
                }
                else
                {
                    letter.InnerText = c.ToString();
                }

                AlphabetRow.Cells.Add(letter);
            }

            for (int i = 0; i <= (sum-1) / userPerPage; i++)
            {
                HtmlTableCell pagenum = new HtmlTableCell();
                HtmlInputButton but = new HtmlInputButton();
                but.Attributes.Add("title", "Go to page " + (i + 1).ToString());
                but.Value = (i + 1).ToString();
                but.ID = (i + 1).ToString();
                if((i+1) == page)
                    but.Attributes.Add("class", "highlightedpages");
                else
                    but.Attributes.Add("class", "pages");
                pagenum.Controls.Add(but);
                PageRow.Cells.Add(pagenum);
            }
        }

        private void DeleteUser(int id)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            bool isDeleted;
            string errorMessage = String.Empty;

            try
            {
                Stopwatch w = Stopwatch.StartNew();
                db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(q => q.user == id));
                db.SubmitChanges();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", remove all interactive friends of an user");
                Stopwatch w2 = Stopwatch.StartNew();
                db.DynamicFriends.DeleteAllOnSubmit(db.DynamicFriends.Where(q => q.user == id));
                db.SubmitChanges();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", remove all dynamic friends of an user");
                Stopwatch w3 = Stopwatch.StartNew();
                db.StaticFriends.DeleteAllOnSubmit(db.StaticFriends.Where(q => q.user == id));
                db.SubmitChanges();
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", remove all static friends of an user");
                Stopwatch w4 = Stopwatch.StartNew();
                db.Suggestions.DeleteAllOnSubmit(db.Suggestions.Where(q => q.user == id));
                db.SubmitChanges();
                w4.Stop();
                ILog log4 = LogManager.GetLogger("QueryLogger");
                log4.Info(" Elapsed time: " + w4.Elapsed + ", remove all suggestions of an user");
                Stopwatch w1 = Stopwatch.StartNew();
                db.Users.DeleteAllOnSubmit(db.Users.Where(u => u.id == id));
                db.SubmitChanges();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", remove the user");
                isDeleted = true;
            }
            catch (Exception e)
            {
                
                errorMessage = e.Message;
               // errorMessage = e.StackTrace; 
                isDeleted = false;
            }

            XDocument xml = new XDocument(
                            new XElement("Root",
                                new XElement("Deleted", isDeleted),
                                new XElement("Errors", errorMessage)));
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(xml);
            Response.End();
        }
    }
}