using System;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Web.UI;

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

            var users =
                (from usr in db.Users
                 where !usr.isAdmin
                 orderby usr.username
                 select usr).Skip((page - 1) * userPerPage).Take(userPerPage);

            if (!users.Any() && page > 1)
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
                statuses.InnerText = db.Posts.Where(p => p.ChosenFeature.user == item.id).Count().ToString();
                followings.Attributes.Add("class", "center");
                followings.InnerText = db.StaticFriends.Where(sf => sf.User == item).Count().ToString();
                followers.Attributes.Add("class", "center");
                followers.InnerText = db.StaticFriends.Where(sf => sf.Friend == item).Count().ToString();

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

            var alphabet =
                (from usr in db.Users
                 where !usr.isAdmin
                 orderby usr.username
                 group usr by usr.username.ToUpper().Substring(0, 1) into userGroup
                 select new { firstLetter = userGroup.Key, user = userGroup })
                 .ToDictionary(firstLetter => firstLetter.firstLetter, firstLetter => firstLetter.user.Count());

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
                db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(q => q.user == id));
                db.SubmitChanges();
                db.DynamicFriends.DeleteAllOnSubmit(db.DynamicFriends.Where(q => q.user == id));
                db.SubmitChanges();
                db.StaticFriends.DeleteAllOnSubmit(db.StaticFriends.Where(q => q.user == id));
                db.SubmitChanges();
                db.Suggestions.DeleteAllOnSubmit(db.Suggestions.Where(q => q.user == id));
                db.SubmitChanges();
                db.Users.DeleteAllOnSubmit(db.Users.Where(u => u.id == id));
                db.SubmitChanges();
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