// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TrakHound.Configurations;

namespace TrakHound_Overview.Controls.DeviceDisplay
{
    /// <summary>
    /// Interaction logic for Column_Overlay.xaml
    /// </summary>
    public partial class Overlay : UserControl, IComparable
    {
        public Overlay(DeviceConfiguration config)
        {
            InitializeComponent();
            DataContext = this;

            if (config != null) Index = config.Index;

            configuration = config;
        }

        public int Index { get; set; }

        DeviceConfiguration configuration;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Overlay), new PropertyMetadata(true));



        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Overlay), new PropertyMetadata(false));
      

        public string ConnectionStatus
        {
            get { return (string)GetValue(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(string), typeof(Overlay), new PropertyMetadata("Connnecting.."));


        public ImageSource ConnectionImage
        {
            get { return (ImageSource)GetValue(ConnectionImageProperty); }
            set { SetValue(ConnectionImageProperty, value); }
        }

        public static readonly DependencyProperty ConnectionImageProperty =
            DependencyProperty.Register("ConnectionImage", typeof(ImageSource), typeof(Overlay), new PropertyMetadata(null));


        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() == typeof(Overlay))
            {
                var i = obj as Overlay;
                if (i != null)
                {
                    if (i > this) return -1;
                    else if (i < this) return 1;
                    else return 0;
                }
                else return 1;
            }
            else return 1;
        }

        #region "Private"

        static bool EqualTo(Overlay c1, Overlay c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Index == c2.Index;
        }

        static bool NotEqualTo(Overlay c1, Overlay c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Index != c2.Index;
        }

        static bool LessThan(Overlay c1, Overlay c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(Overlay c1, Overlay c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(Overlay c1, Overlay c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(Overlay c1, Overlay c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(Overlay c1, Overlay c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(Overlay c1, Overlay c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(Overlay c1, Overlay c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(Overlay c1, Overlay c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

    }
}
