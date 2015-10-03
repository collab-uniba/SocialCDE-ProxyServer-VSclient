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
            SocialTFSEntities db = new SocialTFSEntities();

            foreach (var item in db.ServiceInstance.Where(s => s.Service.name != "SocialTFS"))
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
                    editBT.Value = item.pk_id.ToString();
                    edit.Attributes.Add("class", "center");
                    edit.Controls.Add(editBT);
                }

                HtmlInputButton deleteBT = new HtmlInputButton();
                deleteBT.Attributes.Add("title", "Delete " + item.name);
                deleteBT.Attributes.Add("class", "delete");
                deleteBT.Value = item.pk_id.ToString();
                delete.Attributes.Add("class", "center");
                delete.Controls.Add(deleteBT);

                HtmlTableRow tr = new HtmlTableRow();
                tr.ID = "Row" + item.pk_id;
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
            SocialTFSEntities db = new SocialTFSEntities();
            bool isDeleted;

            try
            {
                var delInst = db.ServiceInstance.Where(si => si.pk_id == id);
                foreach (ServiceInstance s in delInst)
                {
                    db.ServiceInstance.DeleteObject(s);
                }
                //db.ServiceInstance.DeleteAllOnSubmit(db.ServiceInstance.Where(si => si.pk_id == id));
                db.SaveChanges();
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