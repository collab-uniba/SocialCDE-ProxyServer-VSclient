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
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels.UIUser
{
    /// <summary>
    /// Logica di interazione per UIHeader.xaml
    /// </summary>
    public partial class UIHeader : UserControl
    {
        private SharedLibrary.WUser _user;
        public int? metascore;
        public UIHeader()
        {
            InitializeComponent();
            }

        internal void Update(string displayName, string username, string mainJob , String uri, double score)
        {
            
            try
            {
                imgHeader.Source = new BitmapImage(new Uri(uri));
            }
            catch (Exception)
            {

            }
            
            txtDisplayName.Text = displayName;
            //calcolo metascore
            lblmetascore.Content = score;
            //txtUsername.Text = username;
            //txtMainJob.Text = mainJob;
        }
    }
}
