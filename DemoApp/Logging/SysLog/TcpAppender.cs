using DemoApp.Id;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    //https://github.com/TSYS-Merchant/syslog4net/
    public class TcpAppender : AppenderSkeleton, ISysLogAppender
    {
        private static int _failedConnectionCount = 0;
        public bool Testing { get; set; }
        public event EventHandler<EventArgs> TestSuccess;
        public event EventHandler<SyslogErrorEventArgs> TestFailed;

        #region Public Instance Constructors

        public TcpAppender()
        {
            Testing = false;

            // set port to some invalid value, forcing you to set the port
            this._remotePort = IPEndPoint.MinPort - 1;
        }

        private class AsyncLoggingData
        {
            internal TcpClient Client { get; set; }
            internal LoggingEvent LoggingEvent { get; set; }
            internal string Message { get; set; }
        }

        #endregion Public Instance Constructors

        #region Public Instance Properties
      
        public IPAddress RemoteAddress
        {
            get { return this._remoteAddress; }
            set { this._remoteAddress = value; }
        }

        public int RemotePort
        {
            get { return this._remotePort; }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("value", value,
                        "The value specified is less than " +
                        IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) +
                        " or greater than " +
                        IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
                }
                else
                {
                    this._remotePort = value;
                }
            }
        }
        public Encoding Encoding
        {
            get { return this._encoding; }
            set { this._encoding = value; }
        }

        #endregion Public Instance Properties

        #region Implementation of IOptionHandler

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (this.RemoteAddress == null)
            {
                throw new ArgumentNullException("RemoteAddress");
            }

            if (this.RemotePort < IPEndPoint.MinPort || this.RemotePort > IPEndPoint.MaxPort)
            {
                throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("RemotePort", this.RemotePort,
                    "The RemotePort is less than " +
                    IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) +
                    " or greater than " +
                    IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
            }
        }

        #endregion

        #region Override implementation of AppenderSkeleton

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (Testing)
            {
                if ((loggingEvent.MessageObject as string).StartsWith(Logs.SysLogTestMessageId))
                {
                    AppendTest(loggingEvent);
                    return;
                }
            }

            try
            {
                TcpClient client = new TcpClient();

                string message = RenderLoggingEvent(loggingEvent);

                //Async Programming Model allows socket connection to happen on threadpool so app can continue.
                client.BeginConnect(
                    this.RemoteAddress,
                    this.RemotePort,
                    this.ConnectionEstablishedCallback,
                    new AsyncLoggingData()
                    {
                        Client = client,
                        LoggingEvent = loggingEvent,
                        Message = message
                    });
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(
                    "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                    this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
        }

        private void ConnectionEstablishedCallback(IAsyncResult asyncResult)
        {
            // TODO callback happens on background thread. lose data if app pool recycled?
            AsyncLoggingData loggingData = asyncResult.AsyncState as AsyncLoggingData;
            if (loggingData == null)
            {
                throw new ArgumentException("LoggingData is null", "loggingData");
            }

            try
            {
                loggingData.Client.EndConnect(asyncResult);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedConnectionCount);
                if (_failedConnectionCount >= 1)
                {
                    //We have failed to connect to all the IP Addresses. connection has failed overall.
                    ErrorHandler.Error(
                        "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                        this.RemotePort + ".", ex, ErrorCode.FileOpenFailure);
                    return;
                }
            }

            try
            {
                Byte[] buffer = this._encoding.GetBytes(loggingData.Message.ToCharArray());
                using (var netStream = loggingData.Client.GetStream())
                {
                    netStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(
                    "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                    this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
            finally
            {
                loggingData.Client.Close();
            }
        }

        private void AppendTest(LoggingEvent loggingEvent)
        {
            try
            {
                TcpClient client = new TcpClient();

                string message = RenderLoggingEvent(loggingEvent);

                //Async Programming Model allows socket connection to happen on threadpool so app can continue.
                client.BeginConnect(
                    this.RemoteAddress,
                    this.RemotePort,
                    this.ConnectionEstablishedCallbackTest,
                    new AsyncLoggingData()
                    {
                        Client = client,
                        LoggingEvent = loggingEvent,
                        Message = message
                    });
            }
            catch (Exception ex)
            {
                TestFailed?.Invoke(this, new SyslogErrorEventArgs
                {
                    Message = "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " + this.RemotePort + ".",
                    Exception = ex,
                    ErrorCode = ErrorCode.WriteFailure

                });

                ErrorHandler.Error(
                    "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                    this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
        }

        private void ConnectionEstablishedCallbackTest(IAsyncResult asyncResult)
        {
            // TODO callback happens on background thread. lose data if app pool recycled?
            AsyncLoggingData loggingData = asyncResult.AsyncState as AsyncLoggingData;
            if (loggingData == null)
            {
                throw new ArgumentException("LoggingData is null", "loggingData");
            }

            try
            {
                loggingData.Client.EndConnect(asyncResult);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedConnectionCount);
                if (_failedConnectionCount >= 1)
                {
                    TestFailed?.Invoke(this, new SyslogErrorEventArgs
                    {
                        Message = "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " + this.RemotePort + ".",
                        Exception = ex,
                        ErrorCode = ErrorCode.WriteFailure

                    });

                    //We have failed to connect to all the IP Addresses. connection has failed overall.
                    ErrorHandler.Error(
                        "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                        this.RemotePort + ".", ex, ErrorCode.FileOpenFailure);
                    return;
                }
            }

            try
            {
                Byte[] buffer = this._encoding.GetBytes(loggingData.Message.ToCharArray());
                using (var netStream = loggingData.Client.GetStream())
                {
                    netStream.Write(buffer, 0, buffer.Length);
                }
                TestSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                TestFailed?.Invoke(this, new SyslogErrorEventArgs
                {
                    Message = "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " + this.RemotePort + ".",
                    Exception = ex,
                    ErrorCode = ErrorCode.WriteFailure

                });

                ErrorHandler.Error(
                    "Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " +
                    this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
            finally
            {
                loggingData.Client.Close();
            }
        }

       
        override protected bool RequiresLayout
        {
            get { return true; }
        }

        override protected void OnClose()
        {
            base.OnClose();
        }

        #endregion Override implementation of AppenderSkeleton

        #region Private Instance Fields
        private IPAddress _remoteAddress;

        private int _remotePort;

        private Encoding _encoding = Encoding.UTF8;

        #endregion Private Instance Fields
    }
}
