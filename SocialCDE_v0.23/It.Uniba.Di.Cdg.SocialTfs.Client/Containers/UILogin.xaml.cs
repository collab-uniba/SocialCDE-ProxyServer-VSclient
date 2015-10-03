using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Diagnostics.Contracts;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Containers
{
    /// <summary>
    /// Interaction logic for UILogin.xaml.
    /// </summary>
    public partial class UILogin : UserControl
    {
        // Log4Net reference
        private static readonly ILog log = 
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor.
        /// </summary>
        public UILogin()
        {
            InitializeComponent();
            ProxyHost.Text = Setting.Default.ProxyRoot;
            Username.Text = Setting.Default.UserName;
            SavePassword.IsChecked = Setting.Default.SavePassword;
            if (Setting.Default.SavePassword)
            {
                Password.Password = Setting.Default.Password;
                AutoLogin.IsChecked = Setting.Default.AutoLogin;
                if (Setting.Default.AutoLogin)
                    Login_Click(null, null);
            }
            else
            {
                AutoLogin.IsEnabled = false;
            }
        }

        private void ShowRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            UIController.ShowRegistrationPanel();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorRow.Height = new GridLength(0);
            String formattedHost = UIController.GetProxyUriFormatted(ProxyHost.Text);
            String username = Username.Text;
            String password = Password.Password;
            bool savePassword = SavePassword.IsChecked.Value;
            bool autoLogin = AutoLogin.IsChecked.Value;
            ThreadStart starter = delegate 
            {
                try
                {
                    Login(formattedHost, username, password, savePassword, autoLogin);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            };
            Thread loginThread = new Thread(starter);
            loginThread.Name = "Login";
            loginThread.Start();
        }

        private void Login(String formattedHost, String username, String password, bool savePassword, bool autoLogin)
        {
            Contract.Requires(String.IsNullOrEmpty(formattedHost));
            Contract.Requires(String.IsNullOrEmpty(username));
            Contract.Requires(String.IsNullOrEmpty(password));

            Popups.UILoading loading = null;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                loading = new Popups.UILoading("Login in progress");
                UIController.ShowPanel(loading);
            }));

            String error = String.Empty;
            try
            {
                UIController.Proxy = new SocialTFSProxyClient(formattedHost);

                if (UIController.Proxy.IsWebServiceRunning())
                {
                    WUser user = UIController.Proxy.GetUser(username, password);
                    if (user == null)
                        error = "Username or Password is not correct";
                    else
                    {
                        Setting.Default.ProxyHost = formattedHost;
                        Uri proxyUri = new Uri(formattedHost);
                        Setting.Default.ProxyRoot = proxyUri.Scheme + "://" + proxyUri.Authority;
                        Setting.Default.UserName = username;
                        Setting.Default.Password = password;
                        Setting.Default.SavePassword = savePassword;
                        if (savePassword)
                        {
                            Setting.Default.AutoLogin = autoLogin;
                        }
                        else
                        {
                            Setting.Default.Password = "";
                            Setting.Default.AutoLogin = false;
                        }
                        Setting.Default.Save();
                        UIController.Password = password;
                        UIController.Connected = true;
                    }
                }
                else
                {
                    error = "The connection with the Proxy failed";
                }
            }
            catch (UriFormatException)
            {
                error = "Server URI is not in a correct form";
                UIController.Proxy = null;
            }

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                UIController.HidePanel(loading);
                if (error.Equals(String.Empty))
                {
                    UIController.ShowMainPanel();
                }
                else
                {
                    ErrorLabel.Content = error;
                    ErrorRow.Height = GridLength.Auto;
                }
                if (!savePassword)
                    Password.Clear();
            }));
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked.Value)
            {
                AutoLogin.IsEnabled = true;
            }
            else
            {
                AutoLogin.IsChecked = false;
                AutoLogin.IsEnabled = false;
            }
        }

    }
}
