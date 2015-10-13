using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;
using System.Diagnostics;
using log4net;
using log4net.Config;
using System.Collections.Generic;

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

            Stopwatch w = Stopwatch.StartNew();
            List<ServiceInstance> sInstance = db.ServiceInstances.Where(s => s.Service.name != "SocialTFS").ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", select all service instances different from 'SocialTFS' to load them");

            foreach (var item in sInstance)
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
                Stopwatch w1 = Stopwatch.StartNew();
                db.ServiceInstances.DeleteAllOnSubmit(db.ServiceInstances.Where(si => si.id == id));
                db.SubmitChanges();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", remove all service instances");
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