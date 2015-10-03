using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Containers
{
    /// <summary>
    /// Interaction logic for UIRegistration.xaml.
    /// </summary>
    public partial class UIRegistration : UserControl
    {
        private String _host;
        private String _username;
        private String _email;
        private String _password;
        private String _invitationCode;

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UIRegistration()
        {
            InitializeComponent();
        }

        private void ShowLoginButton_Click(object sender, RoutedEventArgs e)
        {
            UIController.ShowLoginPanel();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (UIController.Proxy == null)
            {
                ErrorBorder.Background = Brushes.Red;
                ErrorRow.Height = GridLength.Auto;
                ErrorLabel.Content = "Please enter a valid host for the proxy";
            }
            else if (!CheckPassword())
            {
                ErrorBorder.Background = Brushes.Red;
                ErrorRow.Height = GridLength.Auto;
                ErrorLabel.Content = "Please enter a valid password";
            }
            else
            {
                _email = Email.Text;
                _username = UserName.Text;
                _password = Password.Password;
                _invitationCode = InvitationCode.Password;

                ThreadStart starter = delegate
                {
                    try
                    {
                        Register();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                };

                Thread registerThread = new Thread(starter);
                registerThread.Name = "Register";
                registerThread.Start();
            }
        }

        private void Register()
        {
            Popups.UILoading loading = null;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                loading = new Popups.UILoading("Registration in progress");
                UIController.ShowPanel(loading);
            }));

            int res = UIController.Proxy.SubscribeUser(_email, _invitationCode, _username);
            GridLength errorRowHeight = GridLength.Auto; ;
            String error = String.Empty;
            bool password = false;

            switch (res)
            {
                case -1: // error in connection
                    error = "There's a problem. Check your connection and try again";
                    break;
                case 0: // if subscription is successful
                    password = UIController.Proxy.ChangePassword(_username, _invitationCode, _password);
                    if (password)
                    {
                        Setting.Default.ProxyHost = _host;
                        Uri proxyUri = new Uri(_host);
                        Setting.Default.ProxyRoot = proxyUri.Scheme + "://" + proxyUri.Authority;
                        Setting.Default.UserName = _username;
                        Setting.Default.UserEmail = _email;
                        Setting.Default.Save();
                        UIController.Password = _password;
                        errorRowHeight = new GridLength(0);
                        UIController.Connected = true;
                    }
                    else
                    {
                        error = "There's a problem. Check your connection and try again";
                    }
                    break;
                case 1: // if e-mail address does not exist in the database
                    error = "Please enter the email address where you received the invite";
                    break;
                case 2: // if password does not match with the e-mail address sent
                    error = "Please enter the invitation code that you recived in the invite";
                    break;
                case 3: // if username is already used by another user
                    error = "The Username chosen is not available";
                    break;
            }
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                UIController.HidePanel(loading);
                ErrorRow.Height = GridLength.Auto;
                if (res == 0 && password)
                    UIController.ShowMainPanel();
                else
                {
                    ErrorBorder.Background = Brushes.Red;
                    ErrorLabel.Content = error;
                }
            }));
        }

        private void ProxyHost_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ProxyHost.Text))
            {
                HostErrorRow.Height = new GridLength(0);
                if (!String.IsNullOrEmpty(UserName.Text))
                {
                    UsernameErrorBorder.Background = Brushes.Red;
                    UsernameErrorRow.Height = GridLength.Auto;
                    UsernameErrorLabel.Content = "Enter a valid Proxy Host address to check if the Username is available";
                }
            }
            else
            {
                ProxyHost.IsEnabled = false;
                LoadingProxyHost.Visibility = Visibility.Visible;
                _host = UIController.GetProxyUriFormatted(ProxyHost.Text);
                ThreadStart starter = delegate
                {
                    try
                    {
                        CheckProxyHost();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                };
                Thread checkProxyHostThread = new Thread(starter);
                checkProxyHostThread.Name = "Check Proxy Host";
                checkProxyHostThread.Start();
            }
        }

        /// <summary>
        /// Check if the Proxy Host entered is correct.
        /// </summary>
        private void CheckProxyHost()
        {
            String error = string.Empty;
            try
            {
                UIController.Proxy = new SocialTFSProxyClient(_host);

                if (!UIController.Proxy.IsWebServiceRunning())
                {
                    error = "The connection with the Proxy failed";
                    UIController.Proxy = null;
                }
            }
            catch (UriFormatException)
            {
                error = "Server URI is not in a correct form";
                UIController.Proxy = null;
            }

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                HostErrorBorder.Background = Brushes.Red;
                HostErrorRow.Height = GridLength.Auto;
                HostErrorLabel.Content = error;
                UsernameErrorRow.Height = new GridLength(0);
                if (UIController.Proxy == null)
                {
                    if (!UserName.Text.Equals(String.Empty))
                    {
                        UsernameErrorBorder.Background = Brushes.Red;
                        UsernameErrorRow.Height = GridLength.Auto;
                        UsernameErrorLabel.Content = "Enter a valid Proxy Host address to check if the Username is available";
                    }
                }
                else
                {
                    HostErrorBorder.Background = Brushes.Green;
                    HostErrorRow.Height = GridLength.Auto;
                    HostErrorLabel.Content = "Proxy Host correct";
                    if (!UserName.Text.Equals(String.Empty))
                    {
                        _username = UserName.Text;

                        ThreadStart starter = delegate
                        {
                            try
                            {
                                CheckUsername();
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.Message, ex);
                            }
                        };
                        Thread checkUsernameThread = new Thread(starter);
                        checkUsernameThread.Name = "Check Username";
                        checkUsernameThread.Start();
                    }
                }
                LoadingProxyHost.Visibility = Visibility.Hidden;
                ProxyHost.IsEnabled = true;
            }));
        }

        private void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!UserName.Text.Equals(String.Empty))
            {
                if (UIController.Proxy != null)
                {
                    UserName.IsEnabled = false;
                    LoadingUserName.Visibility = Visibility.Visible;
                    _username = UserName.Text;

                    ThreadStart starter = delegate
                    {
                        try
                        {
                            CheckUsername();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message, ex);
                        }
                    };
                    Thread checkUsernameThread = new Thread(starter);
                    checkUsernameThread.Name = "Check Username";
                    checkUsernameThread.Start();
                }
                else
                {
                    UsernameErrorBorder.Background = Brushes.Red;
                    UsernameErrorRow.Height = GridLength.Auto;
                    UsernameErrorLabel.Content = "Enter a valid Proxy Host address to check if the Username is available";
                }
            }
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckPassword();
        }

        /// <summary>
        /// Check if the Username is available.
        /// </summary>
        private void CheckUsername()
        {
            String error = String.Empty;
            Brush backgroundColor;

            if (UIController.Proxy.IsAvailable(_username))
            {
                error = "Username available";
                backgroundColor = Brushes.Green;
            }
            else
            {
                error = "The Username is not available";
                backgroundColor = Brushes.Red;
            }

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                UsernameErrorBorder.Background = backgroundColor;
                UsernameErrorRow.Height = GridLength.Auto;
                UsernameErrorLabel.Content = error;
                LoadingUserName.Visibility = Visibility.Hidden;
                UserName.IsEnabled = true;
            }));
        }

        /// <summary>
        /// Check if the two password typed are equal and display an error if they are different.
        /// </summary>
        /// <returns>True if the the passwords matches, false otherwise.</returns>
        private bool CheckPassword()
        {
            if (!String.IsNullOrEmpty(Password.Password) && !String.IsNullOrEmpty(ConfirmPassword.Password))
            {
                if (Password.Password == ConfirmPassword.Password)
                {
                    ErrorBorder.Background = Brushes.Green;
                    ErrorRow.Height = new GridLength(0);
                    ErrorLabel.Content = String.Empty;
                    return true;
                }
                else
                {
                    ErrorBorder.Background = Brushes.Red;
                    ErrorRow.Height = GridLength.Auto;
                    ErrorLabel.Content = "Passwords do not match";
                    return false;
                }
            }
            return false;
        }

    }
}
