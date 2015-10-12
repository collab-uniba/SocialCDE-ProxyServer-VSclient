using System;
using System.Linq;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class EditService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "GET")
                if (String.IsNullOrEmpty(Request.QueryString["id"]))
                    Response.Redirect("Services.aspx");
                else
                    PopulateService();
            else if (Request.RequestType == "POST")
                SaveService();
            else
                Response.Redirect("Services.aspx");
        }

        private void PopulateService()
        {
            ConnectorDataContext db = new ConnectorDataContext();

            Stopwatch w = Stopwatch.StartNew();
            ServiceInstance service =
                   (from serin in db.ServiceInstances
                    where serin.id == Int32.Parse(Request.QueryString["id"])
                    select serin).Single();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", select the service instances to edit");
            
            IService iService = ServiceFactory.getService(service.Service.name);

            if (!iService.GetPrivateFeatures().Contains(FeaturesType.MoreInstance) && !iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
                Response.Redirect("Services.aspx");
            
            Id.Value = service.id.ToString();
            ServiceTB.Value = service.Service.name;
            NameTB.Value = service.name;
            HostTB.Value = service.host;
            if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1))
            {
                ConsumerKeyTB.Value = service.consumerKey;
                ConsumerSecretTB.Value = service.consumerSecret;
            }
            else
            {
                ConsumerKeyTB.Attributes["required"] = String.Empty;
                ConsumerSecretTB.Attributes["required"] = String.Empty;
                ConsumerKeyRW.Visible = false;
                ConsumerSecretRW.Visible = false;
            }

            if (!iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
            {
                GitHubLabelRW.Visible = false;
                ErrGitHubLabelRW.Visible = false; 
            }
            else
            {
                ServiceTB.Disabled = true;
                NameTB.Disabled = true;
                HostTB.Disabled = true;
                GitHubLabelTB.Value = ServiceFactory.GitHubLabels;
                ErrGitHubLabelRW.Visible = true; 
            }
        }

        private void SaveService()
        {
            if (!String.IsNullOrEmpty(NameTB.Attributes["required"]) && String.IsNullOrEmpty(Request.Params["ctl00$MainContent$NameTB"]))
                Response.Redirect("Services.aspx");
            if (!String.IsNullOrEmpty(HostTB.Attributes["required"]) && String.IsNullOrEmpty(Request.Params["ctl00$MainContent$HostTB"]))
                Response.Redirect("Services.aspx");
            if (!String.IsNullOrEmpty(ConsumerKeyTB.Attributes["required"]) && String.IsNullOrEmpty(Request.Params["ctl00$MainContent$ConsumerKeyTB"]))
                Response.Redirect("Services.aspx");
            if (!String.IsNullOrEmpty(ConsumerSecretTB.Attributes["required"]) && String.IsNullOrEmpty(Request.Params["ctl00$MainContent$ConsumerSecretTB"]))
                Response.Redirect("Services.aspx");

            ConnectorDataContext db = new ConnectorDataContext();

            Stopwatch w = Stopwatch.StartNew();
            ServiceInstance service =
                   (from serin in db.ServiceInstances
                    where serin.id == Int32.Parse(Request.QueryString["id"])
                    select serin).Single();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", select the service instances to save");

            IService iService = ServiceFactory.getService(service.Service.name);

            if (iService.GetPrivateFeatures().Contains(FeaturesType.Labels))
            {
                if (String.IsNullOrEmpty(Request.Params["ctl00$MainContent$GitHubLabelTB"]))
                {
                    System.Diagnostics.Debug.WriteLine("label nulla");
                    ServiceFactory.GitHubLabels = String.Empty;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("labels associate " + Request.Params["ctl00$MainContent$GitHubLabelTB"]);
                    ServiceFactory.GitHubLabels = Request.Params["ctl00$MainContent$GitHubLabelTB"];
                }
            }
            else
            {
                service.name = Request.Params["ctl00$MainContent$NameTB"];
                service.host = Request.Params["ctl00$MainContent$HostTB"];
                if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1))
                {
                    service.consumerKey = Request.Params["ctl00$MainContent$ConsumerKeyTB"];
                    service.consumerSecret = Request.Params["ctl00$MainContent$ConsumerSecretTB"];
                }
                else if (iService.GetPrivateFeatures().Contains(FeaturesType.TFSAuthenticationWithDomain))
                {
                    service.consumerKey = Request.Params["ctl00$MainContent$UsernameTB"];
                    service.consumerSecret = Request.Params["ctl00$MainContent$PasswordTB"];
                }

                Stopwatch w1 = Stopwatch.StartNew();
                db.SubmitChanges();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", edit service");
            }
            
            Response.Redirect("Services.aspx");
        }
    }
}