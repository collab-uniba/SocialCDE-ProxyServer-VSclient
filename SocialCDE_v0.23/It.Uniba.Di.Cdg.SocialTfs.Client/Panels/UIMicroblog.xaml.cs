using System;
using System.Diagnostics.Contracts;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Threading;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Interaction logic for UIMicroblog.xaml.
    /// </summary>
    public partial class UIMicroblog : UIPanel
    {
        #region Attributes

        internal bool _noOlderPost = false;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public UIMicroblog(Objects.UITimeline.TimelineType timelineType)
        {
            InitializeComponent();
            Timeline.LoadTimeline(UIController.MyProfile, timelineType);
            SendPost.OnMessageSended += SendPost_OnMessageSended;
        }

        public override void Open()
        {
            if (UIController.Connected)
            {
                Timeline.StartUpdates();
                SendPost.IsEnabled = true;
            }
            else
            {
                SendPost.IsEnabled = false;
            }
        }

        public override void Close()
        {
            Timeline.StopUpdates();
        }

        public override void WakeUp()
        {
            Timeline.UpdateThread();
        }

        private void SendPost_OnMessageSended(object sender, EventArgs e)
        {
            WakeUp();
        }

        #endregion
    }
}
