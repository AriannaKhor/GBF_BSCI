using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using IOManager;
using Prism.Ioc;
using Prism.Mvvm;
using Sequence;
using System;
using System.Globalization;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using TCPIPManager;
using UIModule.StandardViews.Services;

namespace GreatechApp.SplashScreen
{
    public class SplashScreenViewModel : BindableBase
    {
        #region Variables
        private object m_sqlKey = new object();
         
        IContainerProvider m_container;
        //IBaseMotion Motion;
        IBaseIO IO;
        ITCPIP TCPIP;
        ISerialPort Serial;
        ISQLOperation SQLOperation;
        //ISecsGem SecsGem;
        SystemConfig SysConfig;
        CultureResources m_CultureResources;

        public Action CloseDialogCallback;
        public Action<string,int> UpdateStatusCallback;
        #endregion

        public SplashScreenViewModel(IContainerProvider container, CultureResources cultureResources)
        {
            m_container = container;
            m_CultureResources = cultureResources;
            #region Culture
            CultureInfo cultureLoaded = CultureInfo.GetCultureInfoByIetfLanguageTag(LocUtil.GetCurrentCultureName());
            Global.CurrentCulture = cultureLoaded;
            if (Thread.CurrentThread.CurrentUICulture != null && !Thread.CurrentThread.CurrentUICulture.Equals(cultureLoaded))
            {
                m_CultureResources.ChangeCulture(cultureLoaded);
            }
            #endregion
        }

        public void Initialization()
        {
            #region Configuration
            UpdateStatus(m_CultureResources.GetStringValue("Config"), 20);

            SysConfig = m_container.Resolve<SystemConfig>();

            Global.MachName = SysConfig.Machine.EquipName;
            //Global.MachName = SysConfig.Machine.OperaName;
            Global.MachNo = int.Parse(SysConfig.Machine.MachineID);

            MachineName = Global.MachName;

            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName asmName = asm.GetName();
            Global.SoftwareVersion = asmName.Version.ToString();

            SoftwareVersion = Global.SoftwareVersion;
            #endregion

            #region SQL Database
            UpdateStatus(m_CultureResources.GetStringValue("Database"), 40);

            var sqlService = SysConfig.General.SQLService;
            SQLOperation = m_container.Resolve<ISQLOperation>();
            SQLOperation.serviceCtrl = new ServiceController(sqlService);

            if (!SQLOperation.EvalSQLService(m_sqlKey))
            {
                MessageBox.Show($"{m_CultureResources.GetStringValue("SQLService")} : {sqlService} {m_CultureResources.GetStringValue("FailConnect")}", m_CultureResources.GetStringValue("DatabaseError"), MessageBoxButton.OK);
            }
            #endregion

            #region IO
            UpdateStatus(m_CultureResources.GetStringValue("IO"), 60);
            IO = m_container.Resolve<IBaseIO>();

            //IO.MaxBitPerDevice = SysConfig.DigitalIO.MaxBitPerPort;

            if (IO.OpenDevice())
            {
                IO.StartScanIO();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
#if !SIMULATION
                    MessageBox.Show($"{m_CultureResources.GetStringValue("IOError")} : {IO.ErrorMsg}", m_CultureResources.GetStringValue("IOConnection"), MessageBoxButton.OK);
#endif
                });
            }
#endregion

            #region Motion
//            UpdateStatus(m_CultureResources.GetStringValue("Motor"), 60);
//            Motion = m_container.Resolve<IBaseMotion>();

//            for (int i = 0; i < Motion.TotalCards; i++)
//            {
//                if (!Motion.IsConnect(SysConfig.MotionCards[i].CardID))
//                {
//                    Application.Current.Dispatcher.Invoke(() =>
//                    {
//#if !SIMULATION
//                        MessageBox.Show($"{m_CultureResources.GetStringValue("MotorError")} : {Motion.ErrorMsg}", m_CultureResources.GetStringValue("MotionControllerConnection"), MessageBoxButton.OK);
//#endif
//                    });
//                }
//            }
            #endregion

            #region TCPIP
            UpdateStatus(m_CultureResources.GetStringValue("TCPIP"), 80);

            if (SysConfig.NetworkDevices.Count > 0)
            {
                TCPIP = m_container.Resolve<ITCPIP>();

                Thread.Sleep(500);

                for (int i = 0; i < TCPIP.clientSockets.Count; i++)
                {
                    if (!TCPIP.clientSockets[i].IsAlive)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
#if !SIMULATION
                            MessageBox.Show($"{m_CultureResources.GetStringValue("TCPIP")} : [ {TCPIP.clientSockets[i].Name} ] {m_CultureResources.GetStringValue("FailConnect")}", m_CultureResources.GetStringValue("TCPIP"), MessageBoxButton.OK);
#endif
                        });
                    }
                }
            }
            #endregion

            #region Serial Port
//            UpdateStatus(m_CultureResources.GetStringValue("SerialPort"), 80);

//            if (SysConfig.SerialPortRef.Count > 0)
//            {
//                Serial = m_container.Resolve<ISerialPort>();

//                for (int i = 0; i < Serial.serialPortCollection.Count; i++)
//                {
//                    if (!Serial.serialPortCollection[i].IsOpen)
//                    {
//                        Application.Current.Dispatcher.Invoke(() =>
//                        {
//#if !SIMULATION
//                            MessageBox.Show($"{m_CultureResources.GetStringValue("SerialPort")} : [ {Serial.serialPortCollection[i].PortName} ] {m_CultureResources.GetStringValue("FailConnect")}", m_CultureResources.GetStringValue("SerialPort"), MessageBoxButton.OK);
//#endif
//                        });
//                    }
//                }
//            }
            #endregion

            #region SecsGem
            //UpdateStatus(m_CultureResources.GetStringValue("SecsGem"), 85);

            //SecsGem = m_container.Resolve<SecsGemBase>();
            #endregion

            #region Utilities
            m_container.Resolve<TowerLight>();

            Global.MachineStatus = MachineStateType.Idle;

            m_container.Resolve<OEECalculation>();
            #endregion

            #region Sequence
            UpdateStatus(m_CultureResources.GetStringValue("Sequence"), 90);

            m_container.Resolve<DelegateSeq>();
            #endregion

            #region Others
            UpdateStatus(m_CultureResources.GetStringValue("LoadComplete"), 100);

            Application.Current.Dispatcher.Invoke(() =>
            {
                CloseDialog();
            });
            #endregion
        }

        #region Properties
        public string MachineName { get; set; }
        public string SoftwareVersion { get; set; }
        #endregion

        public void UpdateStatus(string title, int value)
        {
            UpdateStatusCallback(title, value);
        }

        public void CloseDialog()
        {
            CloseDialogCallback();
        }
    }
}
