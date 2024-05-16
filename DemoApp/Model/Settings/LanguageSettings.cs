using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Model.Settings
{

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class LanguageSettings : ObservableObject
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string DateFormat { get; set; }
        public string TimestampFormat { get; set; }

        public LanguageSettings()
        {
            Code = "xx-XX";
            Name = "Sample Text";
            DateFormat = "dd-MM-yyyy";
            TimestampFormat = "dd-MM-yyyy hh:mm:ss";
        }

        public LanguageSettings(string code, string text, string dateformat, string timeStampFormat)
        {
            Code = code;
            Name = text;
            DateFormat = dateformat;
            TimestampFormat = timeStampFormat;
        }

        public override string ToString()
        {
            return Code + " / " + Name;
        }
    }
}
