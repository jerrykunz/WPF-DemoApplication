using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Settings
{
    [SettingsManageability(SettingsManageability.Roaming)]
    public sealed partial class Preferences : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<LanguageCollectionSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <LanguageSettings>
    <LanguageSetting>
      <Code>en-GB</Code>
      <Name>In English</Name>
      <DateFormat>dd-MM-yyyy</DateFormat>
      <TimestampFormat>dd-MM-yyyy hh:mm:ss</TimestampFormat>
    </LanguageSetting>
    <LanguageSetting>
      <Code>fi-FI</Code>
      <Name>Suomeksi</Name>
      <DateFormat>dd-MM-yyyy</DateFormat>
      <TimestampFormat>dd-MM-yyyy hh:mm:ss</TimestampFormat>
    </LanguageSetting>
    <LanguageSetting>
      <Code>sv-SE</Code>
      <Name>På Svenska</Name>
      <DateFormat>dd-MM-yyyy</DateFormat>
      <TimestampFormat>dd-MM-yyyy hh:mm:ss</TimestampFormat>
    </LanguageSetting>
  </LanguageSettings>
</LanguageCollectionSettings>")]
        public global::DemoApp.Model.Settings.LanguageCollectionSettings LanguageSettings
        {
            get
            {
                return ((global::DemoApp.Model.Settings.LanguageCollectionSettings)(this["LanguageSettings"]));
            }
            set
            {
                this["LanguageSettings"] = value;
            }
        }



    }
}
