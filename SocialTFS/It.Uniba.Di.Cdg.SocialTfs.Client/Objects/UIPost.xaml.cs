using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Net;
using System.Threading;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Interaction logic for UIPost.xaml.
    /// </summary>
    public partial class UIPost : UserControl
    {
        #region Attributes

        private WPost _post;
        private Boolean _enableProfileRedirect = false;

        // Log4Net reference
        private static readonly ILog log = 
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="post">Post to be showed by this interface.</param>
        internal UIPost(WPost post)
            : this(post, false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="post">Post to be showed by this interface.</param>
        internal UIPost(WPost post, Boolean enableProfileRedirect)
        {
            Contract.Requires(post != null);

            InitializeComponent();
            _post = post;
            _enableProfileRedirect = enableProfileRedirect;

            if (String.IsNullOrEmpty(_post.User.Avatar))
                AvatarImage.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
            else
                AvatarImage.InternalImage.Source = new BitmapImage(new Uri(_post.User.Avatar));

            if (_enableProfileRedirect)
            {
                Hyperlink authorHyperlink = new Hyperlink(new Run(_post.User.Username));
                authorHyperlink.ToolTip = ("Go to: " + _post.User.Username + " profile");
                authorHyperlink.Click += authorHyperlink_Click;
                AuthorTextBlock.Inlines.Add(authorHyperlink);
                AvatarImage.ToolTip = ("Go to: " + _post.User.Username + " profile");
            }
            else
            {
                AuthorTextBlock.Text = _post.User.Username;
                AvatarImage.Type = UICustomButton.ButtonType.JustImage;
            }

            string[] words = _post.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].StartsWith(@"http"))
                {
                    //last element of words array is always "http:/…"
                    if (!words[i].Equals("http:/…"))
                    {
                        Hyperlink hyperlink = new Hyperlink();
                        hyperlink.Inlines.Add(words[i]);
                        hyperlink.NavigateUri = new Uri(words[i]);
                        hyperlink.ToolTip = ("Go to: " + words[i]);
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
                        TextTextBlock.Inlines.Add(hyperlink);
                        TextTextBlock.Inlines.Add(" ");
                    }
                    string link = words[i];

                    ThreadStart starter = delegate
                    {
                        try
                        {
                            checkMediaContent(link);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message, ex);
                        }
                    };
                    
                    Thread previewThread = new Thread(starter);
                    previewThread.Name = "Preview";
                    previewThread.Start();
                }
                else
                {
                    TextTextBlock.Inlines.Add(words[i]);
                    TextTextBlock.Inlines.Add(" ");
                }
            }

            TimeSpan difference = DateTime.UtcNow - _post.CreateAt;

            if (DateTime.Now.Hour < _post.CreateAt.Hour)
                difference += new TimeSpan(1, 0, 0, 0);

            if (difference.Days > 30)
                TimeSourceTextBlock.Text = "more than a month ago";
            else if (difference.Days > 1 && difference.Days <= 30)
                TimeSourceTextBlock.Text = "about " + difference.Days + " days ago";
            else if (difference.Days == 1)
                TimeSourceTextBlock.Text = "about a day ago";
            else
            {
                if (difference.Hours > 1)
                    TimeSourceTextBlock.Text = "about " + difference.Hours + " hours ago";
                else if (difference.Hours == 1)
                    TimeSourceTextBlock.Text = "about an hour ago";
                else
                {
                    if (difference.Minutes > 1)
                        TimeSourceTextBlock.Text = "about " + difference.Minutes + " minutes ago";
                    else if (difference.Minutes == 1)
                        TimeSourceTextBlock.Text = "about a minute ago";
                    else
                        TimeSourceTextBlock.Text = "a few seconds ago";
                }
            }

            TimeSourceTextBlock.Text += " from " + _post.Service.Name;
        }

        private void checkMediaContent(string link)
        {
            try
            {

                link = HttpWebRequest.Create(link).GetResponse().ResponseUri.AbsoluteUri.ToString();
            }
            catch {}

            UIPreview photo = null;

            Dispatcher.BeginInvoke(new Action(delegate()
            {
                if (link.StartsWith("http://www.youtube.com"))
                    photo = new UIPreview(link, PreviewType.Youtube);
                else if (link.StartsWith("http://vimeo.com"))
                    photo = new UIPreview(link, PreviewType.Viemo);
                else if (link.StartsWith("http://www.dailymotion.com"))
                    photo = new UIPreview(link, PreviewType.DailyMotion);
                else if (link.StartsWith("http://www.flickr.com"))
                    photo = new UIPreview(link, PreviewType.Flickr);
                //else if (link.StartsWith("https://picasaweb.google.com"))
                //    photo = new UIPreview(link, PreviewType.Picasa);
                else if (link.EndsWith(@".jpg") || link.EndsWith(@".jpeg"))
                    photo = new UIPreview(link, PreviewType.JPEG);
                else if (link.EndsWith(@".png"))
                    photo = new UIPreview(link, PreviewType.PNG);
                else if (link.EndsWith(@".bmp"))
                    photo = new UIPreview(link, PreviewType.BMP);
                else if (link.EndsWith(@".gif"))
                    photo = new UIPreview(link, PreviewType.GIF);

                if (photo != null)
                    PreviewStack.Children.Add(photo);
            }));

        }

        private void authorHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OpenUserProfile();
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

        private void AvatarImage_Click(object sender, RoutedEventArgs e)
        {
            OpenUserProfile();
        }

        private void OpenUserProfile()
        {
            if (_enableProfileRedirect)
            {
                Panels.UIColleague colleague = new Panels.UIColleague(_post.User.Id);
                UIController.AddPanel(colleague);
            }
        }

        #endregion
    }
}
