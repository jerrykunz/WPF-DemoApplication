using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DemoApp
{
    internal static class AsyncHelper
    {

        private delegate void RunSyncTDelegate(Func<Task> func);
        private delegate void RunSyncDelegate<TResult>(Func<Task<TResult>> func);

        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        //public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        //{
        //    var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

        //    //if UI thread, do task in new thread
        //    if (threadApartment != System.Threading.ApartmentState.STA)
        //    {
        //        Application.Current.Dispatcher.BeginInvoke(
        //                        System.Windows.Threading.DispatcherPriority.Normal,
        //                        new RunSyncDelegate<TResult>(RunSync),
        //                        func);
        //        return 
        //    }


        //    return AsyncHelper._myTaskFactory
        //      .StartNew<Task<TResult>>(func)
        //      .Unwrap<TResult>()
        //      .GetAwaiter()
        //      .GetResult();
        //}

        public static void RunSync(Func<Task> func)
        {
            var threadApartment = System.Threading.Thread.CurrentThread.GetApartmentState();

            //if UI thread, do task in new thread
            if (threadApartment != System.Threading.ApartmentState.STA)
            {
                Application.Current.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new RunSyncTDelegate(RunSync),
                                func);
                return;
            }

            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
