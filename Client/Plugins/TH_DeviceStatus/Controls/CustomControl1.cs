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

namespace TH_StatusTable.Controls
{
    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }



        public int Status
        {
            get { return (int)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Status.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(int), typeof(CustomControl1), new PropertyMetadata(-1));


    }

    public class BetterCell : Control
    {
        static BetterCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BetterCell), new FrameworkPropertyMetadata(typeof(BetterCell)));
        }

        public HourData HourData
        {
            get { return (HourData)GetValue(HourDataProperty); }
            set { SetValue(HourDataProperty, value); }
        }

        public static readonly DependencyProperty HourDataProperty =
            DependencyProperty.Register("HourData", typeof(HourData), typeof(BetterCell), new PropertyMetadata(null));

    }

    public class DrawingElement : FrameworkElement
    {
        static readonly TranslateTransform tt_cache = new TranslateTransform();

        public DrawingElement(Drawing drawing)
        {
            this.drawing = drawing;
        }
        readonly Drawing drawing;

        TranslateTransform get_transform()
        {
            if (Margin.Left == 0 && Margin.Top == 0)
                return null;
            tt_cache.X = Margin.Left;
            tt_cache.Y = Margin.Top;
            return tt_cache;
        }
        protected override Size MeasureOverride(Size _)
        {
            var sz = drawing.Bounds.Size;
            return new Size
            {
                Width = sz.Width + Margin.Left + Margin.Right,
                Height = sz.Height + Margin.Top + Margin.Bottom,
            };
        }
        protected override void OnRender(DrawingContext dc)
        {
            var tt = get_transform();
            if (tt != null)
                dc.PushTransform(tt);
            dc.DrawDrawing(drawing);
            if (tt != null)
                dc.Pop();
        }
    };
}
