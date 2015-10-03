using System.Windows;
using System.Windows.Controls;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Windows.Input;
using System.Threading;
using System;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UIPeople.xaml.
    /// </summary>
    public partial class UIPeople : UIPanel
    {

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UIPeople()
        {
            InitializeComponent();
        }

        public override void Open()
        {
            ThreadStart starter = delegate
            {
                try
                {
                    AsyncUpdate();
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            };
            Thread asyncUpdateThread = new Thread(starter);
            asyncUpdateThread.Name = "Async Update";
            asyncUpdateThread.Start();
        }

        /// <summary>
        /// Update the lists of people in asyncronous way.
        /// </summary>
        private void AsyncUpdate()
        {
            Popups.UILoading loading = null;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                loading = new Popups.UILoading("Loading People");
                UIController.ShowPanel(loading);
            }));

            WUser[] suggestion = UIController.Proxy.GetSuggestedFriends(UIController.MyProfile.Username, UIController.Password);
            WUser[] followings = UIController.Proxy.GetFollowings(UIController.MyProfile.Username, UIController.Password);
            WUser[] followers = UIController.Proxy.GetFollowers(UIController.MyProfile.Username, UIController.Password);
            WUser[] hidden = UIController.Proxy.GetHiddenUsers(UIController.MyProfile.Username, UIController.Password);

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                Suggestion.Children.Clear();
                if (suggestion.Length == 0)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "We have no suggestions for you.\nPlease try again soon.";
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(5);
                    Suggestion.Children.Add(textBlock);
                }
                foreach (WUser item in suggestion)
                {
                    Objects.UIPerson user = new Objects.UIPerson(item);
                    user.Click += user_Click;
                    Suggestion.Children.Add(user);
                }

                Followings.Children.Clear();
                if (followings.Length == 0)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "You are following no one.";
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(5);
                    Followings.Children.Add(textBlock);
                }
                foreach (WUser item in followings)
                {
                    Objects.UIPerson user = new Objects.UIPerson(item);
                    user.Click += user_Click;
                    Followings.Children.Add(user);
                }

                Followers.Children.Clear();
                if (followers.Length == 0)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "No one is following you.";
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(5);
                    Followers.Children.Add(textBlock);
                }
                foreach (WUser item in followers)
                {
                    Objects.UIPerson user = new Objects.UIPerson(item);
                    user.Click += user_Click;
                    Followers.Children.Add(user);
                }

                Hiddens.Children.Clear();
                if (hidden.Length == 0)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "You have hidden no one.";
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.Margin = new Thickness(5);
                    Hiddens.Children.Add(textBlock);
                }
                foreach (WUser item in hidden)
                {
                    Objects.UIPerson user = new Objects.UIPerson(item);
                    user.Click += user_Click;
                    Hiddens.Children.Add(user);
                }

                UIController.HidePanel(loading);
            }));
        }

        private void user_Click(object sender, RoutedEventArgs e)
        {
            Objects.UIPerson user = sender as Objects.UIPerson;
            UIColleague colleague = new UIColleague(user.User.Id);
            UIController.AddPanel(colleague);
        }
    }
}
