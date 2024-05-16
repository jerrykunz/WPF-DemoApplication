using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DemoApp.Model.Settings
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class LanguageCollectionSettings
    {
        [XmlArray("LanguageSettings")]
        [XmlArrayItem("LanguageSetting", typeof(LanguageSettings))]
        public List<LanguageSettings> Languages { get; set; }
        public LanguageCollectionSettings()
        {
            Languages = new List<LanguageSettings>();
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
