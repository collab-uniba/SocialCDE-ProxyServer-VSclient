using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Services : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "GET")
                LoadServices();
            else if (Request.RequestType == "POST")
                DeleteService(Int32.Parse(Request.Params["id"]));
        }

        private void LoadServices()
        {
            ConnectorDataContext db = new ConnectorDataContext();

            foreach (var item in db.ServiceInstances.Where(s => s.Service.name != "SocialTFS"))
            {
                HtmlTableCell name = new HtmlTableCell();
                HtmlTableCell service = new HtmlTableCell();
                HtmlTableCell host = new HtmlTableCell();
                HtmlTableCell edit = new HtmlTableCell();
                HtmlTableCell delete = new HtmlTableCell();

                name.InnerText = item.name;
                service.InnerText = item.Service.name;
                host.InnerText = item.host;

                IService iService = ServiceFactory.getService(item.Service.name);

                if (iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance) || iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
                {
                    HtmlInputButton editBT = new HtmlInputButton();
                    editBT.Attributes.Add("title", "Edit " + item.name);
                    editBT.Attributes.Add("class", "edit");
                    editBT.Value = item.id.ToString();
                    edit.Attributes.Add("class", "center");
                    edit.Controls.Add(editBT);
                }

                HtmlInputButton deleteBT = new HtmlInputButton();
                deleteBT.Attributes.Add("title", "Delete " + item.name);
                deleteBT.Attributes.Add("class", "delete");
                deleteBT.Value = item.id.ToString();
                delete.Attributes.Add("class", "center");
                delete.Controls.Add(deleteBT);

                HtmlTableRow tr = new HtmlTableRow();
                tr.ID = "Row" + item.id;
                tr.Cells.Add(name);
                tr.Cells.Add(service);
                tr.Cells.Add(host);
                tr.Cells.Add(edit);
                tr.Cells.Add(delete);
                
                ServiceTable.Rows.Add(tr);
            }
        }

        private void DeleteService(int id)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            bool isDeleted;

            try
            {
                db.ServiceInstances.DeleteAllOnSubmit(db.ServiceInstances.Where(si => si.id == id));
                db.SubmitChanges();
                isDeleted = true;
            }
            catch (Exception)
            {
                isDeleted = false;
            }


            XDocument xml = new XDocument(
                            new XElement("Root",
                                new XElement("Deleted", isDeleted)));
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(xml);
            Response.End();
        }
    }
}