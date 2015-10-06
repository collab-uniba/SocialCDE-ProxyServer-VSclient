using System;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;
using System.Collections.Generic;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class NewService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "GET")
            {
                if (ServiceSE.Items.Count == 0)
                    PopulateServices();
                if (!String.IsNullOrEmpty(Request.QueryString["id"]))
                    ServiceFields();
            }
            else if (Request.RequestType == "POST")
                SaveService();
            else
                Response.Redirect("Services.aspx");
        }

        private void PopulateServices()
        {
            ServiceSE.Items.Add(new ListItem());
            SocialTFSEntities db = new SocialTFSEntities();

            foreach (Service item in db.Service.Where(s => s.name != "SocialTFS"))
            {
                IService iService = ServiceFactory.getService(item.name);
                if (iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance) ||
                    (!db.ServiceInstance.Select(si => si.fk_service).Contains(item.pk_id)))
                    ServiceSE.Items.Add(new ListItem(item.name, item.pk_id.ToString()));
            }
        }

        private void ServiceFields()
        {
            SocialTFSEntities db = new SocialTFSEntities();
            try
            {
                //Request.QueryString["id"])).Single().name;
                //Request.QueryString["id"]

                int id = Int32.Parse(Request.QueryString["id"]);

                IService iService = ServiceFactory.getService(db.Service.Where(s => s.pk_id == id).Single().name);

                XDocument xml = new XDocument(
                    new XElement("Root",
                        new XElement("CanHaveMoreInstance", iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance)),
                        new XElement("NeedOAuth", iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1)),
                        new XElement("NeedGitHubLabel", iService.Name.Equals("GitHub"))));
                Response.Clear();
                Response.ContentType = "text/xml";
                Response.Write(xml);
                Response.End();
            }
            catch (TargetInvocationException)
            {
                XDocument xml = new XDocument(
                       new XElement("Root",
                           new XElement("CanHaveMoreInstance", false),
                           new XElement("NeedOAuth", false),
                           new XElement("NeedGitHubLabel", false)));
                Response.Clear();
                Response.ContentType = "text/xml";
                Response.Write(xml);
                Response.End();
            }
            catch (InvalidOperationException)
            {
                Response.Redirect("Services.aspx");
            }
        }

        private void SaveService()
        {
            SocialTFSEntities db = new SocialTFSEntities();
            Service service = new Service();
            int id = Int32.Parse(Request.Params["ctl00$MainContent$ServiceSE"]);
            try
            {
                service = db.Service.Where(s => s.pk_id == id).Single();
            }
            catch
            {
                ErrorPA.Style.Remove("display");
            }

            IService iService = ServiceFactory.getService(service.name);
            ServiceInstance serviceInstance = new ServiceInstance();
            if (!iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance))
            {
                PreregisteredService preser = db.PreregisteredService.Where(ps => ps.service == service.pk_id).Single();

                serviceInstance.name = preser.name;
                serviceInstance.host = preser.host;
                serviceInstance.fk_service = preser.service;
                serviceInstance.consumerKey = preser.consumerKey;
                serviceInstance.consumerSecret = preser.consumerSecret;
                db.ServiceInstance.AddObject(serviceInstance);
            }
            else
            {
                string consumerKey = null, consumerSecret = null;
                string host = Request.Params["ctl00$MainContent$HostTB"];

                if (host.EndsWith(@"/"))
                    host = host.Remove(host.Length - 1);

                if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1))
                {
                    consumerKey = Request.Params["ctl00$MainContent$ConsumerKeyTB"];
                    consumerSecret = Request.Params["ctl00$MainContent$ConsumerSecretTB"];
                }

                serviceInstance.name = Request.Params["ctl00$MainContent$NameTB"];
                serviceInstance.host = host;
                serviceInstance.Service = service;
                serviceInstance.consumerKey = consumerKey;
                serviceInstance.consumerSecret = consumerSecret;

                db.ServiceInstance.AddObject(serviceInstance);
            }

            db.SaveChanges();

            if (iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
            {
                iService.Get(FeaturesType.Labels, Request.Params["ctl00$MainContent$GitHubLabelTB"]);
            }

            foreach (FeaturesType featureType in iService.GetScoredFeatures())
            {
                db.FeatureScore.AddObject(new FeatureScore()
                {

                    pk_fk_serviceInstance = serviceInstance.pk_id,
                    pk_fk_feature = featureType.ToString(),
                    score = 1
                });
            }
            //TODO update the new version (leave comment from the next line)
            //dbService.version = newServiceVersion;
            db.SaveChanges();

            Response.Redirect("Services.aspx");
        }
    }
}