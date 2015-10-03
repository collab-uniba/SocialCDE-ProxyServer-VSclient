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
    /// Logica di interazione per UISingleExperience.xaml
    /// </summary>
    public partial class UISingleExperience : UserControl
    {
        WPos pos;

        public UISingleExperience() { }

        public UISingleExperience(WPos pos)
        {
            InitializeComponent();

            this.pos = pos;
            txtJob.Text = pos.title;
            txtArea.Text = pos.name;
            txtCompany.Text = pos.industry;
        }
    }
}
