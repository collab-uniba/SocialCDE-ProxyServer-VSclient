using System;
using System.Web;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Windows;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Interaction logic for UIService.xaml.
    /// </summary>
    public partial class UIService : UserControl
    {
        #region Attributes

        private int _size = 120;
        private WOAuthData _oauthData;
        internal WService Service { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Event activated when the control is clicked.
        /// </summary>
        public event ClickDelegate Click;

        #endregion

        public void setAccessToken(String token)
        {
            _oauthData.AccessToken = token;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="service">Social network service interface reference.</param>
        public UIService(WService service)
        {
            Contract.Requires(service != null);

            InitializeComponent();
            Service = service;
            Title.Text = Service.Name;
            Logo.Source = new BitmapImage(new Uri(Setting.Default.ProxyRoot + Service.Image));
            if (Service.Registered)
            {
                ToolTip = "Select " + Service.Name + " features";
                Active.Fill = new SolidColorBrush(Color.FromRgb(0, 192, 0));
            }
            else
            {
                ToolTip = "Subscribe to " + Service.Name;
                Active.Fill = new SolidColorBrush(Color.FromRgb(192, 0, 0));
            }
        }

        internal void Unsubscribe()
        {
            if (UIController.Proxy.DeleteRegistredService(UIController.MyProfile.Username, UIController.Password, Service.Id))
            {
                Service.Registered = false;
                ToolTip = "Subscribe to " + Service.Name;
                Active.Fill = new SolidColorBrush(Color.FromRgb(192, 0, 0));
            }
        }

        internal void Subscribe(String username, String password, String domain)
        {
            if (UIController.Proxy.RecordService(UIController.MyProfile.Username, UIController.Password, Service.Id, username, password, domain))
                Authorized();
        }

        /// <summary>
        /// Start the Authorization procedure.
        /// </summary>
        internal SHDocVw.WebBrowser Subscribe()
        {
            _oauthData = UIController.Proxy.GetOAuthData(UIController.MyProfile.Username, UIController.Password, Service.Id);
            if (_oauthData != null)
                return UIController.Browse(_oauthData.AuthorizationLink);
            else
                return null;
        }

        /// <summary>
        /// Continue the authentication process requiring authorization.
        /// </summary>
        /// <param name="verifier">Verifier pin</param>
        internal void Authorize(String verifier)
        {
            if (UIController.Proxy.Authorize(UIController.MyProfile.Username, UIController.Password, Service.Id, verifier, _oauthData.AccessToken, _oauthData.AccessSecret))
                Authorized();
        }

        private void Authorized()
        {
            Service.Registered = true;
            ToolTip = "Select " + Service.Name + " features";
            Active.Fill = new SolidColorBrush(Color.FromRgb(0, 192, 0));
            _oauthData = null;
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Opacity = 0.8;
            Width = _size;
            Height = _size;
            Margin = new Thickness(5, 0, 0, 5);
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Width = _size - 10;
            Height = _size - 10;
            Margin = new Thickness(10, 5, 5, 10);
        }

        private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Width = _size;
            Height = _size;
            Margin = new Thickness(5, 0, 0, 5);
            if (Click != null)
                Click(this, null);
        }
    }
}
