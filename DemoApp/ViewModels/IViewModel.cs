using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{
    public interface IViewModel
    {
        void OnEnterSoft();
        void OnExitSoft();
        void OnEnter();
        void OnExit();
        void OnLanguageChange();
        void Dispose();
    }
}
