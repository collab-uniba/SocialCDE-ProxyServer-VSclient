using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UIPinPanel.xaml.
    /// </summary>
    public partial class UIPinPanel : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logo">Url to the logo of the service.</param>
        /// <param name="oauthVersion">Oauth version of the service.</param>
        public UIPinPanel(String logo, int oauthVersion)
        {
            Contract.Requires(!String.IsNullOrEmpty(logo));
            Contract.Requires(oauthVersion == 1 || oauthVersion == 2);

            InitializeComponent();
            Logo.Source = new BitmapImage(new Uri(Setting.Default.ProxyRoot + logo));
            switch (oauthVersion)
            {
                case 1:
                    PinLabel.Visibility = Visibility.Visible;
                    Pin.Visibility = Visibility.Visible;
                    Ok.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Instruction.Visibility = Visibility.Visible;
                    Busy.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
