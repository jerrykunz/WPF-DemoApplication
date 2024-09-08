using DemoApp.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Model
{
    public class PopupModel
    {
        public int QueueNumber { get; set; }
        public PopupMode Mode { get; set; }
        public string Title { get;set; }
        public string Text { get; set; }
        public string Button1Text { get; set; }
        public string Button2Text { get; set; }

        public PopupModel()
        {
            Mode = PopupMode.OneButton;
            Title = "Title";
            Text = "Text";
            Button1Text = "Ok";
            Button2Text = "Cancel";
        }
    }
}
