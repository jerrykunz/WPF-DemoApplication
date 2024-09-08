using DemoApp.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DemoApp.Converters
{
    public class PopupModeToVisibilityConverter : IValueConverter
    {
        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }
        public PopupMode Mode { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PopupMode))
                return FalseVisibility;

            if ((PopupMode)value == PopupMode.Inactive)
            {
                return FalseVisibility;
            }

            if ((PopupMode)value == Mode)
            {
                return TrueVisibility;
            }

            return FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class PopupModeActiveToVisibilityConverter : IValueConverter
    //{
    //    public Visibility TrueVisibility { get; set; }
    //    public Visibility FalseVisibility { get; set; }
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (!(value is PopupMode))
    //            return FalseVisibility;

    //        if ((PopupMode)value == PopupMode.Inactive)
    //        {
    //            return FalseVisibility;
    //        }

    //        return TrueVisibility;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class PopupModeActiveToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PopupMode))
                return false;

            if ((PopupMode)value == PopupMode.Inactive)
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
