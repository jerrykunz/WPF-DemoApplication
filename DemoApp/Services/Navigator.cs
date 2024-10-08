﻿using DemoApp.Model;
using DemoApp.Stores;
using DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DemoApp.Services
{
    //TODO: view navigation does not work well at all
    public class Navigator : INavigator
    {
        private INavigationStore _navigationStore;
        private delegate void PreviousVmDelegate(bool start, bool end, bool dispose);
        private delegate void ChangeVmDelegate(Type type, bool start, bool end, bool dispose);
        private delegate void ChangeVmDelegate<T>(bool start, bool end, bool dispose);
        private delegate void ChangeVmIndexDelegate(int index, bool start, bool end, bool dispose);
        private delegate void ChangeViewDelegate(string name, bool start, bool end, bool dispose);

        public Navigator(INavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }

        #region Non-disposable Views/VMS
        public void ChangeViewModel(Type viewModelType, bool onEnter, bool onExit)
        {
            ChangeViewModelInternal(viewModelType, onEnter, onExit, false);
        }

        public void ChangeViewModel<T>(bool onEnter, bool onExit) where T : class, IViewModel
        {
            ChangeViewModelInternal<T>(onEnter, onExit, false);
        }

        public void ChangeToSlot(int index, bool onEnter, bool onExit)
        {
            ChangeToSlotInternal(index, onEnter, onExit, false);
        }

        public void PreviousViewModel(bool onEnter, bool onExit)
        {
            PreviousViewModelInternal(onEnter, onExit, false);
        }

        public void ChangeView(Type viewType, bool onEnter, bool onExit)
        {
            ChangeViewInternal(viewType, onEnter, onExit, false);
        }

        public void ChangeView<T>(bool onEnter, bool onExit)
        {
            ChangeViewInternal<T>(onEnter, onExit, false);
        }

        public void ChangeView(string viewName, bool onEnter, bool onExit)
        {
            ChangeViewInternal(viewName, onEnter, onExit, false);
        }

        public void PreviousView(bool onEnter, bool onExit)
        {
            PreviousViewInternal(onEnter, onExit, false);
        }

        #endregion

        #region Disposable Views/VMS
        public void ChangeViewModelDispose(Type viewModelType, bool onEnter, bool onExit)
        {
            ChangeViewModelInternal(viewModelType, onEnter, onExit, true);
        }
    

        public void ChangeViewModelDispose<T>(bool onEnter, bool onExit) where T : class, IViewModel
        {
            ChangeViewModelInternal<IViewModel>(onEnter, onExit, true);
        }

        public void PreviousViewModelDispose(bool onEnter, bool onExit)
        {
            PreviousViewModelInternal(onEnter, onExit, true);
        }

        public void ChangeViewDispose(Type viewType, bool onEnter, bool onExit)
        {
            ChangeViewInternal(viewType, onEnter, onExit, true);
        }

        public void ChangeViewDispose<T>(bool onEnter, bool onExit)
        {
            ChangeViewInternal<T>(onEnter, onExit, true);
        }

        public void ChangeViewDispose(string viewName, bool onEnter, bool onExit)
        {
            ChangeViewInternal(viewName, onEnter, onExit, true);
        }

        public void PreviousViewDispose(bool onEnter, bool onExit)
        {
            PreviousViewInternal(onEnter, onExit, true);
        }

        #endregion

        #region Internal
        private void ChangeViewModelInternal(Type viewModelType, bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewModelInternal),
                                viewModelType, onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });


            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;


            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModel(viewModelType);

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = viewModelType;

            //get new view and set datacontext to new vm
            var newView = _navigationStore.GetViewByViewModelType(viewModelType);

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;
            _navigationStore.CurrentViewName = _navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(viewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }
        private void ChangeViewModelInternal<T>(bool onEnter, bool onExit, bool dispose) where T : class, IViewModel
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate<T>(this.ChangeViewModelInternal<T>),
                                onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });


            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;   
                }

                //vm exit script
                if (onExit)
                {
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }            

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModel(typeof(T));

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel; 
            _navigationStore.CurrentViewModelType = typeof(T);

           //get new view and set datacontext to new vm
           var newView = _navigationStore.GetViewByViewModelType(typeof(T));

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;
            _navigationStore.CurrentViewName = _navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(typeof(T));
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }        
        private void PreviousViewModelInternal(bool onEnter, bool onExit, bool dispose)
        {
            if (_navigationStore.PreviousViewModelType == null)
                return;

            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new PreviousVmDelegate(this.PreviousViewModelInternal),
                                onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });

            var newViewModelType = _navigationStore.PreviousViewModelType;

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModel(newViewModelType);

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = newViewModelType;

            //get new view and set datacontext to new vm
            var newView = _navigationStore.GetViewByViewModelType(newViewModelType);

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;
            _navigationStore.CurrentViewName = _navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(newViewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }

        //embedded by type
        private void ChangeViewInternal(Type viewType, bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewInternal),
                                viewType, onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });


            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;


            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModelByView(viewType);
            var newViewModelType = newViewModel.GetType();

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = newViewModelType;

            //get new view and set datacontext to new vm
            var newView = _navigationStore.GetView(viewType);

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            //Type currentViewType = null;
            //_navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = viewType; //currentViewType;


            string currentViewName = null;
            _navigationStore.ViewNamesByViewType.TryGetValue(viewType, out currentViewName);
            _navigationStore.CurrentViewName = currentViewName ?? viewType.Name; //TODO: get from viewnamesbytype, not vms //_navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(newViewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }
        private void ChangeViewInternal<T>(bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewInternal),
                                typeof(T), onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });


            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModelByView(typeof(T));
            var newViewModelType = newViewModel.GetType();

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = newViewModelType;

            //get new view and set datacontext to new vm
            var newView = _navigationStore.GetView(typeof(T));

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            //Type currentViewType = null;
            //_navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = typeof(T); //currentViewType;

            string currentViewName = null;
            _navigationStore.ViewNamesByViewType.TryGetValue(typeof(T), out currentViewName);
            _navigationStore.CurrentViewName = currentViewName ?? newView.Name; //TODO: get from viewnamesbytype, don't use vms //_navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(newViewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }
        //loose by name
        private void ChangeViewInternal(string viewName , bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewInternal),
                                viewName, onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });


            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModelByViewName(viewName);
            var newViewModelType = newViewModel.GetType();

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = newViewModelType;

            //get new view and set datacontext to new vm
            var newView = _navigationStore.GetViewByName(viewName);

            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set current views
            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;
            _navigationStore.CurrentViewName = viewName; //_navigationStore.ViewNamesByVM[newViewModel.GetType()];

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(newViewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }
        //both
        private void PreviousViewInternal(bool onEnter, bool onExit, bool dispose)
        {
            if (_navigationStore.PreviousViewModelType == null)
                return;

            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new PreviousVmDelegate(this.PreviousViewInternal),
                                onEnter, onExit, dispose);
                return;
            }

            ///Actual function

            //If the index is not the last in the list, remove the ones before it
            int previousVmTypesMaxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //Math.Max(_navigationStore.PreviousViewModelTypes.Count() - 1, 0); //can't be less than zero
            bool wasLatest = true;
            if (_navigationStore.NextIndex <= previousVmTypesMaxIndex)
            {
                wasLatest = false;
                for (int i = previousVmTypesMaxIndex; i >= _navigationStore.NextIndex; i--)
                {
                    _navigationStore.PreviousViewModelTypes.RemoveAt(i);
                    _navigationStore.ViewModelList.RemoveAt(i);
                    _navigationStore.ViewList.RemoveAt(i);
                    _navigationStore.ViewTypeList.RemoveAt(i);
                    _navigationStore.ViewNameList.RemoveAt(i);
                    _navigationStore.FunctionList.RemoveAt(i);
                }
            }

            _navigationStore.FunctionList.Add(new NavigationFunctionRecord { OnEnter = false, OnExit = false });

            var newViewType = _navigationStore.PreviousViewType;
            var newViewName = _navigationStore.PreviousViewName;

            var newViewModelType = _navigationStore.PreviousViewModelType;

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            _navigationStore.PreviousViewModelType = previousViewModelType;
            //if this is null, everything else in that slot is too
            if (previousViewModelType != null)
            {
                //set previous view/vm types
                _navigationStore.PreviousViewType = previousViewType;
                _navigationStore.PreviousViewName = previousViewName;

                //set previous views/vms (non-disposable only, otherwise the refs would stay and cause trouble)
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.PreviousView = previousView;
                }

                //vm exit script
                if (onExit)
                {
                    //_navigationStore.FunctionList[_navigationStore.NextIndex - 1].OnExit = true;
                    _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit = true;
                    previousViewModel.OnExit();
                }
                else
                {
                    previousViewModel.OnExitSoft();
                }

                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new vm
            var newViewModel = _navigationStore.GetViewModel(newViewModelType);

            //set current vms
            _navigationStore.CurrentViewModel = newViewModel;
            _navigationStore.CurrentViewModelType = newViewModelType;

            //get new view 
            UserControl newView = null;
            if (newViewType != null)
            {
                newView = _navigationStore.GetView(newViewType);
            }
            else if (!string.IsNullOrWhiteSpace(newViewName))
            {
                newView = _navigationStore.GetViewByName(newViewName);
            }


            //if disposing, these queues are ignored
            if (!dispose)
            {
                if (wasLatest)
                {
                    _navigationStore.AddtoViewModelList(newViewModel);
                    _navigationStore.AddToViewList(newView);
                }
            }

            //set datacontext
            newView.DataContext = _navigationStore.CurrentViewModel;

            //set current views
            _navigationStore.CurrentView = newView;
            _navigationStore.CurrentViewType = newViewType;
            _navigationStore.CurrentViewName = newViewName;

            if (wasLatest)
            {
                _navigationStore.AddToViewModelTypeList(newViewModelType);
                _navigationStore.AddToViewTypeList(_navigationStore?.CurrentViewType);
                _navigationStore.AddToViewNameList(_navigationStore?.CurrentViewName ?? _navigationStore?.CurrentViewType?.Name);
            }

            //we were not latest in the queue
            if (wasLatest)
            {
                _navigationStore.NextIndex = Math.Min(_navigationStore.NextIndex + 1, _navigationStore.Slots);
            }

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnEnter = true;
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }


        private void ChangeToSlotInternal(int index, bool onEnter, bool onExit, bool dispose)
        {
            var test = _navigationStore.PreviousViewModelTypes;

            //set within limits
            int maxIndex = _navigationStore.PreviousViewModelTypes.Count() - 1; //_navigationStore.Slots - 1;
            index = Math.Min(Math.Max(index, 0), maxIndex);

            //nothing to do
            if (index == _navigationStore.CurrentIndex)
                return;

            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmIndexDelegate(this.ChangeToSlotInternal),
                                index, onEnter, onExit, dispose);
                return;
            }

            //Actual function         

            _navigationStore.PreviousViewModelType = _navigationStore.CurrentViewModelType;

            //set previous vm
            _navigationStore.PreviousViewModel = _navigationStore.CurrentViewModel;

            //set previous view
            _navigationStore.PreviousView = _navigationStore.CurrentView;


            _navigationStore.PreviousViewType = _navigationStore.CurrentViewType;
            _navigationStore.PreviousViewName = _navigationStore.CurrentViewName;


            //vm exit script
            //if (onExit)
            //always do exit if we haven't yet determined whether to do it or not
            //not sure what is the best way actually
            if (_navigationStore.CurrentIndex == maxIndex ||
                _navigationStore.FunctionList[_navigationStore.CurrentIndex].OnExit)
            {
                _navigationStore.PreviousViewModel.OnExit();
            }
            else
            {
                _navigationStore.PreviousViewModel.OnExitSoft();
            }

            //dispose previous vm if applicable
            if (dispose)
            {
                _navigationStore.PreviousViewModel.Dispose();
                _navigationStore.PreviousViewModel = null;
                _navigationStore.PreviousView = null;
            }

            //create new vm
            //previous vm = new vm
            _navigationStore.NextIndex = index + 1;
            var newViewModelType = _navigationStore.PreviousViewModelTypes[index];

            var newViewModel = _navigationStore.GetViewModel(newViewModelType);
            _navigationStore.CurrentViewModel = newViewModel;


            //create new view and set datacontext to new vm
            var newView = _navigationStore.GetViewByViewModelType(newViewModelType);

            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            //set current view type and name
            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModelType, out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;

            _navigationStore.CurrentViewName = _navigationStore.ViewNamesByVM[newViewModelType];

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            //if (onEnter)
            if (_navigationStore.FunctionList[index].OnEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }

        #region Old
        private void PreviousViewModelInternalOld(bool onEnter, bool onExit, bool dispose)
        {
            if (_navigationStore.PreviousViewModelType == null)
                return;

            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new PreviousVmDelegate(this.PreviousViewModelInternal),
                                onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //previous vm = new vm
            var newViewModelType = _navigationStore.PreviousViewModelType;

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            if (!dispose)
            {
                _navigationStore.PreviousViewModel = previousViewModel;
                _navigationStore.AddtoViewModelList(previousViewModel);

                _navigationStore.PreviousView = previousView;
                _navigationStore.AddToViewList(previousView);
            }

            Type type = previousViewModel.GetType();
            _navigationStore.PreviousViewModelType = type;
            _navigationStore.AddToViewModelTypeList(type);

            _navigationStore.PreviousViewType = previousViewType;
            _navigationStore.AddToViewTypeList(previousViewType);
            _navigationStore.AddToViewNameList(previousViewName ?? previousViewType.Name);

            //vm exit script
            if (onExit)
            {
                previousViewModel.OnExit();
            }
            else
            {
                previousViewModel.OnExitSoft();
            }


            //dispose previous vm if applicable
            if (dispose)
            {
                previousViewModel.Dispose();
            }

            //create new vm
            //_navigationStore.CurrentViewModel = _navigationStore.GetViewModel(newViewModelType);
            var newViewModel = _navigationStore.GetViewModel(type);
            _navigationStore.CurrentViewModel = newViewModel;


            //create new view and set datacontext to new vm
            var newView = _navigationStore.GetViewByViewModelType(newViewModelType);

            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;

            //_navigationStore.CurrentViewType = _navigationStore.ViewsByVM[newViewModel.GetType()];
            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;

            _navigationStore.CurrentViewName = _navigationStore.ViewNamesByVM[newViewModel.GetType()];

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }
        private void ChangeViewInternalOld(Type viewType, bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewInternal),
                                viewType, onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //get new vm
            var newViewModel = _navigationStore.GetViewModelByView(viewType);

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;

            bool viewHasSameViewModel = previousViewModel == newViewModel;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;


            if (!dispose)
            {
                _navigationStore.PreviousView = previousView;
                _navigationStore.AddToViewList(previousView);
            }

            _navigationStore.PreviousViewType = previousViewType;
            _navigationStore.AddToViewTypeList(previousViewType);
            _navigationStore.AddToViewNameList(previousViewName ?? previousViewType.Name);

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.AddtoViewModelList(previousViewModel);

                }

                //old and new vm can be same
                Type type = previousViewModel.GetType();
                _navigationStore.PreviousViewModelType = type;
                _navigationStore.AddToViewModelTypeList(type);
            }

            //vm exit script
            //if navigator is used in onexits, there'll be trouble most likely
            if (onExit)
            {
                previousViewModel.OnExit();
            }
            else
            {
                previousViewModel.OnExitSoft();
            }

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new view and set datacontext to new vm
            UserControl newView = _navigationStore.GetViewByViewModelType(_navigationStore.ViewModelTypeByViewType[viewType]);

            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;
            _navigationStore.CurrentViewType = viewType;
            _navigationStore.CurrentViewName = viewType?.Name;

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script (only if vm change)
            //I think you could use navigator in these and not be in trouble

            if (onEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }

        }

        private void ChangeViewInternalOld<T>(bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeVmDelegate(this.ChangeViewInternal),
                                typeof(T), onEnter, onExit, dispose);
                return;
            }

            //Actual function


            //get new vm
            var newViewModel = _navigationStore.GetViewModelByView(typeof(T));

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;

            bool viewHasSameViewModel = previousViewModel == newViewModel;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;


            if (!dispose)
            {
                _navigationStore.PreviousView = previousView;
                _navigationStore.AddToViewList(previousView);
            }

            _navigationStore.PreviousViewType = previousViewType;
            _navigationStore.AddToViewTypeList(previousViewType);
            _navigationStore.AddToViewNameList(previousViewName ?? previousViewType.Name);

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.AddtoViewModelList(previousViewModel);
                }

                //old and new vm can be same
                Type type = previousViewModel.GetType();
                _navigationStore.PreviousViewModelType = type;
                _navigationStore.AddToViewModelTypeList(type);
            }

            //vm exit script
            //if navigator is used in onexits, there'll be trouble most likely
            if (onExit)
            {
                previousViewModel.OnExit();
            }
            else
            {
                previousViewModel.OnExitSoft();
            }

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new view and set datacontext to new vm
            UserControl newView = _navigationStore.GetViewByViewModelType(_navigationStore.ViewModelTypeByViewType[typeof(T)]);

            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;
            _navigationStore.CurrentViewType = typeof(T);

            _navigationStore.CurrentViewName = typeof(T).Name;

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script (only if vm change)
            //I think you could use navigator in these and not be in trouble          
            if (onEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }

        private void ChangeViewInternalOld(string viewName, bool onEnter, bool onExit, bool dispose)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ChangeViewDelegate(this.ChangeViewInternal),
                                viewName, onEnter, onExit, dispose);
                return;
            }

            //Actual function


            //get new vm
            var newViewModel = _navigationStore.GetViewModelByViewName(viewName);

            //set previous vm
            var previousViewModel = _navigationStore.CurrentViewModel;

            bool viewHasSameViewModel = previousViewModel == newViewModel;

            //set previous view & type
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            if (!dispose)
            {
                _navigationStore.PreviousView = previousView;
                _navigationStore.AddToViewList(previousView);
            }

            _navigationStore.PreviousViewType = previousViewType;
            _navigationStore.AddToViewTypeList(previousViewType);
            _navigationStore.AddToViewNameList(previousViewName ?? previousViewType.Name);

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                if (!dispose)
                {
                    _navigationStore.PreviousViewModel = previousViewModel;
                    _navigationStore.AddtoViewModelList(previousViewModel);
                }

                //old and new vm can be same
                Type type = previousViewModel.GetType();
                _navigationStore.PreviousViewModelType = type;
                _navigationStore.AddToViewModelTypeList(type);
            }

            //vm exit script
            //if navigator is used in onexits, there'll be trouble most likely
            if (onExit)
            {
                previousViewModel.OnExit();
            }
            else
            {
                previousViewModel.OnExitSoft();
            }

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }
            }

            //get new view and set datacontext to new vm
            //UserControl newView = _navigationStore.GetViewByVm(_navigationStore.ViewModelsByViewName[viewName]);
            UserControl newView = _navigationStore.GetViewByName(viewName);


            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;
            //_navigationStore.CurrentViewType = _navigationStore.ViewTypesByViewName[viewName];
            Type currentViewType = null;
            _navigationStore.ViewTypeByViewModelType.TryGetValue(newViewModel.GetType(), out currentViewType);
            _navigationStore.CurrentViewType = currentViewType;
            _navigationStore.CurrentViewName = viewName;

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script (only if vm change)
            //I think you could use navigator in these and not be in trouble
            if (onEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }

        private void PreviousViewInternalOld(bool onEnter, bool onExit, bool dispose)
        {
            if (_navigationStore.PreviousViewModelType == null)
                return;

            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new PreviousVmDelegate(this.PreviousViewInternal),
                                onEnter, onExit, dispose);
                return;
            }

            //Actual function

            //previous view = new view
            //are these available if dispose?
            var newView = _navigationStore.PreviousView;
            var newViewType = _navigationStore.PreviousViewType;
            var newViewModelType = _navigationStore.PreviousViewModelType;

            //set previous view & vm
            var previousView = _navigationStore.CurrentView;
            var previousViewType = _navigationStore.CurrentViewType;
            var previousViewName = _navigationStore.CurrentViewName;

            var previousViewModel = _navigationStore.CurrentViewModel;
            var previousViewModelType = _navigationStore.CurrentViewModel.GetType();

            bool viewHasSameViewModel = previousViewModelType == newViewModelType;

            if (!dispose)
            {
                _navigationStore.PreviousView = previousView;
                _navigationStore.AddToViewList(previousView);

                if (newViewModelType != previousViewModelType)
                {
                    _navigationStore.AddtoViewModelList(previousViewModel);
                }
            }

            _navigationStore.PreviousViewType = previousViewType;
            _navigationStore.AddToViewTypeList(previousViewType);
            _navigationStore.AddToViewNameList(previousViewName ?? previousViewType.Name);

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                Type type = previousView.GetType();
                _navigationStore.PreviousViewModelType = type;
                _navigationStore.AddToViewModelTypeList(type);
            }
            if (onExit)
            {
                previousViewModel.OnExit();
            }
            else
            {
                previousViewModel.OnExitSoft();
            }

            if (previousViewModel != null &&
                !viewHasSameViewModel)
            {
                //dispose previous vm if applicable
                if (dispose)
                {
                    previousViewModel.Dispose();
                }

                //create new vm
                _navigationStore.CurrentViewModel = _navigationStore.GetViewModel(newViewModelType);
            }

            //create new view and set datacontext to new vm
            //var newView = _navigationStore.GetViewByVm(newView);

            newView.DataContext = _navigationStore.CurrentViewModel;
            _navigationStore.CurrentView = newView;
            _navigationStore.CurrentViewType = newViewType;
            _navigationStore.CurrentViewName = newViewType?.Name ?? _navigationStore?.ViewNameList?.Last() ?? null;

            //loads new view to screen
            _navigationStore.RaiseChanged();

            //vm entry script
            if (onEnter)
            {
                _navigationStore.CurrentViewModel.OnEnter();
            }
            else
            {
                _navigationStore.CurrentViewModel.OnEnterSoft();
            }
        }

        #endregion



        #endregion
    }
}
