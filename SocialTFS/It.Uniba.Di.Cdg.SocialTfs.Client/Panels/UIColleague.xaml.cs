using System;
using System.Diagnostics.Contracts;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Threading;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UIColleague.xaml.
    /// </summary>
    public partial class UIColleague : UIPanel
    {
        private WUser _user;
        private BitmapImage _followImage = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Follow.png"));
        private BitmapImage _unfollowImage = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Unfollow.png"));
        private Popups.UIHideUser _hideuser;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userId">Identifier of the user.</param>
        public UIColleague(int userId)
        {
            Contract.Requires(userId >= 0);

            _user = UIController.Proxy.GetColleagueProfile(UIController.MyProfile.Username, UIController.Password, userId); 
            InitializeComponent();

            Timeline.LoadTimeline(_user);
            if (UIController.MyProfile.Id == _user.Id)
                FollowButton.Visibility = Visibility.Hidden;
            else
            {
                if (_user.Followed)
                {
                    FollowButton.InternalImage.Source = _unfollowImage;
                    FollowButton.ToolTip = "Unfollow this user in SocialTFS";
                }
                else
                {
                    FollowButton.InternalImage.Source = _followImage;
                    FollowButton.ToolTip = "Follow this user in SocialTFS";
                }
            }

            MoreButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Skills.png"));
            HideButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Hide.png"));
            Posts.Text = _user.Statuses.ToString();
            Followings.Text = _user.Followings.ToString();
            Followers.Text = _user.Followers.ToString();

            if (String.IsNullOrEmpty(_user.Avatar))
                AvatarImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
            else
                AvatarImage.Source = new BitmapImage(new Uri(_user.Avatar));

            UserName.Text = _user.Username;
        }

        public override void Open()
        {
            if (UIController.Connected)
                Timeline.StartUpdates();
        }

        public override void Close()
        {
            Timeline.StopUpdates();
        }

        private void Follow_Click(object sender, RoutedEventArgs e)
        {
            FollowButton.IsChecked = false;
            if (_user.Followed)
            {
                if (UIController.Proxy.Unfollow(UIController.MyProfile.Username, UIController.Password, _user.Id))
                {
                    FollowButton.InternalImage.Source = _followImage;
                    FollowButton.ToolTip = "Follow this user in SocialTFS";
                    _user.Followed = false;
                }
            }
            else
            {
                if (UIController.Proxy.Follow(UIController.MyProfile.Username, UIController.Password, _user.Id))
                {
                    FollowButton.InternalImage.Source = _unfollowImage;
                    FollowButton.ToolTip = "Unfollow this user in SocialTFS";
                    _user.Followed = true;
                }
            }
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            _hideuser = new Popups.UIHideUser(_user);
            _hideuser.Save.Click += HideSave_Click;
            _hideuser.Cancel.Click += HideCancel_Click;
            WHidden wHide = UIController.Proxy.GetUserHideSettings(UIController.MyProfile.Username, UIController.Password, _user.Id);
            if (wHide.Suggestions)
                _hideuser.SuggestionsCheck.IsChecked = true;
            if (wHide.Dynamic)
                _hideuser.IterationCheck.IsChecked = true;
            if (wHide.Interactive)
                _hideuser.InteractiveCheck.IsChecked = true;
            UIController.ShowPanel(_hideuser);
        }

        private void HideSave_Click(object sender, RoutedEventArgs e)
        {
            UIController.Proxy.UpdateHiddenUser(
                UIController.MyProfile.Username,
                UIController.Password,
                _user.Id,
                (bool)_hideuser.SuggestionsCheck.IsChecked,
                (bool)_hideuser.IterationCheck.IsChecked,
                (bool)_hideuser.InteractiveCheck.IsChecked);

            HideCancel_Click(sender, e);
        }

        private void HideCancel_Click(object sender, RoutedEventArgs e)
        {
            UIController.HidePanel(_hideuser);
            _hideuser = null;
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            MoreButton.IsChecked = false;
            UISkills socialNetwork = new UISkills(_user);
            UIController.AddPanel(socialNetwork);
        }
    }
}
