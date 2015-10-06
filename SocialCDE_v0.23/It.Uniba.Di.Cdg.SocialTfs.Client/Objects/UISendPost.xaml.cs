using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Interaction logic for UISendPost.xaml.
    /// </summary>
    public partial class UISendPost : UserControl
    {
        /// <summary>
        /// Event activated when the user posts a message.
        /// </summary>
        public event EventHandler OnMessageSended;

        public UISendPost()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sends the post when the enter key is pressed in the post textbox.
        /// </summary>
        /// <param name="sender">Textbox related.</param>
        /// <param name="e">Event arguments.</param>
        private void PostTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !string.IsNullOrEmpty(PostTextBox.Text))
            {
                if (UIController.Proxy.Post(UIController.MyProfile.Username, UIController.Password, PostTextBox.Text))
                {
                    PostTextBox.Clear();
                    if (OnMessageSended != null)
                        OnMessageSended(this, new EventArgs());
                }
                else
                {
                    this.IsEnabled = false;
                }
            }
        }
    }
}
