using Cognex.InSight;
using ConfigManager;
using GreatechApp.Core.Variable;
using GreatechApp.Services.Utilities;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TCPIPManager
{
   public class InSightVision
    {
        #region Variable
        private string topvisIp;
        private InSightDisplayControl formVis;
        protected DispatcherTimer tmrScanIOEnableLive;
        protected DispatcherTimer tmrScanIOLiveAcquire;
        private CvsInSight m_InsightV1 = new CvsInSight();
        private readonly IEventAggregator m_Events;

        #endregion

        #region Constructor
        public InSightVision(IEventAggregator eventAggregator)
        {
            m_Events = eventAggregator;

            // Configure Vision timer object //To Pop Up need 3 seconds
            tmrScanIOEnableLive = new DispatcherTimer();
            tmrScanIOEnableLive.Tick += new System.EventHandler(tmrScanIOEnableLive_Tick);
            tmrScanIOEnableLive.Interval = new TimeSpan(0, 0, 0, 3, 0);
            tmrScanIOEnableLive.IsEnabled = false;

            // Configure Vision timer object //To Enable Live 0.5 seconds
            tmrScanIOLiveAcquire = new DispatcherTimer();
            tmrScanIOLiveAcquire.Tick += new System.EventHandler(tmrScanIOLiveAcquire_Tick);
            tmrScanIOLiveAcquire.Interval = new TimeSpan(0, 0, 0, 0, 500);
            tmrScanIOLiveAcquire.IsEnabled = false;

            m_InsightV1.ResultsChanged += new System.EventHandler(InsightV1_ResultsChanged);
            m_InsightV1.StateChanged += new Cognex.InSight.CvsStateChangedEventHandler(InsightV1_StateChanged);
        }
        #endregion

        #region Method
        public void ConnectVision()
        {
            try
            {
                SystemConfig sysCfg = SystemConfig.Open(@"..\Config Section\General\System.Config");
                topvisIp = sysCfg.NetworkDevices[0].IPAddress;
                m_InsightV1.Connect(topvisIp, "admin", "", true, false);// Determine the state of the sensor
                m_InsightV1.SoftOnline = true;
                switch (m_InsightV1.State)
                {
                    case CvsInSightState.Offline:
                        VisConnectionStatus(true, false, true, "Connected Successfully in Offline mode...");
                        Console.WriteLine("The sensor is offline.");
                        break;
                    case CvsInSightState.Online:
                        VisConnectionStatus(true, false, true, "Connected");
                        Console.WriteLine("The sensor is online.");
                        break;
                    case CvsInSightState.NotConnected:
                        VisConnectionStatus(false, true, false, "Disconnected");
                        Console.WriteLine("The sensor is not connected.");
                        break;
                }
            }
            catch (CvsInSightLockedException ex)
            {
                // The sensor has been software-locked, preventing a connection.
                // Display a message and consume the exception.
                Console.WriteLine("The sensor is currently locked.");
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (CvsSensorAlreadyConnectedException ex)
            {
                // Someone is already connected to this sensor.
                // Display a message and consume the exception.
                Console.WriteLine("A user currently has an open connection to the sensor.");
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (CvsInvalidLogonException ex)
            {
                // If we receive an invalid logon, then we can look at the IsInvalidUsername property 
                // to find out more about the exception.
                if (ex.IsInvalidUsername)
                {
                    Console.WriteLine("Invalid Username");
                    MachineBase.ShowMessage(ex);
                }
                else
                {
                    Console.WriteLine("Invalid Password");
                    VisConnectionStatus(false, true, false, "Disconnected");
                    MachineBase.ShowMessage(ex);
                }
            }

            catch (CvsNetworkException ex)
            {
                // Unable to successfully connect to the sensor.
                // Display a message and consume the exception.
                Console.WriteLine("A network error occurred while connecting to the sensor: " + ex.Message);
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

            catch (Exception ex)
            {
                // Consume any other exception that may occur
                Console.WriteLine("Error: " + ex.Message);
                VisConnectionStatus(false, true, false, "Disconnected");
                MachineBase.ShowMessage(ex);
            }

        }

        public void VisConnectionStatus(bool visConnection, bool canConnect, bool canDisconnect, string status)
        {
            Global.VisionConnStatus = status;
        }

        public void VisionLive()
        {
            try
            {
                formVis = new InSightDisplayControl(topvisIp, m_Events);
                formVis.Show();
                tmrScanIOEnableLive.Start();

            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }
        #endregion

        #region Event
        private void tmrScanIOEnableLive_Tick(object sender, System.EventArgs e) //the "EventArgs" need to use System.EventArgs else it will refer to the EventArgs under TCPIPManager which is not compatoble -------------------------------->>
        {
            try
            {
                formVis.EnableLive();
                tmrScanIOLiveAcquire.Start();
                tmrScanIOEnableLive.Stop();
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        private void tmrScanIOLiveAcquire_Tick(object sender, System.EventArgs e)
        {
            try
            {
                formVis.LiveAcquire();
                tmrScanIOLiveAcquire.Stop();
            }
            catch (Exception ex)
            {
                MachineBase.ShowMessage(ex);
            }
        }

        #endregion


    }
}
