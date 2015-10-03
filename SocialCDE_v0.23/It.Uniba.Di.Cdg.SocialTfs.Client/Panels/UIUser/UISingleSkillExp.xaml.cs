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
    /// Logica di interazione per UISingleSkillExp.xaml
    /// </summary>
    public partial class UISingleSkillExp : UserControl
    {
        public static readonly DependencyProperty _SkillContent =
            DependencyProperty.Register("SkillContent", typeof(String),
            typeof(UISingleSkillExp), new FrameworkPropertyMetadata(string.Empty));

        public String SkillContent 
        {
            get { return GetValue(_SkillContent).ToString(); }
            set { SetValue(_SkillContent, value); } 
        }

        public UISingleSkillExp()
        {
            InitializeComponent();

            RefreshGUI();
        }

        public UISingleSkillExp(String text)
        {
            InitializeComponent();

            SkillContent = text;
            RefreshGUI();
        }

        public void RefreshGUI()
        {
            if (SkillContent == "")
                content.Content = "ND";
            else
                content.Content = SkillContent;
        }
    }
}
