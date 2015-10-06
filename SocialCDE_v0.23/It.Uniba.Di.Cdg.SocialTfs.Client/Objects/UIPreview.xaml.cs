using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Threading;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Type of previewThread.
    /// </summary>
    public enum PreviewType
    {
        Youtube,
        Viemo,
        DailyMotion,
        Flickr,
        Picasa,
        JPEG,
        PNG,
        BMP,
        GIF
    }

    /// <summary>
    /// Interaction logic for UIPreview.xaml.
    /// </summary>
    public partial class UIPreview : UserControl
    {

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pageUrl">Url of a web page.</param>
        /// <param name="type">Type of the previewThread.</param>
        public UIPreview(string pageUrl, PreviewType type)
        {
            Contract.Requires(!String.IsNullOrEmpty(pageUrl));

            InitializeComponent();

            ThreadStart starter = delegate
            {
                try
                {
                    loadPreview(pageUrl, type);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            };

            Thread loadPreviewThread = new Thread(starter);
            loadPreviewThread.Name = "Load Preview";
            loadPreviewThread.Start();
        }

        private void loadPreview(string pageUrl, PreviewType type)
        {
            string htmlCode = new WebClient().DownloadString(pageUrl);
            string title = FormatString(Regex.Match(htmlCode, @"\<title\>([^>]+)\<\/title\>").Groups[1].Value);
            string imageUrl = "";

            switch (type)
            {
                case PreviewType.Youtube:
                    imageUrl = YouTubePreview(pageUrl);
                    break;
                case PreviewType.Viemo:
                    imageUrl = ViemoPreview(pageUrl);
                    break;
                case PreviewType.DailyMotion:
                    imageUrl = DailyMotionPreview(pageUrl);
                    break;
                case PreviewType.Flickr:
                    imageUrl = FlickrPreview(pageUrl);
                    break;
                //case PreviewType.Picasa:
                //    ImageUrl = PicasaPreview(pageUrl);
                //    break;
                case PreviewType.JPEG:
                case PreviewType.PNG:
                case PreviewType.BMP:
                case PreviewType.GIF:
                    imageUrl = ImagePreview(pageUrl);
                    break;
            }

            Dispatcher.BeginInvoke(new Action(delegate()
                {
                    Hyperlink hyperlink = new Hyperlink();
                    hyperlink.NavigateUri = new Uri(pageUrl);
                    hyperlink.Inlines.Add((title == String.Empty) ? pageUrl.Split('/').Last() : title);
                    hyperlink.ToolTip = ("Go to: " + pageUrl);
                    hyperlink.Click += hyperlink_Click;
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItemWithLink internalBrowser = new MenuItemWithLink();
                    internalBrowser.hyperlink = hyperlink;
                    internalBrowser.Header = "Internal Browser";
                    internalBrowser.Click += internalBrowser_Click;
                    MenuItemWithLink externalBrowser = new MenuItemWithLink();
                    externalBrowser.hyperlink = hyperlink;
                    externalBrowser.Header = "External Browser";
                    externalBrowser.Click += externalBrowser_Click;
                    contextMenu.Items.Add(internalBrowser);
                    contextMenu.Items.Add(externalBrowser);
                    hyperlink.ContextMenu = contextMenu;

                    PreviewTitle.Inlines.Add(hyperlink);
                    PreviewImage.Source = new BitmapImage(new Uri(imageUrl));
                }));
        }

        private string YouTubePreview(string pageUrl)
        {
            string videoCode = string.Empty;
            foreach (string s in pageUrl.Split('/')[3].Split('?')[1].Split('&'))
            {
                if (s.Split('=')[0] == "v")
                    videoCode = s.Split('=')[1];
            }
            return @"http://img.youtube.com/vi/" + videoCode + "/0.jpg";
        }

        private string ViemoPreview(string pageUrl)
        {
            XElement root = XElement.Load(@"http://vimeo.com/api/v2/video/" + pageUrl.Split('/')[3] + ".xml");
            return root.Element("video").Element("thumbnail_medium").Value;
        }

        private string DailyMotionPreview(string pageUrl)
        {
            return @"http://www.dailymotion.com/thumbnail/video/" + pageUrl.Split('/')[4];
        }

        private string FlickrPreview(string pageUrl)
        {
            XElement root = XElement.Load(@"http://api.flickr.com/services/rest/?method=flickr.photos.getSizes&api_key=a651635596490fd0ded161f4db65f7fb&photo_id=" + pageUrl.Split('/')[5] + "&format=rest");

            string imageUrl =
                (from elem in root.Element("sizes").Elements("size")
                 where (string)elem.Attribute("label") == "Medium"
                 select elem.Attribute("source").Value).First().ToString();

            return imageUrl;
        }

        /// This code doesn't work becouse API need login
        /// 
        //private string PicasaPreview(string pageUrl)
        //{
        //    XElement root = XElement.Load(@"https://picasaweb.google.com/data/feed/api/user/" + pageUrl.Split('/')[3] + "/album/" + pageUrl.Split('/')[4].Split('?')[0]);

        //    string imageUrl =
        //        (from elem in root.Elements("entry")
        //         where elem.Element("id").Value.EndsWith(pageUrl.Split('#')[1])
        //         select elem.Element("media:group").Elements("media:thumbnail").Attributes("url")).ToArray()[2].ToString();

        //    return imageUrl;
        //}

        private string ImagePreview(string pageUrl)
        {
            return pageUrl;
        }

        private string FormatString(string inputString)
        {
            string result = "";

            string[] parts = inputString.Split(new char[] { ' ', '\n', '\t', '\r', '\f', '\v' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in parts)
                result += item + " ";

            return result;
        }

        private void internalBrowser_Click(object sender, RoutedEventArgs e)
        {
            UIController.Browse(((MenuItemWithLink)sender).hyperlink.NavigateUri.ToString());
        }

        private void externalBrowser_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((MenuItemWithLink)sender).hyperlink.NavigateUri.ToString());
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UIController.Browse((sender as Hyperlink).NavigateUri.ToString());
        }
    }
}
