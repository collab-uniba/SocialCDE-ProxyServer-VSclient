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
    /// Logica di interazione per UIEducation.xaml
    /// </summary>
    public partial class UIEducation : UserControl
    {
        public UIEducation() { InitializeComponent(); }

        public UIEducation(UISingleEducation[] experiences)
        {
            InitializeComponent();
        }

        internal void Update(WEdu[] experiences)
        {
            stkEducation.Children.Clear();

            foreach (WEdu item in experiences)
            {
                stkEducation.Children.Add(new UISingleEducation(item));
            }
        }
    }
}
