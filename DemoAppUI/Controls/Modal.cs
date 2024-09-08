using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DemoAppUI.Controls
{
    public class Modal : ContentControl
    {
        public static readonly DependencyProperty IsOpenProperty =  DependencyProperty.Register("IsOpen", typeof(bool), typeof(Modal), new PropertyMetadata(false));

        public Style BorderStyle
        {
            get { return (Style)GetValue(BorderStyleProperty); }
            set { SetValue(BorderStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderStyleProperty =
            DependencyProperty.Register("BorderStyle", typeof(Style), typeof(Modal), new PropertyMetadata(null));

        public Style OuterBorderStyle
        {
            get { return (Style)GetValue(OuterBorderStyleProperty); }
            set { SetValue(OuterBorderStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OuterBorderStyleProperty =
            DependencyProperty.Register("OuterBorderStyle", typeof(Style), typeof(Modal), new PropertyMetadata(null));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal)));
            BackgroundProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(CreateDefaultBackground()));

        }

        private static object CreateDefaultBackground()
        {
            return new SolidColorBrush(Colors.Black)
            {
                Opacity = 0.3
            };
        }
    }
}
