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
    /// Logica di interazione per UISingleEducation.xaml
    /// </summary>
    public partial class UISingleEducation : UserControl
    {
        WEdu edu;

        public UISingleEducation() { }

        public UISingleEducation(WEdu edu)
        {
            InitializeComponent();

            this.edu = edu;
            txtField.Text = edu.fieldOfStudy;
            txtSchool.Text = edu.schoolName;
        }
    }
}
