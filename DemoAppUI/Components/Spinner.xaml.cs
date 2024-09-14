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
        public double CanvasWidth => 60 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double CanvasHeight => 60 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseWidth => 15 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseHeight => 15 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));

        public double Ellipse0Top => 21 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse1Top => 7 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse2Top => 0.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse3Top => 6.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse4Top => 20.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse5Top => 34.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse6Top => 39.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse7Top => 34.25 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));

        public double Ellipse0Left => 1.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse1Left => 6.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse2Left => 20.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse3Left => 34.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse4Left => 40.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse5Left => 34.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse6Left => 20.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double Ellipse7Left => 34.25 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseHiddenLeft => 8.75 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseHiddenTop => 8 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseHiddenWidth => 39.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));
        public double EllipseHiddenHeight => 39.5 * ((double)Application.Current.FindResource("SpinnerSizeMultiplier"));


        public Spinner()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
