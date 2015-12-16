using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Security;

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for PasswordBox.xaml
    /// </summary>
    public partial class PasswordBox : UserControl
    {
        public PasswordBox()
        {
            InitializeComponent();
            root_STACK.DataContext = this;
        }

        //public event EventHandler PasswordChanged;

        public event RoutedEventHandler PasswordChanged;

        public event KeyEventHandler PreviewKeyDown;

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PasswordBox), new PropertyMetadata(null));


        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(PasswordBox), new PropertyMetadata(null));


        public SecureString SecurePassword { get; set; }

        public string PasswordText { get; set; }


        public string Instructions
        {
            get { return (string)GetValue(InstructionsProperty); }
            set { SetValue(InstructionsProperty, value); }
        }

        public static readonly DependencyProperty InstructionsProperty =
            DependencyProperty.Register("Instructions", typeof(string), typeof(PasswordBox), new PropertyMetadata(null));



        public bool Required
        {
            get { return (bool)GetValue(RequiredProperty); }
            set { SetValue(RequiredProperty, value); }
        }

        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register("Required", typeof(bool), typeof(PasswordBox), new PropertyMetadata(false));

        

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (TextChanged != null) TextChanged(sender, e);
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

        private void pswd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SecurePassword = ((System.Windows.Controls.PasswordBox)sender).SecurePassword;

            PasswordText = ((System.Windows.Controls.PasswordBox)sender).Password;

            if (PasswordChanged != null) PasswordChanged(sender, e);
        }

        private void pswd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PreviewKeyDown != null) PreviewKeyDown(sender, e);
        }
    }
}
