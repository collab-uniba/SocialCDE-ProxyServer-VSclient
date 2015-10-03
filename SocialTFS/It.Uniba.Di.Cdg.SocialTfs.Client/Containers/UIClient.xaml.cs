using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Containers
{

    /// <summary>
    /// Interaction logic for UIClient.xaml.
    /// </summary>
    public partial class UIClient : UserControl
    {

        #region Attributes

        private Panels.UIMicroblog _homeTimeline;
        private Panels.UIMicroblog _dynamicNetworkTimeline;
        private Panels.UIMicroblog _interactiveNetworkTimeline;
        private Panels.UIPeople _peoplePanel;
        private Panels.UIProfile _uiProfile;
        private Objects.UICustomButton _lastCheckedButton;
        private Popups.UILostConnection _lostConnection;
        private Dictionary<Objects.UICustomButton, Panels.UIPanel> panels;
        private Stack<Panels.UIPanel> _dynamicPanelsStack;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIClient()
        {
            InitializeComponent();

            // Create all panels
            _uiProfile = new Panels.UIProfile();
            _peoplePanel = new Panels.UIPeople();
            _homeTimeline = new Panels.UIMicroblog(Objects.UITimeline.TimelineType.HomeTimeline);
            _dynamicNetworkTimeline = new Panels.UIMicroblog(Objects.UITimeline.TimelineType.DynamicTimeline);
            _interactiveNetworkTimeline = new Panels.UIMicroblog(Objects.UITimeline.TimelineType.InteractiveNetworkTimeline);

            // Add panels to client
            ContentGrid.Children.Add(_uiProfile);
            ContentGrid.Children.Add(_peoplePanel);
            ContentGrid.Children.Add(_homeTimeline);
            ContentGrid.Children.Add(_dynamicNetworkTimeline);
            ContentGrid.Children.Add(_interactiveNetworkTimeline);

            // Save reference of panel linked
            panels = new Dictionary<Objects.UICustomButton, Panels.UIPanel>()
            {
                {ProfileButton, _uiProfile},
                {HomeTimelineButton, _homeTimeline},
                {IterationTimelineButton, _dynamicNetworkTimeline},
                {InteractiveTimelineButton, _interactiveNetworkTimeline},
                {PeopleButton, _peoplePanel}
            };

            if (HaveOneRegisteredService())
            {
                HomeTimelineButton.IsChecked = true;
                SaveCheckedButton(HomeTimelineButton);
                _homeTimeline.Open();
                _uiProfile.Visibility = Visibility.Hidden;
            }
            else
            {
                ProfileButton.IsChecked = true;
                SaveCheckedButton(ProfileButton);
                _uiProfile.Open();
                _homeTimeline.Visibility = Visibility.Hidden;
            }
            _dynamicNetworkTimeline.Visibility = Visibility.Hidden;
            _interactiveNetworkTimeline.Visibility = Visibility.Hidden;
            _peoplePanel.Visibility = Visibility.Hidden;
            _dynamicPanelsStack = new Stack<Panels.UIPanel>();

            // Load images on buttons
            if (String.IsNullOrEmpty(UIController.MyProfile.Avatar))
                ProfileButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/DefaultAvatar.png"));
            else
                ProfileButton.InternalImage.Source = new BitmapImage(new Uri(UIController.MyProfile.Avatar));
            HomeTimelineButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Home.png"));
            IterationTimelineButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/IterationTimeline.png"));
            InteractiveTimelineButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/InteractiveTimeline.png"));
            PeopleButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/People.png"));
            BackPanelButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Back.png"));
        }

        private bool HaveOneRegisteredService()
        {
            bool result = false;

            WService[] services = UIController.Proxy.GetServices(UIController.MyProfile.Username, UIController.Password);
            foreach (WService s in services)
            {
                if (s.Registered)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Save the button checked as the last checked button.
        /// </summary>
        /// <param name="button">The button.</param>
        private void SaveCheckedButton(Objects.UICustomButton button)
        {
            Contract.Requires(button != null);

            if (_lastCheckedButton != null && _lastCheckedButton != button)
                _lastCheckedButton.IsChecked = false;

            _lastCheckedButton = button;
        }

        /// <summary>
        /// Add a panel dynamically over the panel currently shown.
        /// </summary>
        /// <remarks>
        /// This method allows to create a stack of panels in way to navigate on the panels in a similar manner on the web page browsing.
        /// </remarks>
        /// <param name="panel">The panel to add.</param>
        internal void AddPanel(Panels.UIPanel panel)
        {
            Contract.Requires(panel != null);

            panels[_lastCheckedButton].Visibility = Visibility.Hidden;
            
            if (_dynamicPanelsStack.Count == 0)
                panels[_lastCheckedButton].Close();
            else
                _dynamicPanelsStack.Peek().Close();

            _lastCheckedButton.IsChecked = false;
            ContentGrid.Children.Add(panel);
            _dynamicPanelsStack.Push(panel);
            panel.Open();
            DynamicToolbar.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Cames back to the previous panel. 
        /// </summary>
        private void BackPanelButton_Click(object sender, RoutedEventArgs e)
        {
            BackPanelButton.IsChecked = false;
            _dynamicPanelsStack.Peek().Close();
            ContentGrid.Children.Remove(_dynamicPanelsStack.Pop());
            if (_dynamicPanelsStack.Count == 0)
            {
                DynamicToolbar.Visibility = Visibility.Collapsed;
                _lastCheckedButton.IsChecked = true;
                panels[_lastCheckedButton].Open();
                panels[_lastCheckedButton].Visibility = Visibility.Visible;
            }
            else
            {
                _dynamicPanelsStack.Peek().Open();
            }
        }

        /// <summary>
        /// Close all panel in the stack of panels.
        /// </summary>
        private void CloseDynamicPanels()
        {
            while (_dynamicPanelsStack.Count > 0)
            {
                _dynamicPanelsStack.Peek().Close();
                ContentGrid.Children.Remove(_dynamicPanelsStack.Pop());
            }

            _dynamicPanelsStack.Clear();
            DynamicToolbar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Manage the click of each button in the toolbar.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Contract.Requires(sender.GetType() == typeof(Objects.UICustomButton));

            CloseDynamicPanels();
            if (_lastCheckedButton != null)
            {
                panels[_lastCheckedButton].Visibility = Visibility.Hidden;
                panels[_lastCheckedButton].Close();
            }
            SaveCheckedButton(sender as Objects.UICustomButton);
            panels[_lastCheckedButton].Open();
            panels[_lastCheckedButton].Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// Reload the Interactive Timeline throws the wake up of the panel.
        /// </summary>
        internal void ReloadInteractiveTimeline()
        {
            if (_interactiveNetworkTimeline != null && panels[InteractiveTimelineButton].IsVisible)
                panels[InteractiveTimelineButton].WakeUp();
        }

        /// <summary>
        /// Show a panel that manage the losing of the connection.
        /// </summary>
        internal void ShowLostConnection()
        {
            if (_lostConnection == null)
            {
                _lostConnection = new Popups.UILostConnection();
                _lostConnection.WorkOffline.Click += WorkOffline_Click;
                _lostConnection.Reconnect.Click += Reconnect_Click;
                UIController.ShowPanel(_lostConnection);
            }
        }

        /// <summary>
        /// Try to reconnect after a disconnection.
        /// </summary>
        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            panels[_lastCheckedButton].Open();
            UIController.HidePanel(_lostConnection);
            _lostConnection = null;
        }

        /// <summary>
        /// Allows to work without a connection.
        /// </summary>
        private void WorkOffline_Click(object sender, RoutedEventArgs e)
        {
            UIController.Connected = false;
            _uiProfile.ExitButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Reconnect.png"));
            _uiProfile.ExitButton.ToolTip = "Reconnect";
            UIController.HidePanel(_lostConnection);
            _lostConnection = null;
        }

        /// <summary>
        /// Exit from the application and logout.
        /// </summary>
        internal void Exit()
        {
            if (UIController.Connected)
                UIController.Logout();
            else
            {
                UIController.Connected = true;
                panels[_lastCheckedButton].Open();
                _uiProfile.ExitButton.InternalImage.Source = new BitmapImage(new Uri("pack://application:,,,/It.Uniba.Di.Cdg.SocialTfs.Client;component/Images/Toolbar/Logout.png"));
                _uiProfile.ExitButton.ToolTip = "Logout";
            }
        }

        /// <summary>
        /// Logout and show the login panel.
        /// </summary>
        internal void LogOut()
        {
            CloseDynamicPanels();
            foreach (Panels.UIPanel item in panels.Values)
                item.Close();
        }

        #endregion
    }
}
