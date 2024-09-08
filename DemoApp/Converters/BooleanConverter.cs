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
    public class VisibilityBooleanConverter : IValueConverter
    {
        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }
        public int TrueVisibilitySpan { get; set; }
        public int FalseVisibilitySpan { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
