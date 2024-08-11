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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoAppUI.Controls
{
   
    public class NavigationPanel : Control
    {
        public static readonly DependencyProperty IsOpenProperty =
     DependencyProperty.Register("IsOpen", typeof(bool), typeof(NavigationPanel),
         new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty OpenCloseDurationProperty =
            DependencyProperty.Register("OpenCloseDuration", typeof(Duration), typeof(NavigationPanel),
                new PropertyMetadata(Duration.Automatic));

        public Duration OpenCloseDuration
        {
            get { return (Duration)GetValue(OpenCloseDurationProperty); }
            set { SetValue(OpenCloseDurationProperty, value); }
        }

        public static readonly DependencyProperty FallbackOpenWidthProperty =
            DependencyProperty.Register("FallbackOpenWidth", typeof(double), typeof(NavigationPanel),
                new PropertyMetadata(100.0));

        public double FallbackOpenWidth
        {
            get { return (double)GetValue(FallbackOpenWidthProperty); }
            set { SetValue(FallbackOpenWidthProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(NavigationPanel),
                new PropertyMetadata(null));

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        static NavigationPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationPanel), new FrameworkPropertyMetadata(typeof(NavigationPanel)));
        }

        public NavigationPanel()
        {
            Width = 0;
            Loaded += NavigationPanel_Loaded;           
        }

        private void NavigationPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Width = GetDesiredContentWidth();
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigationPanel hamburgerMenu)
            {
                hamburgerMenu.OnIsOpenPropertyChanged();
            }
        }

        private void OnIsOpenPropertyChanged()
        {
            if (IsOpen)
            {
                OpenMenuAnimated();
            }
            else
            {
                CloseMenuAnimated();
            }
        }

        private void OpenMenuAnimated()
        {
            double contentWidth = GetDesiredContentWidth();

            DoubleAnimation openingAnimation = new DoubleAnimation(contentWidth, OpenCloseDuration);
            BeginAnimation(WidthProperty, openingAnimation);
        }

        private double GetDesiredContentWidth()
        {
            if (Content == null)
            {
                return FallbackOpenWidth;
            }

            Content.Measure(new Size(MaxWidth, MaxHeight));

            return Content.DesiredSize.Width;
        }

        private void CloseMenuAnimated()
        {
            DoubleAnimation closingAnimation = new DoubleAnimation(0, OpenCloseDuration);
            BeginAnimation(WidthProperty, closingAnimation);
        }
    }
}
