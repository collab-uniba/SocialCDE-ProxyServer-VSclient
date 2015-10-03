using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics.Contracts;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UITFSLogin.xaml.
    /// </summary>
    public partial class UITFSLogin : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logo">Url to the logo of the service.</param>
        /// <param name="requireTFSDomain">True if require domain, false otherwise.</param>
        public UITFSLogin(String logo, bool requireTFSDomain)
        {
            Contract.Requires(!String.IsNullOrEmpty(logo));

            InitializeComponent();
            Logo.Source = new BitmapImage(new Uri(Setting.Default.ProxyRoot + logo));
            if (!requireTFSDomain)
            {
                DomainLA.Visibility = Visibility.Hidden;
                TFSDomain.Visibility = Visibility.Hidden;
                DomainRow.Height = new GridLength(0);
            }
        }
    }
}
