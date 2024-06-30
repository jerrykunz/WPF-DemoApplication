using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DemoAppUI.Controls
{
    public class NavigationPanelItem : RadioButton
    {
        static NavigationPanelItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationPanelItem), new FrameworkPropertyMetadata(typeof(NavigationPanelItem)));
        }
    }
}
