using System.Windows.Controls;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels
{
    /// <summary>
    /// Manage the lifecycle of the panel
    /// </summary>
    public class UIPanel : UserControl
    {
        /// <summary>
        /// Open the panel
        /// </summary>
        virtual public void Open() { }

        /// <summary>
        /// Close the panel
        /// </summary>
        virtual public void Close() { }

        /// <summary>
        /// Update the panel
        /// </summary>
        virtual public void WakeUp() { }
    }
}
