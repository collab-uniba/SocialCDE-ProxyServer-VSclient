using System;
using System.Diagnostics.Contracts;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using log4net;
using System.Web;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UIProfile.xaml.
    /// </summary>
    public partial class UIProfile : UIPanel
    {
        #region Attributes

        private Popups.UIPinPanel _pinPanel;
        private Popups.UIFeatures _featuresPanel;
        private Popups.UITFSLogin _tfsLoginPanel;
        private Objects.UIService _service;
        private Popups.UIAvatars _avatars;
        private Popups.UISettings _settings;
        private SHDocVw.WebBrowser _browser;

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIProfile()
        {
            InitializeComponent();
            MoreButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Skills.png"));
            SettingsButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Settings.png"));
            ExitButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Logout.png"));
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
                loading = new Popups.UILoading("Loading Profile");
                UIController.ShowPanel(loading);
            }));

            WUser me = UIController.Proxy.GetUser(UIController.MyProfile.Username, UIController.Password);
            if (me != null)
                UIController.MyProfile = me;

            WService[] services = UIController.Proxy.GetServices(UIController.MyProfile.Username, UIController.Password);

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                Posts.Text = UIController.MyProfile.Statuses.ToString();
                Followings.Text = UIController.MyProfile.Followings.ToString();
                Followers.Text = UIController.MyProfile.Followers.ToString();

                if (String.IsNullOrEmpty(UIController.MyProfile.Avatar))
                    Avatar.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
                else
                    Avatar.InternalImage.Source = new BitmapImage(new Uri(UIController.MyProfile.Avatar));

                UserName.Text = UIController.MyProfile.Username;

                if (services.Length != 0)
                    Services.Children.Clear();
                foreach (WService s in services)
                {
                    Objects.UIService service = new Objects.UIService(s);
                    service.Click += service_Click;
                    Services.Children.Add(service);
                }
                UIController.HidePanel(loading);
            }));
        }

        #region Features

        private void ShowDefaultSettings(Objects.UIService service)
        {
            _service = service;
            _featuresPanel = new Popups.UIFeatures(_service.Service, true);
            _featuresPanel.Unsubscribe.Click += Unsubscribe_Click;
            _featuresPanel.SaveServiceSettings.Click += SaveServiceSettings_Click;
            UIController.ShowPanel(_featuresPanel);
        }

        private void ShowSettings(Objects.UIService service)
        {
            _service = service;
            _featuresPanel = new Popups.UIFeatures(_service.Service, false);
            _featuresPanel.Unsubscribe.Click += Unsubscribe_Click;
            _featuresPanel.SaveServiceSettings.Click += SaveServiceSettings_Click;
            UIController.ShowPanel(_featuresPanel);
        }

        private void SaveServiceSettings_Click(object sender, RoutedEventArgs e)
        {
            UIController.Proxy.UpdateChosenFeatures(
                UIController.MyProfile.Username,
                UIController.Password,
                _service.Service.Id,
                _featuresPanel.ChosenFeatures().ToArray());

            if (String.IsNullOrEmpty(UIController.MyProfile.Avatar) && _featuresPanel.ChosenFeatures().Contains("Avatar"))
            {
                Uri[] avatarsUri = UIController.Proxy.GetAvailableAvatars(UIController.MyProfile.Username, UIController.Password);
                if (avatarsUri.Length > 0)
                {
                    Uri uri = avatarsUri[0];
                    if (UIController.Proxy.SaveAvatar(UIController.MyProfile.Username, UIController.Password, uri))
                    {
                        UIController.ChangeProfileButtonImage(uri);
                        Avatar.InternalImage.Source = new BitmapImage(uri);
                    }
                }
            }

            UIController.HidePanel(_featuresPanel);
            _service = null;
            _featuresPanel = null;
        }

        private void Unsubscribe_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to unsubscribe?", "Unsubscribing", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _service.Unsubscribe();
                UIController.HidePanel(_featuresPanel);
                _service = null;
                _featuresPanel = null;
            }
        }

        #endregion

        #region Service

        private void service_Click(object sender, RoutedEventArgs e)
        {
            Objects.UIService service = sender as Objects.UIService;
            if (service.Service.Registered)
            {
                ShowSettings(service);
            }
            else
            {
                _service = service;
                if (service.Service.RequireOAuth)
                {
                    _pinPanel = new Popups.UIPinPanel(_service.Service.Image, _service.Service.OAuthVersion);
                    _pinPanel.Ok.Click += PinPanelOk;
                    _pinPanel.Cancel.Click += PinPanelCancel;
                    UIController.ShowPanel(_pinPanel);
                    _browser = service.Subscribe();
                    if (_service.Service.OAuthVersion == 2)
                        _browser.DocumentComplete += browser_DocumentComplete;
                }
                else if (service.Service.RequireTFSAuthentication)
                {
                    _tfsLoginPanel = new Popups.UITFSLogin(_service.Service.Image, _service.Service.RequireTFSDomain);
                    _tfsLoginPanel.Ok.Click += TFSLoginOk;
                    _tfsLoginPanel.Cancel.Click += TFSLoginCancel;
                    UIController.ShowPanel(_tfsLoginPanel);
                }
            }
        }

        void browser_DocumentComplete(object pDisp, ref object URL)
        {
            String strURL = URL.ToString();
            if (strURL.Contains("access_token=") || strURL.Contains("code="))
            {
                _service.setAccessToken(HttpUtility.ParseQueryString(strURL).Get(0));
                PinPanelOk(null, null);
                _browser.DocumentComplete -= browser_DocumentComplete;
            }
        }

        #endregion

        #region Pin Panel

        private void PinPanelOk(object sender, RoutedEventArgs e)
        {
            switch (_service.Service.OAuthVersion)
            {
                case 1:
                    _service.Authorize(_pinPanel.Pin.Text);
                    break;
                case 2:
                    _service.Authorize(null);
                    break;
            }
            if (_service.Service.Registered)
            {
                UIController.HidePanel(_pinPanel);
                _pinPanel = null;
                ShowDefaultSettings(_service);
            }
            else
            {
                PinPanelCancel(sender, e);
                MessageBox.Show("Something was wrong. Please, try again.", "Error");
            }
        }

        private void PinPanelCancel(object sender, RoutedEventArgs e)
        {
            UIController.HidePanel(_pinPanel);
            _service = null;
            _pinPanel = null;
        }

        #endregion

        #region TFS Login

        private void TFSLoginOk(object sender, RoutedEventArgs e)
        {
            _service.Subscribe(_tfsLoginPanel.TFSUsername.Text, _tfsLoginPanel.TFSPassword.Password, _tfsLoginPanel.TFSDomain.Text);

            if (_service.Service.Registered)
            {
                UIController.HidePanel(_tfsLoginPanel);
                _tfsLoginPanel = null;
                ShowDefaultSettings(_service);
            }
            else
            {
                TFSLoginCancel(sender, e);
                MessageBox.Show("Something was wrong. Please check your username and password and try again.", "Error");
            }
        }

        private void TFSLoginCancel(object sender, RoutedEventArgs e)
        {
            UIController.HidePanel(_tfsLoginPanel);
            _service = null;
            _tfsLoginPanel = null;
        }

        #endregion

        #region More

        private void More_Click(object sender, RoutedEventArgs e)
        {
            UISkills socialNetwork = new UISkills(UIController.MyProfile);
            UIController.AddPanel(socialNetwork);
        }

        #endregion

        #region Settings

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            _settings = new Popups.UISettings();
            _settings.Ok.Click += SettingsOk;
            _settings.Cancel.Click += SettingsCancel;
            UIController.ShowPanel(_settings);
        }

        private void SettingsOk(object sender, RoutedEventArgs e)
        {
            if (_settings.OldPassword.Password != UIController.Password)
            {
                MessageBox.Show("Old password does not match.", "Error");
            }
            else
            {
                if (String.IsNullOrEmpty(_settings.NewPassword.Password))
                {
                    MessageBox.Show("Passwords should not be empty.", "Error");
                }
                else
                {
                    if (_settings.NewPassword.Password != _settings.ConfirmPassword.Password)
                    {
                        MessageBox.Show("Passwords do not match.", "Error");
                    }
                    else
                    {
                        if (UIController.Proxy.ChangePassword(UIController.MyProfile.Username, _settings.OldPassword.Password, _settings.NewPassword.Password))
                        {
                            UIController.Password = _settings.NewPassword.Password;
                            SettingsCancel(sender, e);
                            MessageBox.Show("Password changed successfully.", "Change password");
                        }
                        else
                        {
                            MessageBox.Show("There was a problem. Please, re-enter your data and try again.", "Error");
                        }
                    }
                }
            }
        }

        private void SettingsCancel(object sender, RoutedEventArgs e)
        {
            UIController.HidePanel(_settings);
            _settings = null;
        }

        #endregion

        #region Avatar

        private void Avatar_Click(object sender, RoutedEventArgs e)
        {
            _avatars = new Popups.UIAvatars();
            _avatars.AvatarsBack.Click += AvatarsBack_Click;
            Uri[] avatarsUri = UIController.Proxy.GetAvailableAvatars(UIController.MyProfile.Username, UIController.Password);
            if (avatarsUri.Length != 0)
                _avatars.Avatars.Children.Clear();
            foreach (Uri uri in avatarsUri)
            {
                Objects.UICustomButton customButton = new Objects.UICustomButton();
                customButton.InternalImage.Source = new BitmapImage(uri);
                customButton.Click += CustomButton_Click;
                _avatars.Avatars.Children.Add(customButton);
            }
            UIController.ShowPanel(_avatars);
        }

        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            Objects.UICustomButton button = sender as Objects.UICustomButton;
            Uri uri = new Uri(button.InternalImage.Source.ToString());
            if (UIController.Proxy.SaveAvatar(UIController.MyProfile.Username, UIController.Password, uri))
            {
                UIController.ChangeProfileButtonImage(uri);
                Avatar.InternalImage.Source = new BitmapImage(uri);
            }
            AvatarsBack_Click(sender, e);
        }

        private void AvatarsBack_Click(object sender, RoutedEventArgs e)
        {
            UIController.HidePanel(_avatars);
            _avatars = null;
        }

        #endregion

        #region Logout
        /// <summary>
        /// Exit from the application and logout.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            UIController.Exit();
        }
        #endregion

        #endregion
    }
}
