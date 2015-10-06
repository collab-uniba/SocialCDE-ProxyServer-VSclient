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
using It.Uniba.Di.Cdg.SocialTfs.Client.Objects.UIObject;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels.UIUser
{
    /// <summary>
    /// Logica di interazione per UIExperience.xaml
    /// </summary>
    public partial class UIExperience : UserControl
    {
        public UIExperience() { InitializeComponent(); }

        public UIExperience(SingleExperience[] experiences)
        {
            InitializeComponent();
        }

        internal void Update(WPos[] experiences)
        {
            stkExperience.Children.Clear();

            foreach (WPos item in experiences)
            {
                stkExperience.Children.Add(new UISingleExperience(item));
            }
        }
    }
}
