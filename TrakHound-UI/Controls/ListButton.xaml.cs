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

namespace TrakHound_UI
{
    /// <summary>
    /// Interaction logic for ListButton.xaml
    /// </summary>
    public partial class ListButton : UserControl, IComparable
    {
        public ListButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        //public object DataObject;

        public object DataObject
        {
            get { return (object)GetValue(DataObjectProperty); }
            set { SetValue(DataObjectProperty, value); }
        }

        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(ListButton), new PropertyMetadata(null));



        public bool AlternateStyle
        {
            get { return (bool)GetValue(AlternateStyleProperty); }
            set { SetValue(AlternateStyleProperty, value); }
        }

        public static readonly DependencyProperty AlternateStyleProperty =
            DependencyProperty.Register("AlternateStyle", typeof(bool), typeof(ListButton), new PropertyMetadata(false));

        

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ListButton), new PropertyMetadata(null));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ListButton), new PropertyMetadata(null));


        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(ListButton), new PropertyMetadata(null));


        public bool ShowImage
        {
            get { return (bool)GetValue(ShowImageProperty); }
            set { SetValue(ShowImageProperty, value); }
        }

        public static readonly DependencyProperty ShowImageProperty =
            DependencyProperty.Register("ShowImage", typeof(bool), typeof(ListButton), new PropertyMetadata(true));

        public bool ShowText
        {
            get { return (bool)GetValue(ShowTextProperty); }
            set { SetValue(ShowTextProperty, value); }
        }

        public static readonly DependencyProperty ShowTextProperty =
            DependencyProperty.Register("ShowText", typeof(bool), typeof(ListButton), new PropertyMetadata(true));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(ListButton), new PropertyMetadata(TextWrapping.NoWrap));


        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(ListButton), new PropertyMetadata(TextAlignment.Left));



        public Brush HoverBrush
        {
            get { return (Brush)GetValue(HoverBrushProperty); }
            set { SetValue(HoverBrushProperty, value); }
        }

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(ListButton), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(51, 0, 0, 0))));

        



        
        public delegate void Selected_Handler(ListButton LB);
        public event Selected_Handler Selected;
        public event Selected_Handler MultiSelected;
        public event Selected_Handler MultiUnselected;

        private void Main_GRID_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (MultiSelected != null) MultiSelected(this);

                Selected_Handler handler = Selected;
                if (handler != null) Selected(this);
            }
        }

        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected", typeof(bool), typeof(ListButton), new PropertyMetadata(false));


        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(ListButton), new PropertyMetadata(false));


        private void Main_GRID_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsSelected)
                {
                    if (MultiSelected != null) MultiSelected(this);
                }
                else
                {
                    if (MultiUnselected != null) MultiUnselected(this);
                }
            }
                
            MouseOver = true;
        }

        private void Main_GRID_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }


        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as ListButton;
            if (i != null)
            {
                return Text.CompareTo(i.Text);
            }
            else return 1;
        }
        

    }
}
