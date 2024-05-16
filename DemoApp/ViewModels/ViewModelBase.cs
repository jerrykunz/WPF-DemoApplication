using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.ViewModels
{
    public class ViewModelBase : ObservableObject, IViewModel
    {
        public ViewModelBase()
        {
        }

        public virtual void OnEnterSoft()
        {

        }

        public virtual void OnExitSoft()
        {
        }


        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {

        }

        public virtual void OnLanguageChange()
        {

        }

        #region Dispose
        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {

        }
        #endregion
    }
}
