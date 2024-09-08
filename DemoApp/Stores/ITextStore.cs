using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Stores
{
    public interface ITextStore
    {
        string GetString(string name);
        void PopulateDictionaries(string inFiveCharLang);
    }
}
