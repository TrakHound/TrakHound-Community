using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using TH_Configuration;

namespace TH_DeviceTable
{
    public class DeviceInfo : INotifyPropertyChanged
    {
        public bool Connected { get; set; }

        public Configuration Configuration { get; set; }

        public Description_Settings Description
        {
            get
            {
                if (Configuration != null) return Configuration.Description;
                return null;
            }
        }

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

        private string _productionStatus;
        public string ProductionStatus
        {
            get { return _productionStatus; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _productionStatus, value, () => ProductionStatus); }
        }

        #region "CNC Controller Status Variables"

        private string _emergencyStop;
        public string EmergencyStop
        {
            get { return _emergencyStop; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _emergencyStop, value, () => EmergencyStop); }
        }

        private string _controllerMode;
        public string ControllerMode
        {
            get { return _controllerMode; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _controllerMode, value, () => ControllerMode); }
        }

        private string _executionMode;
        public string ExecutionMode
        {
            get { return _executionMode; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _executionMode, value, () => ExecutionMode); }
        }

        private string _alarm;
        public string Alarm
        {
            get { return _alarm; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _alarm, value, () => Alarm); }
        }

        private string _partCount;
        public string PartCount
        {
            get { return _partCount; }
            set { PropertyChanged.ChangeAndNotify<string>(ref _partCount, value, () => PartCount); }
        }

        #endregion





        public event PropertyChangedEventHandler PropertyChanged;

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
