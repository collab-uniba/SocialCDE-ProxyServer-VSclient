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
            ConnectorDataContext db = new ConnectorDataContext();

            foreach (Service item in db.Services.Where(s => s.name != "SocialTFS"))
            {
                IService iService = ServiceFactory.getService(item.name);
                if (iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance) ||
                    !db.ServiceInstances.Select(si => si.service).Contains(item.id))
                    ServiceSE.Items.Add(new ListItem(item.name, item.id.ToString()));
            }
        }

        private void ServiceFields()
        {
            ConnectorDataContext db = new ConnectorDataContext();
            try
            {
                IService iService = ServiceFactory.getService(db.Services.Where(s => s.id == Int32.Parse(Request.QueryString["id"])).Single().name);

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
            ConnectorDataContext db = new ConnectorDataContext();
            Service service = new Service();
            try
            {
                service = db.Services.Where(s => s.id == Int32.Parse(Request.Params["ctl00$MainContent$ServiceSE"])).Single();
            }
            catch
            {
                ErrorPA.Style.Remove("display");
            }

            IService iService = ServiceFactory.getService(service.name);
            ServiceInstance serviceInstance = new ServiceInstance();
            if (!iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance))
            {
                PreregisteredService preser = db.PreregisteredServices.Where(ps => ps.service == service.id).Single();

                serviceInstance.name = preser.name;
                serviceInstance.host = preser.host;
                serviceInstance.service = preser.service;
                serviceInstance.consumerKey = preser.consumerKey;
                serviceInstance.consumerSecret = preser.consumerSecret;

                db.ServiceInstances.InsertOnSubmit(serviceInstance);
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
                serviceInstance.service = service.id;
                serviceInstance.consumerKey = consumerKey;
                serviceInstance.consumerSecret = consumerSecret;

                db.ServiceInstances.InsertOnSubmit(serviceInstance);
            }

            db.SubmitChanges();

            if (iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
            {
                iService.Get(FeaturesType.Labels, Request.Params["ctl00$MainContent$GitHubLabelTB"]);
            }

            foreach (FeaturesType featureType in iService.GetScoredFeatures())
            {
                db.FeatureScores.InsertOnSubmit(new FeatureScore()
                {
                    serviceInstance = serviceInstance.id,
                    feature = featureType.ToString(),
                    score = 1
                });
            }
            //TODO update the new version (leave comment from the next line)
            //dbService.version = newServiceVersion;
            db.SubmitChanges();

            Response.Redirect("Services.aspx");
        }
    }
}