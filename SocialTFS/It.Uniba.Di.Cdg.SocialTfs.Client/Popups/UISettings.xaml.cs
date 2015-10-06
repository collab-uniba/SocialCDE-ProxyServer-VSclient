using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UISettings.xaml.
    /// </summary>
    public partial class UISettings : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public UISettings()
        {
            InitializeComponent();
            if (String.IsNullOrEmpty(UIController.MyProfile.Avatar))
                Avatar.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
            else
                Avatar.Source = new BitmapImage(new Uri(UIController.MyProfile.Avatar));
        }
    }
}
