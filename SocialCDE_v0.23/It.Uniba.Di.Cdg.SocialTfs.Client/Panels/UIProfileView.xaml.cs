using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using log4net;
using It.Uniba.Di.Cdg.SocialTfs.Client.Panels.UIUser;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UISocialNetwork.xaml
    /// </summary>
    public partial class UIProfileView : UIPanel
    {
        #region Attributes

        WUser _user = null;

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        public UIProfileView(WUser user)
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

            
          
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                SkillsPanel.Children.Clear();
                SkillsPanel.Children.Add ( new UIDevelopers( _user));
                UIController.HidePanel(loading);
            }));
        }
    }
}
