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

namespace DemoAppUI.Components
{
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl
    {
        //public double CanvasWidth => 60 * SpinnerSize;
        //public double CanvasHeight => 60 * SpinnerSize;
        public double EllipseWidth => 15 * SpinnerSize;
        public double EllipseHeight => 15 * SpinnerSize;

        public double Ellipse0Top => 21 * SpinnerSize;
        public double Ellipse1Top => 7 * SpinnerSize;
        public double Ellipse2Top => 0.75 * SpinnerSize;
        public double Ellipse3Top => 6.75 * SpinnerSize;
        public double Ellipse4Top => 20.75 * SpinnerSize;
        public double Ellipse5Top => 34.5 * SpinnerSize;
        public double Ellipse6Top => 39.75 * SpinnerSize;
        public double Ellipse7Top => 34.25 * SpinnerSize;

        public double Ellipse0Left => 1.75 * SpinnerSize;
        public double Ellipse1Left => 6.5 * SpinnerSize;
        public double Ellipse2Left => 20.5 * SpinnerSize;
        public double Ellipse3Left => 34.75 * SpinnerSize;
        public double Ellipse4Left => 40.5 * SpinnerSize;
        public double Ellipse5Left => 34.75 * SpinnerSize;
        public double Ellipse6Left => 20.75 * SpinnerSize;
        public double Ellipse7Left => 34.25 * SpinnerSize;
        public double EllipseHiddenLeft => 8.75 * SpinnerSize;
        public double EllipseHiddenTop => 8 * SpinnerSize;
        public double EllipseHiddenWidth => 39.5 * SpinnerSize;
        public double EllipseHiddenHeight => 39.5 * SpinnerSize;

        public static readonly DependencyProperty SpinnerSizeProperty =
        DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(Spinner), new PropertyMetadata(100.0));

        public static readonly DependencyProperty CanvasWidthProperty =
        DependencyProperty.Register(nameof(CanvasWidth), typeof(double), typeof(Spinner), new PropertyMetadata(100.0));

        public static readonly DependencyProperty CanvasHeightProperty =
        DependencyProperty.Register(nameof(CanvasHeight), typeof(double), typeof(Spinner), new PropertyMetadata(100.0));

        public double SpinnerSize
        {
            get { return (double)GetValue(SpinnerSizeProperty); }
            set { SetValue(SpinnerSizeProperty, value); }
        }

        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }


        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }


        public Spinner()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
