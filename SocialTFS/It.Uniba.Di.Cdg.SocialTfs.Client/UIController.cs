using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualStudio.TeamFoundation;
using EnvDTE;
using EnvDTE80;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Diagnostics;
using SHDocVw;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.Client
{
    /// <summary>
    /// Manage all the add-in interfaces.
    /// </summary>
    public static class UIController
    {
        /// <summary>
        /// Type of Visual Studio document.
        /// </summary>
        public enum DocumentType
        {
            File,
            WorkItem
        }

        #region Attribute

        private static ElementHost _elementHost = null;
        private static Window _toolWindow = null;
        private static AddIn _addInInstance;
        private static DTE2 _applicationObject;
        private static bool _inizialized = false;

        private static ISocialTFSProxy _proxy;
        private static Containers.UILogin _uiLogin;
        private static Containers.UIRegistration _uiRegistration;
        private static Containers.UIClient _uiClient;
        private static Grid _mainGrid;
        private static System.Windows.UIElement _mainPanel;

        private static readonly ILog log = LogManager.GetLogger("Unandled Exceptions");

        #endregion

        #region Properties

        internal static bool Connected { get; set; }
        internal static WUser MyProfile { get; set; }
        internal static String Password { get; set; }

        /// <summary>
        /// Get the Login panel, if the panel doesn't exist, it will be created.
        /// </summary>
        private static Containers.UILogin UILogin
        {
            get
            {
                if (_uiLogin == null)
                    _uiLogin = new Containers.UILogin();

                return _uiLogin;
            }
        }

        internal static Dispatcher UiDispacher
        {
            get
            {
                return _mainGrid.Dispatcher;
            }
        }

        /// <summary>
        /// Get the main panel as a grid, if the panel doesn't exist, it will be created.
        /// </summary>
        private static Grid MainGrid
        {
            get
            {
                if (_mainGrid == null)
                    _mainGrid = new Grid();

                return _mainGrid;
            }
        }

        /// <summary>
        /// Get the Registration panel, if the panel doesn't exist, it will be created.
        /// </summary>
        private static Containers.UIRegistration UIRegistration
        {
            get
            {
                if (_uiRegistration == null)
                    _uiRegistration = new Containers.UIRegistration();

                return _uiRegistration;
            }
        }

        public static ISocialTFSProxy Proxy
        {
            get
            {
                return _proxy;
            }

            internal set
            {
                _proxy = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the add-in.
        /// </summary>
        /// <param name="application">Application reference.</param>
        /// <param name="addInInstance">Add-in instance reference.</param>
        public static void Initialize(DTE2 application, AddIn addInInstance)
        {
            // Initializa Apache Log4Net system
            string assembliPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string execPath = System.IO.Path.GetDirectoryName(assembliPath);
            string config = execPath + "\\log4net.config";
            XmlConfigurator.Configure(new Uri(config));
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ThreadExceptionHandler);

            _applicationObject = application;
            _addInInstance = addInInstance;
            _inizialized = true;
        }

        /// <summary>
        /// Catch all unhandled exception.
        /// This is necessary to avoid the crash on the IDE when an unhandled exception is raised.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Args</param>
        static void ThreadExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            log.Error("IMPORTANT: This thread should be catched in its start method!", e);
        }

        /// <summary>
        /// Check if the add-in is initialized.
        /// </summary>
        public static void CheckInizialization()
        {
            if (!_inizialized)
                throw new Exception("The class is not inizialized!");
        }

        /// <summary>
        /// Start the startup procedures to show the add-in.
        /// </summary>
        /// <remarks>
        /// Initialize the user control containing the interfeces.
        /// </remarks>
        public static void ShowToolWindow()
        {
            CheckInizialization();

            if (_toolWindow == null)
            {
                object objectUserControl = null;
                Windows2 windows = _applicationObject.Windows as Windows2;
                string assemblyLocation = Assembly.GetCallingAssembly().Location;
                string windowID = "{2601CB6B-6DCA-4D7F-97B6-65B57801E887}";
                string className = "It.Uniba.Di.Cdg.SocialTfs.Client.ClientUserControl";

                _toolWindow = windows.CreateToolWindow2(_addInInstance,
                    assemblyLocation, className, "SocialTFS", windowID, ref objectUserControl);

                _toolWindow.IsFloating = false;

                ClientUserControl clientUserControl = objectUserControl as ClientUserControl;
                _elementHost = clientUserControl.elementHost;
                _elementHost.Child = MainGrid;

                _toolWindow.Visible = true;
                if (!IsRegistred())
                    ShowRegistrationPanel();
                else
                    ShowLoginPanel();
            }
            else
            {
                _toolWindow.Visible = true;
            }
        }

        /// <summary>
        /// Show the panel for the login.
        /// </summary>
        public static void ShowLoginPanel()
        {
            if (_mainPanel != null)
                MainGrid.Children.Remove(_mainPanel);
            _mainPanel = UILogin;
            MainGrid.Children.Add(_mainPanel);
        }

        /// <summary>
        /// Check if there is an account stored in the setting file.
        /// </summary>
        /// <returns>True if there is an account stored, False otherwise.</returns>
        private static bool IsRegistred()
        {
            return (!String.IsNullOrEmpty(Setting.Default.UserName)
                && !String.IsNullOrEmpty(Setting.Default.ProxyHost));
        }

        /// <summary>
        /// Show the panel for the registration.
        /// </summary>
        public static void ShowRegistrationPanel()
        {
            if (_mainPanel != null)
                MainGrid.Children.Remove(_mainPanel);
            _mainPanel = UIRegistration;
            MainGrid.Children.Add(_mainPanel);
        }

        /// <summary>
        /// Show the panel for the Profile.
        /// </summary>
        public static void ShowMainPanel()
        {
            MyProfile = Proxy.GetUser(Setting.Default.UserName, Password);
            _uiClient = new Containers.UIClient();
            _applicationObject.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;
            if (_mainPanel != null)
                MainGrid.Children.Remove(_mainPanel);
            _mainPanel = _uiClient;
            MainGrid.Children.Add(_mainPanel);
        }

        static void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            if (GotFocus.Kind == "Document")
                _uiClient.ReloadInteractiveTimeline();
        }

        public static string GetProxyUriFormatted(string host)
        {
            if (!host.StartsWith(@"http://"))
                host = @"http://" + host;
            if (!host.EndsWith(@"SocialTFSProxy.svc"))
            {
                if (host.EndsWith(@"/"))
                    host = host + @"SocialTFSProxy.svc";
                else
                    host = host + @"/SocialTFSProxy.svc";
            }

            return host;
        }

        internal static void ShowPanel(UserControl panel)
        {
            _mainPanel.IsEnabled = false;
            _mainPanel.Opacity = 0.2f;
            MainGrid.Children.Add(panel);
        }

        internal static void HidePanel(UserControl panel)
        {
            MainGrid.Children.Remove(panel);
            _mainPanel.Opacity = 1f;
            _mainPanel.IsEnabled = true;
        }

        /// <summary>
        /// Show lost connection warning.
        /// </summary>
        public static void ShowLostConnection()
        {
            _uiClient.ShowLostConnection();
        }

        /// <summary>
        /// Add stack panel on top of client.
        /// </summary>
        /// <param name="userControlPanel">Panel to add</param>
        public static void AddPanel(Panels.UIPanel userControlPanel)
        {
            Contract.Requires(userControlPanel != null);

            _uiClient.AddPanel(userControlPanel);
        }

        /// <summary>
        /// Requires to open a web page in the default browser. 
        /// </summary>
        /// <param name="link">Link to the web page.</param>
        public static SHDocVw.WebBrowser Browse(string link)
        {
            Contract.Requires(Uri.IsWellFormedUriString(link, UriKind.Absolute));

            _applicationObject.ItemOperations.Navigate(link, vsNavigateOptions.vsNavigateOptionsNewWindow);
            return _applicationObject.Windows.Item(Constants.vsWindowKindWebBrowser).Object as SHDocVw.WebBrowser;
        }

        /// <summary>
        /// Return the current URL in the default browser.
        /// </summary>
        /// <returns>Current URL.</returns>
        public static String CurrentURL()
        {
            try
            {
                return (_applicationObject.Windows.Item(Constants.vsWindowKindWebBrowser).Object as SHDocVw.WebBrowser).LocationURL;
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Return the current collection in Team Explorer.
        /// </summary>
        /// <returns>A list of ids.</returns>
        internal static String GetCurrentCollectionURL()
        {
            return ((TeamFoundationServerExt)_applicationObject.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt")).ActiveProjectContext.DomainUri;
        }

        /// <summary>
        /// Return the file or work items the user is currently interacting with.
        /// </summary>
        /// <returns>The object identifier and type.</returns>
        internal static Tuple<string, string> GetActiveObject()
        {
            Document document = _applicationObject.ActiveDocument;

            if (document != null)
            {
                switch (document.Kind)
                {
                    case "{40A91D9D-8076-4D28-87C5-5AF9F0ACFE0F}":
                        return new Tuple<string, string>(document.Name, DocumentType.WorkItem.ToString());
                    case "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}":
                        if (_applicationObject.SourceControl.IsItemUnderSCC(document.FullName))
                        {
                            SourceControlBindings bindings = (_applicationObject.DTE.SourceControl as EnvDTE80.SourceControl2).GetBindings(document.ProjectItem.ContainingProject.FullName);
                            string path = bindings.ServerBinding + document.FullName.ToLower().Replace(bindings.LocalBinding.ToLower(), "").Replace("\\", "/");
                            return new Tuple<string, string>(path, DocumentType.File.ToString());
                        }
                        else
                        {
                            string fullFileName = document.Name;
                            string fileName = string.Empty;
                            //File mapped or not in history mode open more times (like "name;C1234.00.cs or name.xaml;C1234.00.cs")
                            if (Regex.IsMatch(fullFileName, @"^[0-9a-zA-Z\.]+\;C[0-9]+\.[0-9]+\.[0-9a-zA-Z\.]+$"))
                            {
                                string[] splittedFullFileName = fullFileName.Split(new string[] { ";C" }, StringSplitOptions.None);
                                string[] splittedRightPart = splittedFullFileName[1].Split('.');
                                fileName = splittedFullFileName[0] + "." + splittedRightPart[2];
                            }
                            //File mapped or not in history mode (like "name;C1234.cs or name.xaml;C1234.cs")
                            else if (Regex.IsMatch(fullFileName, @"^[0-9a-zA-Z\.]+\;C[0-9]+\.[0-9a-zA-Z\.]+$"))
                            {
                                string[] splittedFullFileName = fullFileName.Split(new string[] { ";C" }, StringSplitOptions.None);
                                string[] splittedRightPart = splittedFullFileName[1].Split('.');
                                fileName = splittedFullFileName[0] + "." + splittedRightPart[1];
                            }
                            //File mapped associated to the last changeset with the solution closed (like "name.cs" or "name.xaml.cs")
                            else if (Regex.IsMatch(fullFileName, @"^[0-9a-zA-Z]+\.[0-9a-zA-Z\.]+$"))
                            {
                                fileName = fullFileName;
                            }
                            //File not mapped associated to the last changeset (like "name(C1234).cs or name.xaml(C1234).cs")
                            else if (Regex.IsMatch(fullFileName, @"^[0-9a-zA-Z\.]+\(C[0-9]+\).[0-9a-zA-Z]+$"))
                            {
                                string[] splittedFullFileName = fullFileName.Split(new string[] { "(C" }, StringSplitOptions.None);
                                string[] splittedRightPart = splittedFullFileName[1].Split(')');
                                fileName = splittedFullFileName[0] + splittedRightPart[1];
                            }
                            return new Tuple<string, string>(fileName, DocumentType.File.ToString());
                        }
                }
            }

            return null;
        }

        internal static void Exit()
        {
            _uiClient.Exit();
        }

        internal static void Logout()
        {
            MyProfile = null;
            Password = string.Empty;
            Connected = false;
            Proxy = null;
            if (_uiClient != null)
            {
                _uiClient.LogOut();
                _applicationObject.Events.WindowEvents.WindowActivated -= WindowEvents_WindowActivated;
                _uiClient = null;
            }
            UILogin.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    _uiLogin.Opacity = 1f;
                    _uiLogin.IsEnabled = true;
                    ShowLoginPanel();
                }));
        }

        internal static void ChangeProfileButtonImage(Uri avatar)
        {
            _uiClient.ProfileButton.InternalImage.Source = new BitmapImage(avatar);
        }

        #endregion
    }
}
