using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            Series userSeries = RegisteredUser.Series[0];
            userSeries["PieLabelStyle"] = "Outside";
            userSeries.Points.Clear();
            userSeries.Points.AddXY("Registered", db.Users.Where(u => u.active && !u.isAdmin).Count());
            userSeries.Points.AddXY("Unregistered", db.Users.Where(u => !u.active && !u.isAdmin).Count());

            Series serviceSeries = RegisteredService.Series[0];
            serviceSeries["PieLabelStyle"] = "Outside";
            serviceSeries.Points.Clear();

            foreach(ServiceInstance item in db.ServiceInstances.Where(si => si.name != "SocialTFS"))
                serviceSeries.Points.AddXY(item.name, item.Registrations.Count);

        }
    }
}