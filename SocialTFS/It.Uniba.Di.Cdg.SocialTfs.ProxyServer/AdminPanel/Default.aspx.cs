using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using log4net.Config;


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
            Stopwatch w1 = Stopwatch.StartNew();
            int countActiveUsers = db.Users.Where(u => u.active && !u.isAdmin).Count();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", count the number of registered users");
            userSeries.Points.AddXY("Registered", countActiveUsers);
            Stopwatch w2 = Stopwatch.StartNew();
            int countNotActiveUsers = db.Users.Where(u => !u.active && !u.isAdmin).Count();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", count the number of unregistered users");
            userSeries.Points.AddXY("Unregistered", countNotActiveUsers);

            Series serviceSeries = RegisteredService.Series[0];
            serviceSeries["PieLabelStyle"] = "Outside";
            serviceSeries.Points.Clear();

            Stopwatch w = Stopwatch.StartNew();
            List<ServiceInstance> sInstances = db.ServiceInstances.Where(si => si.name != "SocialTFS").ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", select all service instances different from 'SocialTFS'");

            foreach (ServiceInstance item in sInstances)
                serviceSeries.Points.AddXY(item.name, item.Registrations.Count);

        }
    }
}