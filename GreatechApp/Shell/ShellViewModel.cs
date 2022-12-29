using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Variable;
using IOManager;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;
using TCPIPManager;

namespace GreatechApp.Shell
{
    public class ShellViewModel : BindableBase
    {
        private string m_Title = "Main";
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }

        IShowDialog ShowDialog;
        ITCPIP TCPIP;
        IInsightVision InsightVision;
        ICodeReader CodeReader;
        ISerialPort Serial;
        IBaseIO IO;
        //IBaseMotion Motion;
        //ISecsGem SecsGem;
        SystemConfig SysConfig;
        CultureResources m_CultureResources;

        public ShellViewModel(IDialogService dialogService, IContainerProvider container, CultureResources cultureResources)
        {
            ShowDialog = container.Resolve<IShowDialog>();
            TCPIP = container.Resolve<ITCPIP>();
            InsightVision = container.Resolve<IInsightVision>();
            CodeReader = container.Resolve<ICodeReader>();
            Serial = container.Resolve<ISerialPort>();
            IO = container.Resolve<IBaseIO>();
            //Motion = container.Resolve<IBaseMotion>();
            //SecsGem = container.Resolve<ISecsGem>();
            SysConfig = container.Resolve<SystemConfig>();
            m_CultureResources = cultureResources;

            Title = System.Diagnostics.Process.GetCurrentProcess().ProcessName + " ver " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + "  -  " +
                    Global.MachName;

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        #region Event
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ButtonResult dialogResult = ShowDialog.Show(DialogIcon.Question, m_CultureResources.GetDialogValue("AskConfirmCloseApp"), ButtonResult.No, ButtonResult.Yes);

            if (dialogResult == ButtonResult.Yes)
            {
                if (Global.MachineStatus == MachineStateType.Idle || Global.MachineStatus == MachineStateType.Lot_Ended || Global.MachineStatus == MachineStateType.Ready 
                    || Global.MachineStatus == MachineStateType.Init_Done || Global.MachineStatus == MachineStateType.ReInit || Global.MachineStatus == MachineStateType.CriticalAlarm || Global.MachineStatus == MachineStateType.InitFail)
                {
                    AppClosingTask();

                    e.Cancel = false;
                }
                else
                {
                    ShowDialog.Show(DialogIcon.Stop, m_CultureResources.GetDialogValue("NotAllowToCloseApp"));
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void AppClosingTask()
		{
            // Disconnect TCP IP
            for (int i = 0; i < TCPIP.clientSockets.Count; i++)
            {
                TCPIP.clientSockets[i].Disconnect();
            }

            // Disconnect Serial Port
            for (int i = 0; i < Serial.serialPortCollection.Count; i++)
            {
                Serial.serialPortCollection[i].Close();
            }

            // Close IO (Vacuum / Air Pressure etc...)
            IO.WriteBit((int)OUT.DO0101_RedTowerLight, false);
            IO.WriteBit((int)OUT.DO0102_AmberTowerLight, false);
            IO.WriteBit((int)OUT.DO0103_GreenTowerLight, false);
            IO.WriteBit((int)OUT.DO0100_Buzzer, false);
        }
        #endregion
    }
}
