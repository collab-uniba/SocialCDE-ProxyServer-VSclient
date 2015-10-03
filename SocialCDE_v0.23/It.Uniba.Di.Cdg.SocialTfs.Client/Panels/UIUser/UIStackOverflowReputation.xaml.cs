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
    /// Logica di interazione per UIStackOverflowReputation.xaml
    /// </summary>
    public partial class UIStackOverflowReputation : UserControl
    {
        public UIStackOverflowReputation()
        {
            InitializeComponent();
        }

        internal void Update(WReputation repu)
        {
            try
            {
                lblanswers.Content = repu.stackAnswer;
                lblquestions.Content = repu.stackQuestion;
                lblreputation.Content = repu.stackReputationValue;
                lblbronze.Content = repu.stackBronze;
                lblsilver.Content = repu.stackSilver;
                lblgold.Content = repu.stackGold;
            }
            catch (Exception)
            {
               
            }    
            
        }
    }
}
