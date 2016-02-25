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

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class Button : UserControl
    {
        public Button()
        {
            InitializeComponent();
            bd.DataContext = this;
        }

        public object DataObject
        {
            get { return (object)GetValue(DataObjectProperty); }
            set { SetValue(DataObjectProperty, value); }
        }

        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(Button), new PropertyMetadata(null));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Button), new PropertyMetadata(false));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Button), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Button), new PropertyMetadata(null));


        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(Button), new PropertyMetadata(null));

        


        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(Button), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));


        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly new DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Button), new PropertyMetadata(null));


        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly new DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Button), new PropertyMetadata(new Thickness(0d)));



        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static readonly new DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(Button), new PropertyMetadata(new Thickness(5d)));


        public double ImageTextPadding
        {
            get { return (double)GetValue(ImageTextPaddingProperty); }
            set { SetValue(ImageTextPaddingProperty, value); }
        }

        public static readonly DependencyProperty ImageTextPaddingProperty =
            DependencyProperty.Register("ImageTextPadding", typeof(double), typeof(Button), new PropertyMetadata(5d));



        public double TextBottomPadding
        {
            get { return (double)GetValue(TextBottomPaddingProperty); }
            set { SetValue(TextBottomPaddingProperty, value); }
        }

        public static readonly DependencyProperty TextBottomPaddingProperty =
            DependencyProperty.Register("TextBottomPadding", typeof(double), typeof(Button), new PropertyMetadata(0d));

        

        public ImageTextRelationSetting ImageTextRelation
        {
            get { return (ImageTextRelationSetting)GetValue(ImageTextRelationProperty); }
            set { SetValue(ImageTextRelationProperty, value); }
        }

        public static readonly DependencyProperty ImageTextRelationProperty =
            DependencyProperty.Register("ImageTextRelation", typeof(ImageTextRelationSetting), typeof(Button), new PropertyMetadata(ImageTextRelationSetting.ImageBeforeText));        



        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Button), new PropertyMetadata(new CornerRadius(2d)));


        public delegate void Clicked_Handler(Button bt);
        public event Clicked_Handler Clicked;

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }   



    }

    public enum ImageTextRelationSetting
    {
        ImageBeforeText,
        TextBeforeImage
    }

}
