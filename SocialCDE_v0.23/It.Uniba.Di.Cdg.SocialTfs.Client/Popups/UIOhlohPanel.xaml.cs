using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UIOhlohPanel.xaml.
    /// </summary>
    public partial class UIOhlohPanel : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logo">Url to the logo of the service.</param>
        /// <param name="oauthVersion">Oauth version of the service.</param>
        public UIOhlohPanel(String logo)
        {
            Contract.Requires(!String.IsNullOrEmpty(logo));

            InitializeComponent();
            Logo.Source = new BitmapImage(new Uri(Setting.Default.ProxyRoot + logo));
            OhlohLabelEmail.Visibility = Visibility.Visible;
            ohloh_email.Visibility = Visibility.Visible;
            Ok.Visibility = Visibility.Visible;
        }
    }
}