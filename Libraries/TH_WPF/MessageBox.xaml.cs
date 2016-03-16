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
using System.Windows.Shapes;

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static bool? Show(string text) { return CreateWindow(text); }

        public static bool? Show(string text, string windowTitle) { return CreateWindow(text, windowTitle); }

        public static bool? Show(string text, string windowTitle, MessageBoxButtons buttons) { return CreateWindow(text, windowTitle, buttons); }

        public static bool? Show(string text, string windowTitle, MessageBoxButtons buttons, ImageSource image) { return CreateWindow(text, windowTitle, buttons, image); }

        static bool? CreateWindow(string text, string windowTitle = null, MessageBoxButtons buttons = MessageBoxButtons.Ok, ImageSource image = null)
        {
            var msg = new MessageBox();
            msg.Text = text;
            msg.WindowTitle = windowTitle;
            msg.Buttons = buttons;
            msg.Image = image;

            return msg.ShowDialog();
        }


        public string WindowTitle
        {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string), typeof(MessageBox), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MessageBox), new PropertyMetadata(null));


        public MessageBoxButtons Buttons
        {
            get { return (MessageBoxButtons)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(MessageBoxButtons), typeof(MessageBox), new PropertyMetadata(MessageBoxButtons.Ok));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(MessageBox), new PropertyMetadata(null));

        private void Ok_Clicked(TH_WPF.Button bt)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Yes_Clicked(TH_WPF.Button bt)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void No_Clicked(TH_WPF.Button bt)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Cancel_Clicked(TH_WPF.Button bt)
        {
            this.DialogResult = null;
            this.Close();
        }

    }

    public enum MessageBoxButtons
    {
        Ok,
        YesNo,
        YesNoCancel,
        OkCancel
    }

}
