using System;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UIHideUser.xaml.
    /// </summary>
    public partial class UIHideUser : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">User to hide.</param>
        public UIHideUser(WUser user)
        {
            Contract.Requires(user != null);

            InitializeComponent();
            Title.Text = "Hide " + user.Username + " from:";
            Logo.Source = new BitmapImage(new Uri(user.Avatar));
        }
    }
}
