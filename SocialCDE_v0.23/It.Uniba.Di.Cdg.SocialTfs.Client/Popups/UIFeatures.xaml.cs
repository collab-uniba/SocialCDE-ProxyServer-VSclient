using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Popups
{
    /// <summary>
    /// Interaction logic for UIFeatures.xaml.
    /// </summary>
    public partial class UIFeatures : UserControl
    {
        private WService _service;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="service">The service which provides features.</param>
        /// <param name="firstAccess">True if the user access for the first time, false otherwise.</param>
        public UIFeatures(WService service, bool firstAccess)
        {
            _service = service;
            InitializeComponent();
            Logo.Source = new BitmapImage(new Uri(Setting.Default.ProxyRoot + _service.Image));

            //load the settings
            foreach (WFeature item in UIController.Proxy.GetChosenFeatures(UIController.MyProfile.Username, UIController.Password, _service.Id))
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = item.Name;
                checkBox.Content = item.Description;
                checkBox.Margin = new Thickness(5, 2, 5, 2);
                if (item.IsChosen || firstAccess)
                    checkBox.IsChecked = true;
                SettingsList.Children.Add(checkBox);
            }
        }

        /// <summary>
        /// Get the list of the features chosen by the user as a list of strings.
        /// </summary>
        /// <returns>A list of features.</returns>
        public List<String> ChosenFeatures()
        {
            List<String> chosenFeatures = new List<String>();
            foreach (CheckBox setting in SettingsList.Children)
            {
                if (setting.IsChecked.Value)
                    chosenFeatures.Add(setting.Name);
            }
            return chosenFeatures;
        }
    }
}