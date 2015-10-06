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
using System.Threading;
using log4net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    /// <summary>
    /// Interaction logic for UITimeline.xaml.
    /// </summary>
    public partial class UITimeline : UserControl
    {
        /// <summary>
        /// Type of timeline.
        /// </summary>
        public enum TimelineType
        {
            HomeTimeline,
            PersonalTimeline,
            DynamicTimeline,
            InteractiveNetworkTimeline
        }

        private WUser _owner;
        private Thread _updateThread;
        private TimelineType _timelineType;
        private Tuple<string, string> _interactiveObject;
        private long _first;
        private long _last;
        private object _lock;
        private bool _olderPostAllowed;

        // Log4Net reference
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UITimeline()
        {
            InitializeComponent();
            _lock = new object();
            _first = 0;
            _last = 0;
            _olderPostAllowed = true;
            _interactiveObject = new Tuple<string, string>(String.Empty, String.Empty);
        }

        internal void LoadTimeline(WUser user)
        {
            LoadTimeline(user, TimelineType.PersonalTimeline);
        }

        internal void LoadTimeline()
        {
            LoadTimeline(null, TimelineType.HomeTimeline);
        }

        internal void LoadTimeline(WUser user, TimelineType timelineType)
        {
            _owner = user;
            _timelineType = timelineType;

        }

        private void UpdateTimeline()
        {
            lock (_lock)
            {
                while (true)
                {
                    WPost[] posts = new WPost[0];
                    Popups.UILoading loading = null;

                    if (_first == 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            loading = new Popups.UILoading("Downloading posts");
                            UIController.ShowPanel(loading);
                        }));
                    }

                    switch (_timelineType)
                    {
                        case TimelineType.HomeTimeline:
                            posts = UIController.Proxy.GetHomeTimeline(UIController.MyProfile.Username, UIController.Password, _first);
                            break;
                        case TimelineType.PersonalTimeline:
                            posts = UIController.Proxy.GetUserTimeline(UIController.MyProfile.Username, UIController.Password, _owner.Username, _first);
                            break;
                        case TimelineType.DynamicTimeline:
                            posts = UIController.Proxy.GetIterationTimeline(UIController.MyProfile.Username, UIController.Password, _first);
                            break;
                        case TimelineType.InteractiveNetworkTimeline:
                            if (UIController.GetCurrentCollectionURL() != null)
                            {
                                Tuple<string, string> tempObject = UIController.GetActiveObject();
                                if (tempObject != null)
                                {
                                    if (tempObject.Item1 != _interactiveObject.Item1 || tempObject.Item2 != _interactiveObject.Item2)
                                    {
                                        _first = 0;
                                        _last = 0;
                                        this.Dispatcher.BeginInvoke(new Action(delegate()
                                            {
                                                UIController.HidePanel(loading);
                                                TimelineStackPanel.Children.Clear();
                                                loading = new Popups.UILoading("Downloading posts");
                                                UIController.ShowPanel(loading);
                                            }));
                                        _interactiveObject = tempObject;
                                    }
                                    posts = UIController.Proxy.GetInteractiveTimeline(UIController.MyProfile.Username, UIController.Password, UIController.GetCurrentCollectionURL(), _interactiveObject.Item1, _interactiveObject.Item2, _first);

                                    // Reload timeline if document change during posts download
                                    tempObject = UIController.GetActiveObject();
                                    if (tempObject.Item1 != _interactiveObject.Item1 || tempObject.Item2 != _interactiveObject.Item2)
                                    {
                                        this.Dispatcher.BeginInvoke(new Action(delegate()
                                            {
                                                UIController.HidePanel(loading);
                                            }));
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                _first = 0;
                                _last = 0;
                                this.Dispatcher.BeginInvoke(new Action(delegate()
                                {
                                    //TODO Displays a message informing the user that needs to connect to a TFS collection
                                    TimelineStackPanel.Children.Clear();
                                }));
                            }
                            break;
                    }

                    this.Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        if (posts.Length > 0)
                        {
                            _first = posts.First().Id;
                            if (_last == 0)
                                _last = posts.Last().Id;
                            for (int i = 0; i < posts.Length; i++)
                                if (_timelineType == TimelineType.PersonalTimeline)
                                    TimelineStackPanel.Children.Insert(i, new UIPost(posts[i], false));
                                else
                                    TimelineStackPanel.Children.Insert(i, new UIPost(posts[i], true));
                        }

                        UIController.HidePanel(loading);
                    }));

                    Monitor.Wait(_lock, new TimeSpan(0, 1, 0));
                }
            }
        }

        internal void UpdateThread()
        {
            if (_updateThread.ThreadState == ThreadState.WaitSleepJoin)
                lock (_lock)
                    Monitor.Pulse(_lock);
        }

        internal void StartUpdates()
        {
            _first = 0;
            _last = 0;
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                TimelineStackPanel.Children.Clear();
            }));
            if (_updateThread == null)
            {
                ThreadStart starter = delegate
                {
                    try
                    {
                        UpdateTimeline();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                };
                _updateThread = new Thread(starter);
                _updateThread.Name = "Update Timeline";
                _updateThread.Start();
            }
        }

        internal void StopUpdates()
        {
            if (_updateThread != null)
            {
                _updateThread.Abort();
                _updateThread = null;
            }
        }

        private void ContentScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ContentScroll.ScrollableHeight == 0)
            {
                LoadingText.Visibility = Visibility.Visible;
                LoadingText.Text = "There are no older posts";
            }
            else if (ContentScroll.VerticalOffset == ContentScroll.ScrollableHeight && _olderPostAllowed)
            {
                _olderPostAllowed = false;
                Busy.Visibility = Visibility.Visible;
                LoadingText.Visibility = Visibility.Visible;
                LoadingText.Text = "Loading posts";

                ThreadStart starter = delegate
                {
                    try
                    {
                        downloadOlderPosts();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message, ex);
                    }
                };
                Thread downloadOlderPostsThread = new Thread(starter);
                downloadOlderPostsThread.Name = "Download Older Posts";
                downloadOlderPostsThread.Start();
            }
        }

        private void downloadOlderPosts()
        {
            WPost[] posts = new WPost[0];

            switch (_timelineType)
            {
                case TimelineType.HomeTimeline:
                    posts = UIController.Proxy.GetHomeTimeline(
                        UIController.MyProfile.Username, UIController.Password, 0, _last);
                    break;
                case TimelineType.PersonalTimeline:
                    posts = UIController.Proxy.GetUserTimeline(
                        UIController.MyProfile.Username, UIController.Password, _owner.Username, 0, _last);
                    break;
                case TimelineType.DynamicTimeline:
                    posts = UIController.Proxy.GetIterationTimeline(
                        UIController.MyProfile.Username, UIController.Password, 0, _last);
                    break;
                case TimelineType.InteractiveNetworkTimeline:
                    posts = UIController.Proxy.GetInteractiveTimeline(
                        UIController.MyProfile.Username, UIController.Password, UIController.GetCurrentCollectionURL(), 
                        _interactiveObject.Item1, _interactiveObject.Item2, 0, _last);
                    break;
            }

            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                Busy.Visibility = Visibility.Hidden;
                if (posts.Length > 0)
                {
                    _last = posts.Last().Id;
                    foreach (WPost p in posts)
                        if (_timelineType == TimelineType.PersonalTimeline)
                            TimelineStackPanel.Children.Add(new UIPost(p, false));
                        else
                            TimelineStackPanel.Children.Add(new UIPost(p, true));
                    LoadingText.Visibility = Visibility.Hidden;
                }
                else
                {
                    LoadingText.Visibility = Visibility.Visible;
                    LoadingText.Text = "There are no older posts";
                }
                _olderPostAllowed = true;
            }));
        }
    }
}
