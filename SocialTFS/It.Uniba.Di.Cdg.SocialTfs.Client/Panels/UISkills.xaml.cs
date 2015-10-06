using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UISocialNetwork.xaml
    /// </summary>
    public partial class UISkills : UIPanel
    {
        #region Attributes

        WUser _user = null;

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        public UISkills(WUser user)
        {
            InitializeComponent();
            _user = user;
        }

        public override void Open()
        {
            ThreadStart starter = delegate
            {
                try
                {
                    AsyncUpdate();
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            };
            Thread asyncUpdateThread = new Thread(starter);
            asyncUpdateThread.Name = "Async Update";
            asyncUpdateThread.Start();
        }

        public void AsyncUpdate()
        {
            Popups.UILoading loading = null;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                loading = new Popups.UILoading("Loading Info");
                UIController.ShowPanel(loading);
            }));

            string[] skills = UIController.Proxy.GetSkills(UIController.MyProfile.Username, UIController.Password, _user.Username);

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                if (skills.Length != 0)
                    SkillsPanel.Children.Clear();

                foreach (string skill in skills)
                {
                    TextBlock text = new TextBlock();
                    text.Text = skill;
                    text.Margin = new Thickness(5, 0, 0, 0);
                    SkillsPanel.Children.Add(text);
                }

                UIController.HidePanel(loading);
            }));
        }
    }
}
