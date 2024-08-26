using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Stores
{
    public class TextStore : ITextStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, string> _fileByName;

        public TextStore()
        {
            _fileByName = new Dictionary<string, string>();
        }

        public string GetString(string name)
        {
            if (_fileByName.ContainsKey(name))
                return _fileByName[name];

            return string.Empty;
        }

        public void PopulateDictionaries(string inFiveCharLang)
        {
            PopulateTextDictionary(inFiveCharLang);
        }

        public void PopulateTextDictionary(string inFiveCharLang)
        {
            try
            {
                _fileByName.Clear();
                string[] filePaths = Directory.GetFiles(Path.Combine(App.LanguagesFolderPath, inFiveCharLang, "Text"), "*.*");

                foreach (var filePath in filePaths)
                {
                    string id = Path.GetFileName(filePath);
                    string text = File.ReadAllText(filePath);
                    _fileByName.Add(id, text);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
