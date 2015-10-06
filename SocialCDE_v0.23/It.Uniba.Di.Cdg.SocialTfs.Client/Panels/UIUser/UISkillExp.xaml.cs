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

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels.UIUser
{
    /// <summary>
    /// Logica di interazione per UISkillExp.xaml
    /// </summary>
    public partial class UISkillExp : UserControl
    {
        public UISkillExp() { InitializeComponent(); }

        public void Update(string[] skills)
        {
            wrapExperience.Children.Clear();

            foreach (string item in skills)
            {
                wrapExperience.Children.Add( new UISingleSkillExp( item ) );
            }
        }
    }
}
