using System;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using System.Data;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel
{
    public partial class Weights : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebUtility.CheckCredentials(this);
            if (Request.RequestType == "GET")
                LoadWeights();
            else if (Request.RequestType == "POST")
                SaveWeights();
        }

        private void LoadWeights()
        {
            ConnectorDataContext db = new ConnectorDataContext();

            foreach (var item in db.FeatureScores)
            {
                HtmlTableCell service = new HtmlTableCell();
                HtmlTableCell feature = new HtmlTableCell();
                HtmlTableCell weight = new HtmlTableCell();

                service.InnerText = item.ServiceInstance.name;
                feature.InnerText = item.feature;
                weight.InnerText = item.score.ToString();
                weight.Attributes.Add("class", "center");
                weight.Attributes.Add("contenteditable", "true");

                HtmlTableRow tr = new HtmlTableRow();
                tr.Cells.Add(service);
                tr.Cells.Add(feature);
                tr.Cells.Add(weight);

                WeightTable.Rows.Add(tr);
            }
        }

        private void SaveWeights()
        {
            ConnectorDataContext db = new ConnectorDataContext();
            bool isSaved;

            XmlDocument requestXml = new XmlDocument();
            requestXml.Load(new XmlTextReader(new StreamReader(Request.InputStream)));
            try
            {
                foreach (XmlNode item in requestXml.SelectNodes("//weights/item"))
                {
                    FeatureScore featureScore = db.FeatureScores.Where(fs => fs.ServiceInstance.name == item.SelectSingleNode("service").InnerText && fs.feature == item.SelectSingleNode("feature").InnerText).Single();
                    featureScore.score = Int32.Parse(item.SelectSingleNode("weight").InnerText);
                }
                db.SubmitChanges();
                isSaved = true;
            }
            catch (Exception)
            {
                isSaved = false;
            }

            XDocument xml = new XDocument(
                            new XElement("Root",
                                new XElement("Saved", isSaved)));
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(xml);
            Response.End();
        }
    }
}