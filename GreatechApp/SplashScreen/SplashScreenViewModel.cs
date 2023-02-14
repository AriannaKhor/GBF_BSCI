using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using IOManager;
using Prism.Ioc;
using Prism.Mvvm;
using Sequence;
using SerialPortManager;
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
        IBaseIO IO;
        IInsightVision Vision;
        ITCPIP Tcpip;
        ISerialPort Serial;
        ISQLOperation SQLOperation;
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

            #region Vision
            UpdateStatus("Vision", 50);
            Vision = m_container.Resolve<IInsightVision>();
#if !SIMULATION
            if (Vision.VisConnectionStatus())
            {
                Vision.ConnectVision();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{m_CultureResources.GetStringValue("Vision Connect Error")} ", m_CultureResources.GetStringValue("Vision Connection"), MessageBoxButton.OK);
                });
            }
#endif

#endregion

            #region Serial Port
            UpdateStatus("Serial Port", 60);
            Serial = m_container.Resolve<ISerialPort>();
#if !SIMULATION
            if (!Serial.IsPortOpen())
            {
                Serial.OpenSerialPort();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{m_CultureResources.GetStringValue("Serial Connection Error")}", m_CultureResources.GetStringValue("Serial Port Connect"), MessageBoxButton.OK);
                });
            }
#endif
            #endregion

            #region MES 
            UpdateStatus("MES", 70);
            Tcpip = m_container.Resolve<ITCPIP>();
            int ConnectClientCount = 0;
#if !SIMULATION
            if (Tcpip.clientSockets.Count > 0)
            {
                for (int i = 0; i<Tcpip.clientSockets.Count; i++)
                {
                    if (!Tcpip.clientSockets[i].IsAlive)
                    {
                        if(ConnectClientCount < 3)
                        {
                            ConnectClientCount++;
                            Tcpip.ConnectTCPClientNetworkDevices();
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"{m_CultureResources.GetStringValue("MESConnectionError")}", m_CultureResources.GetStringValue("MES Connection"), MessageBoxButton.OK);
                            });
                        }
                     
                    }
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{m_CultureResources.GetStringValue("MESConnectionError")}", m_CultureResources.GetStringValue("MES Connection"), MessageBoxButton.OK);
                });
            }
#endif
            #endregion

            #region IO
            UpdateStatus(m_CultureResources.GetStringValue("IO"), 80);
            IO = m_container.Resolve<IBaseIO>();
#if !SIMULATION
            if (IO.OpenDevice())
            {
                IO.StartScanIO();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    MessageBox.Show($"{m_CultureResources.GetStringValue("IOError")} : {IO.ErrorMsg}", m_CultureResources.GetStringValue("IOConnection"), MessageBoxButton.OK);

                });
            }
#endif
            #endregion

            #region Utilities
            m_container.Resolve<TowerLight>();
            Global.MachineStatus = MachineStateType.Idle;
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
