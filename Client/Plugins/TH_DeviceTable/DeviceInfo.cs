﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media;

using TH_Configuration;

namespace TH_DeviceTable
{
    public class DeviceInfo : INotifyPropertyChanged, IComparable
    {
        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                PropertyChanged.ChangeAndNotify<bool>(ref _connected, value, () => Connected);
            }
        }

        private bool _available;
        public bool Available
        {
            get { return _available; }
            set
            {
                _available = value;
                PropertyChanged.ChangeAndNotify<bool>(ref _available, value, () => Available);
            }
        }

        public Configuration Configuration { get; set; }

        public Description_Settings Description
        {
            get
            {
                if (Configuration != null) return Configuration.Description;
                return null;
            }
        }

        #region "Image"

        private ImageSource _manufacturerLogo;
        public ImageSource ManufacturerLogo
        {
            get { return _manufacturerLogo; }
            set
            {
                _manufacturerLogo = value;
                PropertyChanged.ChangeAndNotify<ImageSource>(ref _manufacturerLogo, value, () => ManufacturerLogo);
            }
        }

        private bool _manufacturerLogoLoading;
        public bool ManufacturerLogoLoading
        {
            get { return _manufacturerLogoLoading; }
            set
            {
                _manufacturerLogoLoading = value;
                PropertyChanged.ChangeAndNotify<bool>(ref _manufacturerLogoLoading, value, () => ManufacturerLogoLoading);
            }
        }

        #endregion

        #region "OEE"

        private double _oee;
        public double Oee
        {
            get { return _oee; }
            set
            {
                PropertyChanged.ChangeAndNotify<double>(ref _oee, value, () => Oee);
                if (_oee >= 0.75) OeeStatus = 2;
                else if (_oee >= 0.5) OeeStatus = 1;
                else OeeStatus = 0;
            }
        }

        private int _oeeStatus;
        public int OeeStatus
        {
            get { return _oeeStatus; }
            set { PropertyChanged.ChangeAndNotify<int>(ref _oeeStatus, value, () => OeeStatus); }
        }

        private double _availability;
        public double Availability
        {
            get { return _availability; }
            set
            {
                PropertyChanged.ChangeAndNotify<double>(ref _availability, value, () => Availability);
                if (_availability >= 0.75) AvailabilityStatus = 2;
                else if (_availability >= 0.5) AvailabilityStatus = 1;
                else AvailabilityStatus = 0;
            }
        }

        private int _availabilityStatus;
        public int AvailabilityStatus
        {
            get { return _availabilityStatus; }
            set { PropertyChanged.ChangeAndNotify<int>(ref _availabilityStatus, value, () => AvailabilityStatus); }
        }

        private double _performance;
        public double Performance
        {
            get { return _performance; }
            set
            {
                PropertyChanged.ChangeAndNotify<double>(ref _performance, value, () => Performance);
                if (_performance >= 0.75) PerformanceStatus = 2;
                else if (_performance >= 0.5) PerformanceStatus = 1;
                else PerformanceStatus = 0;
            }
        }

        private int _performanceStatus;
        public int PerformanceStatus
        {
            get { return _performanceStatus; }
            set { PropertyChanged.ChangeAndNotify<int>(ref _performanceStatus, value, () => PerformanceStatus); }
        }

        #endregion

        #region "Production Status"

        private string _productionStatus;
        public string ProductionStatus
        {
            get { return _productionStatus; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _productionStatus, value, () => ProductionStatus); }
        }

        private double _productionStatusTotal;
        public double ProductionStatusTotal
        {
            get { return _productionStatusTotal; }
            set { PropertyChanged.ChangeAndNotify<double>(ref _productionStatusTotal, value, () => ProductionStatusTotal); }
        }

        private double _productionStatusSeconds;
        public double ProductionStatusSeconds
        {
            get { return _productionStatusSeconds; }
            set { PropertyChanged.ChangeAndNotify<double>(ref _productionStatusSeconds, value, () => ProductionStatusSeconds); }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        #region "IComparable"

        private int Index
        {
            get
            {
                if (Configuration != null) return Configuration.Index;
                return 0;
            }
        }

        private string UniqueId
        {
            get
            {
                if (Configuration != null) return Configuration.UniqueId;
                return null;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceInfo;
            if (i != null)
            {
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        public override bool Equals(object obj)
        {

            var other = obj as DeviceInfo;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceInfo o1, DeviceInfo o2)
        {
            if (!object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return false;
            if (object.ReferenceEquals(o1, null) && !object.ReferenceEquals(o2, null)) return false;
            if (object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return true;

            return o1.UniqueId == o2.UniqueId && o1.Index == o2.Index;
        }

        static bool NotEqualTo(DeviceInfo o1, DeviceInfo o2)
        {
            if (!object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return true;
            if (object.ReferenceEquals(o1, null) && !object.ReferenceEquals(o2, null)) return true;
            if (object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return false;

            return o1.UniqueId != o2.UniqueId || o1.Index != o2.Index;
        }

        static bool LessThan(DeviceInfo o1, DeviceInfo o2)
        {
            if (o1.Index > o2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceInfo o1, DeviceInfo o2)
        {
            if (o1.Index < o2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceInfo o1, DeviceInfo o2)
        {
            return EqualTo(o1, o2);
        }

        public static bool operator !=(DeviceInfo o1, DeviceInfo o2)
        {
            return NotEqualTo(o1, o2);
        }


        public static bool operator <(DeviceInfo o1, DeviceInfo o2)
        {
            return LessThan(o1, o2);
        }

        public static bool operator >(DeviceInfo o1, DeviceInfo o2)
        {
            return GreaterThan(o1, o2);
        }


        public static bool operator <=(DeviceInfo o1, DeviceInfo o2)
        {
            return LessThan(o1, o2) || EqualTo(o1, o2);
        }

        public static bool operator >=(DeviceInfo o1, DeviceInfo o2)
        {
            return GreaterThan(o1, o2) || EqualTo(o1, o2);
        }

        #endregion

    }

    public static class Extensions
    {
        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler,
             ref T field, T value, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }

            field = value;
            return true;
        }
    }
}