using System;
using System.Diagnostics.Contracts;
using System.Net.Cache;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Windows;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Interaction logic for UIPerson.xaml.
    /// </summary>
    public partial class UIPerson : UserControl
    {
        #region Attributes

        int _size = 120;
        internal WUser User { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Event activated when the control is clicked.
        /// </summary>
        public event ClickDelegate Click;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user"></param>
        public UIPerson(WUser user)
        {
            Contract.Requires(user != null);

            InitializeComponent();
            User = user;
            Username.Text = User.Username;

            if (String.IsNullOrEmpty(User.Avatar))
                Avatar.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
            else
                Avatar.Source = new BitmapImage(new Uri(User.Avatar));
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
