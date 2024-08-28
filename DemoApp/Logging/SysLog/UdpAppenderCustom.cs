using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Logging.SysLog
{
    public class UdpAppenderCustom : AppenderSkeleton
    {
        #region Public Instance Constructors

        public UdpAppenderCustom()
        {
            // set port to some invalid value, forcing you to set the port
            this._remotePort = IPEndPoint.MinPort - 1;
        }

        private class AsyncLoggingData
        {
            internal UdpClient Client { get; set; }
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
            UdpClient udpClient = null;
            try
            {
                // Create a new UdpClient
                udpClient = new UdpClient();

                string message = RenderLoggingEvent(loggingEvent);

                // Convert the message string to a byte array
                byte[] data = this._encoding.GetBytes(message.ToCharArray());

                // Send the message asynchronously to the specified remote address and port
                udpClient.BeginSend(data,
                                    data.Length,
                                    new IPEndPoint(this.RemoteAddress, this.RemotePort),
                                    new AsyncCallback(SendCallback),
                                    udpClient);
                
            }
            catch (Exception ex)
            {
                if (udpClient != null)
                {
                    udpClient.Dispose();
                }

                ErrorHandler.Error("Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " + this.RemotePort + ".",
                                   ex,
                                   ErrorCode.WriteFailure);
            }            
        }


        private void SendCallback(IAsyncResult ar)
        {
            UdpClient udpClient = null;
            try
            {
                // Retrieve the UdpClient from the IAsyncResult
                udpClient = (UdpClient)ar.AsyncState;

                // Complete the asynchronous send operation
                udpClient.EndSend(ar);
            }
            catch (Exception ex)
            {
                if (udpClient != null)
                {
                    udpClient.Dispose();
                }

                ErrorHandler.Error("Unable to send logging event to remote host " + this.RemoteAddress.ToString() + " on port " + this.RemotePort + ".",
                                   ex,
                                   ErrorCode.WriteFailure);
            }
            finally 
            {
                if (udpClient != null)
                {
                    udpClient.Dispose();
                }
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
