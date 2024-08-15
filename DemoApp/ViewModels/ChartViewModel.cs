using DemoApp.Services;
using DemoApp.Stores;
using DemoAppDatabase.Model;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private double _minuteAvgMaxY;
        public double MinuteAvgMaxY
        {
            get { return _minuteAvgMaxY; }
            set { _minuteAvgMaxY = value; OnPropertyChanged(nameof(MinuteAvgMaxY)); }
        }

        private double _lastLecture;
        public double LastLecture
        {
            get { return _lastLecture; }
            set
            {
                _lastLecture = value;
                OnPropertyChanged("LastLectureString");
            }
        }
        public string LastLectureString { get { return _lastLecture.ToString("F1"); } }

        private double _liveChartTrend;
        public SeriesCollection LiveSeries { get; set; }

        private int _liveEnergyIndex;
        private double[] _liveEnergyBuffer;

        private int _prevMinute;





        private SeriesCollection _minAvgSeries;
        public SeriesCollection MinAvgSeries
        {
            get { return _minAvgSeries; }
            set
            {
                _minAvgSeries = value;
                OnPropertyChanged(nameof(MinAvgSeries));
            }
        }

        private string _tenMinuteAvg;
        public string TenMinuteAvgString
        {
            get { return _tenMinuteAvg; }
            set { _tenMinuteAvg = value; OnPropertyChanged(nameof(TenMinuteAvgString)); }
        }


        public ChartValues<DateTimePoint> Values { get; set; }
        public Func<double, string> MinAvgXFormatter { get; set; }

        public Func<double, string> MinAvgYFormatter { get; set; }


        #region ICommand

        //RelayCommands work like in Locker AFAIK
        //DelegateCommand is similar, but doesn't test command availabiltiy automatically.
        //Also easy usage of types with <T>



        //private DelegateCommand<object> _clickCommand;
        //public ICommand ClickCommand
        //{
        //    get
        //    {
        //        if (_clickCommand == null)
        //        {
        //            _clickCommand = new DelegateCommand<object>(UpdateAllOnClick);
        //        }
        //        return _clickCommand;
        //    }
        //}

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
            InitMinAvgChart();
            

        }

        #region Live chart
        public void InitEnergyLiveChart()
        {
            DateTime now = DateTime.Now;
            _prevMinute = now.Minute;


            _liveEnergyIndex = 0;
            _liveEnergyBuffer = new double[120];


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

                    Task.Run(() => _databaseService.AddOrUpdateEnergyMinAvg(DateTime.Now, minuteAvg));

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        try
                        {
                            MinAvgSeries[0].Values.Add(new ObservableValue(minuteAvg));
                            MinAvgSeries[0].Values.RemoveAt(0);
                            OnPropertyChanged(nameof(MinAvgSeries));
                            UpdateAvgMinAxisRanges();
                            UpdateTenMinuteAvg();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                    });

                    _liveEnergyIndex = 0;
                    
                   
                }
                _prevMinute = currentMinute;

                _liveEnergyBuffer[_liveEnergyIndex++] = _liveChartTrend;

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        LiveSeries[0].Values.Add(new ObservableValue(_liveChartTrend));
                        LiveSeries[0].Values.RemoveAt(0);
                        SetLiveEnergyLecture();
                        UpdateLiveEnergyAxisRanges();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                });
            }
        }

        private void UpdateLiveEnergyAxisRanges()
        {
            var allValues = LiveSeries
               .SelectMany(series => series.Values.Cast<ObservableValue>())
               .Select(value => value.Value);

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
        #endregion


        #region Minute Average chart
        public void InitMinAvgChart()
        {
            DateTime now = DateTime.Now;
            IEnumerable<EnergyMinAvgRecord> minAvgs = _databaseService.GetEnergyMinAvg(now.AddMinutes(-10), now);

            MinAvgSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0)
                    }
                }
            };


            foreach (var record in minAvgs)
            {
                TimeSpan span = now - record.Time;
                int index = 0;
                for (int i = 11, j = 9; j >= 0; i--, j--)
                {
                    if (span < TimeSpan.FromMinutes(i) &&
                        span > TimeSpan.FromMinutes(j))
                    {
                        MinAvgSeries[0].Values[index] = new ObservableValue(record.Value);
                    }
                    index++;
                }
            }

            MinAvgXFormatter = value => DateTime.Now.AddMinutes(value - MinAvgSeries[0].Values.Count + 1).ToString("mm");
            MinAvgYFormatter = value => value == 0 ? string.Empty : value.ToString();
            UpdateAvgMinAxisRanges();

            UpdateTenMinuteAvg();
        }

        public void UpdateTenMinuteAvg()
        {
            double tenMinuteAvg = 0;
            int minAvgCount = MinAvgSeries[0].Values.Count;
            for (int i = 0; i < minAvgCount; i++)
            {
                tenMinuteAvg += (MinAvgSeries[0].Values[i] as ObservableValue).Value;
            }
            tenMinuteAvg /= minAvgCount;
            TenMinuteAvgString = tenMinuteAvg.ToString("F1");
        }

        private void UpdateAvgMinAxisRanges()
        {
            var allValues = MinAvgSeries
               .SelectMany(series => series.Values.Cast<ObservableValue>())
               .Select(value => value.Value);

            MinuteAvgMaxY = allValues.Max() + 1;

            OnPropertyChanged(nameof(MinAvgXFormatter));
            OnPropertyChanged(nameof(MinAvgYFormatter));
        }
        #endregion




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
