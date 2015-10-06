using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Objects
{
    public delegate void ClickDelegate(object sender, RoutedEventArgs e);

    /// <summary>
    /// Interaction logic for UICustomButton.xaml.
    /// </summary>
    public partial class UICustomButton : UserControl
    {
        /// <summary>
        /// Type of button.
        /// </summary>
        public enum ButtonType
        {
            Button,
            ToggleButton,
            JustImage
        }

        private bool _isChecked = false;

        /// <summary>
        /// Choosen type of button.
        /// </summary>
        public ButtonType Type { get; set; }

        /// <summary>
        /// True if the button action can be activated by Enter button, false otherwise.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The text shown inside the button.
        /// </summary>
        public string Text
        {
            get
            {
                return InternalText.Text;
            }
            set
            {
                InternalText.Text = value;
            }
        }

        /// <summary>
        /// True if the button have a border, false otherwise.
        /// </summary>
        public bool StaticBorder
        {
            get
            {
                return (Border.Visibility == Visibility.Visible);
            }
            set
            {
                if (value)
                    Border.Visibility = Visibility.Visible;
                else
                    Border.Visibility = Visibility.Hidden;
            }

        }

        /// <summary>
        /// True if the button is checked, false otherwise. It's valid just with ToggleButton type.
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                if (value == true)
                    MouseOverColor.Visibility = Visibility.Visible;
                else
                    MouseOverColor.Visibility = Visibility.Hidden;
            }

        }

        /// <summary>
        /// Click event of the button.
        /// </summary>
        public event ClickDelegate Click;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UICustomButton()
        {
            InitializeComponent();
            Type = ButtonType.Button;
        }

        private void MouseOverColor_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Type == ButtonType.JustImage) return;

            if (!_isChecked)
                MouseOverColor.Visibility = Visibility.Visible;
        }

        private void MouseOverColor_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Type == ButtonType.JustImage) return;

            if (!_isChecked)
                MouseOverColor.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Type == ButtonType.JustImage) return;

            if (Type == ButtonType.ToggleButton)
                _isChecked = true;

            if (Click != null)
                Click(this, null);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InternalText.Text))
            {
                InternalImage.Visibility = Visibility.Hidden;
                InternalText.Visibility = Visibility.Visible;
            }

            if (IsDefault)
            {
                (Parent as UIElement).KeyUp += Parent_KeyUp;
            }

        }

        private void Parent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Click != null)
                    Click(this, null);
            }
        }
    }
}
