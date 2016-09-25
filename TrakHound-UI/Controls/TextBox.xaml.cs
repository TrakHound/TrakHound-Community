using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TrakHound_UI
{
    /// <summary>
    /// Interaction logic for TextBox.xaml
    /// </summary>
    public partial class TextBox : UserControl
    {
        public TextBox()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public event TextChangedEventHandler TextChanged;

        public event KeyEventHandler EnterPressed;

        #region "Public Properties"

        public object DataObject { get; set; }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TextBox), new PropertyMetadata(null));


        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(TextBox), new PropertyMetadata(null));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBox), new PropertyMetadata(null));


        public string Instructions
        {
            get { return (string)GetValue(InstructionsProperty); }
            set { SetValue(InstructionsProperty, value); }
        }

        public static readonly DependencyProperty InstructionsProperty =
            DependencyProperty.Register("Instructions", typeof(string), typeof(TextBox), new PropertyMetadata(null));


        public string Example
        {
            get { return (string)GetValue(ExampleProperty); }
            set { SetValue(ExampleProperty, value); }
        }

        public static readonly DependencyProperty ExampleProperty =
            DependencyProperty.Register("Example", typeof(string), typeof(TextBox), new PropertyMetadata(null));


        public bool ShowClear
        {
            get { return (bool)GetValue(ShowClearProperty); }
            set { SetValue(ShowClearProperty, value); }
        }

        public static readonly DependencyProperty ShowClearProperty =
            DependencyProperty.Register("ShowClear", typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        public bool Required
        {
            get { return (bool)GetValue(RequiredProperty); }
            set { SetValue(RequiredProperty, value); }
        }

        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register("Required", typeof(bool), typeof(TextBox), new PropertyMetadata(false));


        public bool TextWrapping
        {
            get { return (bool)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(bool), typeof(TextBox), new PropertyMetadata(false));


        [TypeConverterAttribute(typeof(FontSizeConverter))]
        [LocalizabilityAttribute(LocalizationCategory.Font)]
        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(TextBox), new PropertyMetadata(12d));


        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public new static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(TextBox), new PropertyMetadata(new SolidColorBrush(Colors.White)));


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(TextBox), new PropertyMetadata(new CornerRadius(1)));


        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register("IsFocused", typeof(bool), typeof(TextBox), new PropertyMetadata(false));

        #endregion

        #region "Public Methods"

        public void Clear()
        {
            ClearText();
        }

        #endregion

        #region "Private Methods"

        void ClearText()
        {
            txt.Text = "";
            txt.Clear();
            FocusText();
        }

        void FocusText()
        {
            txt.Focus();
            Keyboard.Focus(txt);
            txt.Select(0, 0);
        }

        #endregion

        #region "Private Event Handlers"

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChanged != null) TextChanged(sender, e);
        }

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        //private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    //txt.Focus();
        //    //Keyboard.Focus(txt);
        //    txt.Select(0, 0);
        //}

        //private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    //txt.Focus();
        //    //Keyboard.Focus(txt);
        //    txt.Select(0, 0);
        //}

        private void txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (EnterPressed != null) EnterPressed(sender, e);
        }


        private bool keyboardFocused = false;
        private bool mouseFocused = false;

        private void txt_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (!mouseFocused) txt.SelectAll();
            mouseFocused = true;

            e.Handled = true;
        }

        private void txt_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!keyboardFocused) txt.SelectAll();
            keyboardFocused = true;

            e.Handled = true;
        }

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            keyboardFocused = false;
            mouseFocused = false;
        }


        private void Clear_Clicked(Button bt)
        {
            ClearText();
        }

        #endregion

        private void txt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
