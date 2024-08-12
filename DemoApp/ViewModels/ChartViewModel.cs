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
        IDatabaseService _databaseService;

        private DateTime _currentDate;
        public string CurrentDate
        {
            get
            {
                return _currentDate.ToShortDateString();
            }
        }


        private double _liveChartMinY;      
        public double LiveChartMinY
        {
            get => _liveChartMinY;
            set
            {
                _liveChartMinY = value;
                OnPropertyChanged(nameof(LiveChartMinY));
            }
        }

        private double _liveChartMaxY;
        public double LiveChartMaxY
        {
            get => _liveChartMaxY;
            set
            {
                _liveChartMaxY = value;
                OnPropertyChanged(nameof(LiveChartMaxY));
            }
        }


        private double _lastLecture;
        private double _liveChartTrend;
        public SeriesCollection LiveSeries { get; set; }

        private int _liveEnergyIndex;
        private double[] _liveEnergyBuffer;

        private int _prevMinute;



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
                              IDatabaseService databaseService,
                              IActivityStore activityStore) :
            base(activityStore)
        {
            _navigator = navigator;
            _databaseService = databaseService;

            _currentDate = DateTime.Now;



            InitEnergyLiveChart();


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

        public void InitEnergyLiveChart()
        {
            DateTime now = DateTime.Now;
            _prevMinute = now.Minute;


            _liveEnergyIndex = 0;
            _liveEnergyBuffer = new double[120];


            LiveChartMinY = 0;
            LiveChartMaxY = 100;

            _liveChartTrend = 8;

            LiveSeries = new SeriesCollection
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
                        new ObservableValue(_liveChartTrend)
                    }
                }
            };           

            Task.Run(() => EnergyLiveChart_Thread());
        }

        public void EnergyLiveChart_Thread()
        {
            var r = new Random();
            while (true)
            {
                Thread.Sleep(500);
                _liveChartTrend += (r.NextDouble() > 0.45 ? 1 : -1) * r.Next(0, 5);

                if (_liveChartTrend < 0)
                    _liveChartTrend = 0;

                int currentMinute = DateTime.Now.Minute;
                if (currentMinute != _prevMinute)
                {                    
                    double minuteAvg = 0;
                    for (int i = 0; i < _liveEnergyIndex; i++)
                    {
                        minuteAvg += _liveEnergyBuffer[i];
                    }
                    minuteAvg /= _liveEnergyIndex;

                    _databaseService.AddOrUpdateEnergyMinAvg(DateTime.Now, minuteAvg);

                    _liveEnergyIndex = 0;
                }
                _prevMinute = currentMinute;

                _liveEnergyBuffer[_liveEnergyIndex++] = _liveChartTrend;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LiveSeries[0].Values.Add(new ObservableValue(_liveChartTrend));
                    LiveSeries[0].Values.RemoveAt(0);
                    SetLiveEnergyLecture();
                    UpdateLiveEnergyAxisRanges();
                });
            }
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

        private void UpdateLiveEnergyAxisRanges()
        {
            var allValues = LiveSeries
               .SelectMany(series => series.Values.Cast<ObservableValue>())
               .Select(value => value.Value);

            LiveChartMinY = 0;
            LiveChartMaxY = allValues.Max() + 1;


            ////MinX = 0;
            ////MaxX = LastHourSeries[0].Values.Count - 1; // Assuming equal count for all series

            //var prevMinY = LiveChartMinY;
            //var prevMaxY = LiveChartMaxY
            //    ;
            ////MinY = allValues.Min() - 1;
            //LiveChartMinY = 0;
            //LiveChartMaxY = allValues.Max() + 1;

            //if (LiveChartMinY < prevMinY)
            //    ;

            //if (LiveChartMaxY > prevMaxY)
            //    ;

        }

        //Updates text value in a smoother way
        //Update from previous value to current value in 400ms, 100ms for each step
        private void SetLiveEnergyLecture()
        {
            var target = ((ChartValues<ObservableValue>)LiveSeries[0].Values).Last().Value;
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
            LiveSeries[0].Values.Add(new ObservableValue(new Random().Next(0, 10)));
            while (LiveSeries[0].Values.Count > 10)
                LiveSeries[0].Values.RemoveAt(0);
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
