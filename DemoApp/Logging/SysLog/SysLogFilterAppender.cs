using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoApp.Config;

namespace DemoApp.Logging.SysLog
{
    public class SysLogFilterAppender : AppenderSkeleton
    {
        private List<IAppender> _appenders;
        private FilterSkeleton _filter;

        public SysLogFilterAppender(Log4NetLogLevel min, Log4NetLogLevel max, IEnumerable<IAppender> appenders)
        {
            Level minLevel = Level.Off;
            switch(min)
            {
                case Log4NetLogLevel.Debug:
                    minLevel = Level.Debug;
                    break;
                case Log4NetLogLevel.Info:
                    minLevel = Level.Info;
                    break;
                case Log4NetLogLevel.Warn:
                    minLevel = Level.Warn;
                    break;
                case Log4NetLogLevel.Error:
                    minLevel = Level.Error;
                    break;
                case Log4NetLogLevel.Fatal:
                    minLevel = Level.Fatal;
                    break;
            }

            Level maxLevel = Level.Off;
            switch (max)
            {
                case Log4NetLogLevel.Debug:
                    maxLevel = Level.Debug;
                    break;
                case Log4NetLogLevel.Info:
                    maxLevel = Level.Info;
                    break;
                case Log4NetLogLevel.Warn:
                    maxLevel = Level.Warn;
                    break;
                case Log4NetLogLevel.Error:
                    maxLevel = Level.Error;
                    break;
                case Log4NetLogLevel.Fatal:
                    maxLevel = Level.Fatal;
                    break;
            }

            _filter = new LevelRangeFilter { LevelMin = minLevel, LevelMax = maxLevel };
            _appenders = appenders.ToList();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_filter.Decide(loggingEvent) == FilterDecision.Accept)
            {
                // Log event passes the filter, forward to configured appenders
                foreach (IAppender appender in _appenders)
                {
                    appender?.DoAppend(loggingEvent);
                }
            }
        }

        public FilterSkeleton Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }
}
