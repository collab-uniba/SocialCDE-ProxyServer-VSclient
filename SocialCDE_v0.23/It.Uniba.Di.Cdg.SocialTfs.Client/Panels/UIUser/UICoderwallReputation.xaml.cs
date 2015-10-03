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
    /// Logica di interazione per UICoderwallReputation.xaml
    /// </summary>
    public partial class UICoderwallReputation : UserControl
    {
        public UICoderwallReputation()
        {
            InitializeComponent();
        }

        internal void Update(WReputation repu)
        {

            try
            {
                lblendorsements.Content = repu.coderwallEndorsements;
            }
            catch (Exception)
            {
                
            }
        }
    }
}
