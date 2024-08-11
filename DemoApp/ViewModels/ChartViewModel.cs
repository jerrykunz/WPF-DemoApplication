using DemoApp.Services;
using DemoApp.Stores;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace DemoApp.ViewModels
{
    public class ChartViewModel : ActivityViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        INavigator _navigator;

        private DateTime _currentDate;
        public string CurrentDate
        {
            get
            {
                return _currentDate.ToShortDateString();
            }
        }


        private double _minY;
        private double _maxY;

        public double MinY
        {
            get => _minY;
            set
            {
                _minY = value;
                OnPropertyChanged(nameof(MinY));
            }
        }

        public double MaxY
        {
            get => _maxY;
            set
            {
                _maxY = value;
                OnPropertyChanged(nameof(MaxY));
            }
        }


        private double _lastLecture;
        private double _trend;
        public SeriesCollection LastHourSeries { get; set; }

        private SeriesCollection _column10Series;
        public SeriesCollection Column10Series
        {
            get { return _column10Series; }
            set
            {
                _column10Series = value;
                OnPropertyChanged(nameof(Column10Series));
            }
        }

        private DispatcherTimer _chartTimer;


        private bool _test;

        public string TestText { get { return "TEST_TEXT1"; } }


        public ChartValues<DateTimePoint> Values { get; set; }
        public Func<double, string> XFormatter { get; set; }

        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>



        private DelegateCommand<object> _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                if (_clickCommand == null)
                {
                    _clickCommand = new DelegateCommand<object>(UpdateAllOnClick);
                }
                return _clickCommand;
            }
        }

        #endregion

        public ChartViewModel(INavigator navigator,
                              IActivityStore activityStore) :
            base(activityStore)
        {
            _navigator = navigator;

            _currentDate = DateTime.Now;

            _chartTimer = new DispatcherTimer();
            _chartTimer.Tick += _chartTimer_Tick;
            _chartTimer.Interval = TimeSpan.FromSeconds(2);
            _chartTimer.Start();

            MinY = 0;
            MaxY = 100;
                

            LastHourSeries = new SeriesCollection
            {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8),
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8)
                    }
                }
            };
            _trend = 8;

            Task.Run(() =>
            {
                var r = new Random();
                while (true)
                {
                    Thread.Sleep(500);
                    _trend += (r.NextDouble() > 0.3 ? 1 : -1) * r.Next(0, 5);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LastHourSeries[0].Values.Add(new ObservableValue(_trend));
                        LastHourSeries[0].Values.RemoveAt(0);
                        SetLecture();
                        UpdateAxisRanges();
                    });
                }
            });





            var now = DateTime.MinValue.AddHours(10);
            Values = new ChartValues<DateTimePoint>
            {
                new DateTimePoint(now.AddHours(-9), 5),
                new DateTimePoint(now.AddHours(-8), 9),
                new DateTimePoint(now.AddHours(-7), 8),
                new DateTimePoint(now.AddHours(-6), 6),
                new DateTimePoint(now.AddHours(-5), 1),
                new DateTimePoint(now.AddHours(-4), 5),
                new DateTimePoint(now.AddHours(-3), 7),
                new DateTimePoint(now.AddHours(-2), 3),
                new DateTimePoint(now.AddHours(-1), 6),
                new DateTimePoint(now, 3)
            };

            Column10Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(1),
                        new ObservableValue(2),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(8),
                        new ObservableValue(9),
                        new ObservableValue(10)
                    }
                }
            };

            XFormatter = value => DateTime.Now.AddHours(value - Column10Series[0].Values.Count + 1 ).ToString("HH");

        }

        private void _chartTimer_Tick(object sender, EventArgs e)
        {
            //SeriesCollection[2].Values.RemoveAt(0);
            //SeriesCollection[2].Values.Add(5d);
        }


        public double LastLecture
        {
            get { return _lastLecture; }
            set
            {
                _lastLecture = value;
                OnPropertyChanged("LastLecture");
            }
        }

        private void UpdateAxisRanges()
        {
            var allValues = LastHourSeries
               .SelectMany(series => series.Values.Cast<ObservableValue>())
               .Select(value => value.Value);

            //MinX = 0;
            //MaxX = LastHourSeries[0].Values.Count - 1; // Assuming equal count for all series

            var prevMinY = MinY;
            var prevMaxY = MaxY
                ;
            //MinY = allValues.Min() - 1;
            MinY = 0;
            MaxY = allValues.Max() + 1;

            if (MinY < prevMinY)
                ;

            if (MaxY > prevMaxY)
                ;

        }

        private void SetLecture()
        {
            var target = ((ChartValues<ObservableValue>)LastHourSeries[0].Values).Last().Value;
            var step = (target - _lastLecture) / 4;

            Task.Run(() =>
            {
                for (var i = 0; i < 4; i++)
                {
                    Thread.Sleep(100);
                    LastLecture += step;
                }
                LastLecture = target;
            });
        }

        private void UpdateAllOnClick(object sender)
        {
            LastHourSeries[0].Values.Add(new ObservableValue(new Random().Next(0, 10)));
            while (LastHourSeries[0].Values.Count > 10)
                LastHourSeries[0].Values.RemoveAt(0);
        }

        public override void OnEnterSoft()
        {

        }

        public override void OnExitSoft()
        {

        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

    }
}
