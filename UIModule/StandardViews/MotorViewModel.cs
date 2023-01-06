//#define ACSMotion

using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using InterlockManager.Motion;
using MotionManager;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UIModule.MainPanel;
using static ConfigManager.MotAxis;

namespace UIModule.StandardViews
{
    public class MotorViewModel : BaseUIViewModel, INavigationAware
    {
        #region Variable
        private Timer tmr_UpdateStatus;

        private string m_title = string.Empty;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private double m_CurrentPos = 0;
        public double CurrentPos
        {
            get { return m_CurrentPos; }
            set { SetProperty(ref m_CurrentPos, value); }
        }

        private string m_UoM;
        public string UoM
        {
            get { return m_UoM; }
            set { SetProperty(ref m_UoM, value); }
        }

        private double m_MovingPos = 0;
        public double MovingPos
        {
            get { return m_MovingPos; }
            set { SetProperty(ref m_MovingPos, value); }
        }

        private string m_JogPos = "0";
        public string JogPos
        {
            get { return m_JogPos; }
            set { SetProperty(ref m_JogPos, value); }
        }

        private string m_Timer = "0";
        public string Timer
        {
            get { return m_Timer; }
            set { SetProperty(ref m_Timer, value); }
        }

        private bool m_ServoOn;
        public bool ServoOn
        {
            get { return m_ServoOn; }
            set { SetProperty(ref m_ServoOn, value); }
        }

        private bool m_IsMtrServoOn;
        public bool IsMtrServoOn
        {
            get { return m_IsMtrServoOn; }
            set { SetProperty(ref m_IsMtrServoOn, value); }
        }

        private SolidColorBrush m_HighBackground = Brushes.Silver;
        public SolidColorBrush HighBackground
        {
            get { return m_HighBackground; }
            set { SetProperty(ref m_HighBackground, value); }
        }

        private SolidColorBrush m_MedBackground = Brushes.Silver;
        public SolidColorBrush MedBackground
        {
            get { return m_MedBackground; }
            set { SetProperty(ref m_MedBackground, value); }
        }

        private SolidColorBrush m_LowBackground = Brushes.Silver;
        public SolidColorBrush LowBackground
        {
            get { return m_LowBackground; }
            set { SetProperty(ref m_LowBackground, value); }
        }

        private BitmapImage m_Alarm = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Alarm
        {
            get { return m_Alarm; }
            set { SetProperty(ref m_Alarm, value); }
        }

        private BitmapImage m_InPos = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage InPos
        {
            get { return m_InPos; }
            set { SetProperty(ref m_InPos, value); }
        }

        private BitmapImage m_Busy = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Busy
        {
            get { return m_Busy; }
            set { SetProperty(ref m_Busy, value); }
        }

        private BitmapImage m_Ready = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Ready
        {
            get { return m_Ready; }
            set { SetProperty(ref m_Ready, value); }
        }

        private BitmapImage m_Home = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage Home
        {
            get { return m_Home; }
            set { SetProperty(ref m_Home, value); }
        }

        private BitmapImage m_PosLmt = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage PosLmt
        {
            get { return m_PosLmt; }
            set { SetProperty(ref m_PosLmt, value); }
        }

        private BitmapImage m_NegLmt = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage NegLmt
        {
            get { return m_NegLmt; }
            set { SetProperty(ref m_NegLmt, value); }
        }

        private BitmapImage m_AxisHome = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage AxisHome
        {
            get { return m_AxisHome; }
            set { SetProperty(ref m_AxisHome, value); }
        }

        private BitmapImage m_ServoOnOffIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch Off.png", uriKind: System.UriKind.RelativeOrAbsolute));
        public BitmapImage ServoOnOffIcon
        {
            get { return m_ServoOnOffIcon; }
            set { SetProperty(ref m_ServoOnOffIcon, value); }
        }

        private string m_AxisHomeToolTip;
        public string AxisHomeToolTip
        {
            get { return m_AxisHomeToolTip; }
            set { SetProperty(ref m_AxisHomeToolTip, value); }
        }

        private List<MotorTabList> m_MotorList;
        public List<MotorTabList> MotorList
        {
            get { return m_MotorList; }
            set { SetProperty(ref m_MotorList, value); }
        }

        private int m_SpeedSelect = 0;
        public int SpeedSelect
        {
            get { return m_SpeedSelect; }
            set { SetProperty(ref m_SpeedSelect, value); }
        }

        private string m_Icon1;
        public string Icon1
        {
            get { return m_Icon1; }
            set { SetProperty(ref m_Icon1, value); }
        }

        private string m_Icon2;
        public string Icon2
        {
            get { return m_Icon2; }
            set { SetProperty(ref m_Icon2, value); }
        }

        private string m_Dir1;
        public string Dir1
        {
            get { return m_Dir1; }
            set { SetProperty(ref m_Dir1, value); }
        }

        private string m_Dir2;
        public string Dir2
        {
            get { return m_Dir2; }
            set { SetProperty(ref m_Dir2, value); }
        }

        private int m_Sign1;
        public int Sign1
        {
            get { return m_Sign1; }
            set { SetProperty(ref m_Sign1, value); }
        }

        private int m_Sign2;
        public int Sign2
        {
            get { return m_Sign2; }
            set { SetProperty(ref m_Sign2, value); }
        }

        private Visibility m_IsReadyVisible;
        public Visibility IsReadyVisible
        {
            get { return m_IsReadyVisible; }
            set { SetProperty(ref m_IsReadyVisible, value); }
        }

        private Visibility m_IsAlarmVisible;
        public Visibility IsAlarmVisible
        {
            get { return m_IsAlarmVisible; }
            set { SetProperty(ref m_IsAlarmVisible, value); }
        }

        private Visibility m_IsInPosVisible;
        public Visibility IsInPosVisible
        {
            get { return m_IsInPosVisible; }
            set { SetProperty(ref m_IsInPosVisible, value); }
        }

        private Visibility m_IsFwdLmtVisible;
        public Visibility IsFwdLmtVisible
        {
            get { return m_IsFwdLmtVisible; }
            set { SetProperty(ref m_IsFwdLmtVisible, value); }
        }

        private Visibility m_IsNegLmtVisible;
        public Visibility IsNegLmtVisible
        {
            get { return m_IsNegLmtVisible; }
            set { SetProperty(ref m_IsNegLmtVisible, value); }
        }

        private int m_SelectedTabIndex;
        public int SelectedTabIndex
        {
            get { return m_SelectedTabIndex; }
            set
            {
                m_SelectedTabIndex = value;
                RaisePropertyChanged(nameof(SelectedTabIndex));
                Title = MotorList[m_SelectedTabIndex].AxisName;
                RaisePropertyChanged(nameof(CanAccess));
            }
        }

        public DelegateCommand<TeachingPosition> TeachCommand { get; private set; }
        public DelegateCommand<TeachingPosition> GoCommand { get; private set; }
        public DelegateCommand<TeachingPosition> TextBoxLostFocusCommand { get; private set; }
        public DelegateCommand<string> MotorOperationCommand { get; private set; }
        public DelegateCommand<string> ChangeSpeedCommand { get; private set; }
        public DelegateCommand<Velocity> SaveSpeedCommand { get; private set; }
        public DelegateCommand InvertCommand { get; private set; }

        public bool AlarmRst = false;

        private Type[] m_MCTypes;

        bool servoON;
        bool alarmON;

        private IEnumerable<IMtrInterlock> m_MtrIntLCollection;
        private IMtrInterlock m_MtrIntL;
        #endregion

        #region Constructor

        public MotorViewModel()
        {
            m_MtrIntLCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IMtrInterlock>>>()();
            m_MtrIntL = m_MtrIntLCollection.FirstOrDefault();

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);

            m_MCTypes = m_Motion.GetType().GetInterfaces();

            MotorList = new List<MotorTabList>();
            for (int i = 0; i < m_SystemConfig.MotCfgRef.Count; i++)
            {
                MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[i].Reference);

                mtrcfg.Axis.IsHome = false;

                List<TeachingPosition> pos = new List<TeachingPosition>();
                List<Velocity> vel = new List<Velocity>();

                for (int j = 0; j < mtrcfg.Position.Count; j++)
                {
                    if (mtrcfg.Position[j].IsVisible)
                    {
                        pos.Add(new TeachingPosition()
                        {
                            ID = mtrcfg.Position[j].ID,
                            TeachingPointName = mtrcfg.Position[j].Description,
                            TeachingPointValue = mtrcfg.Position[j].Point,
                            TeachingPointUOM = mtrcfg.Position[j].UoM,
                            SoftLimit = mtrcfg.Position[j].SoftLimit,
                            SoftLimitToolTip = $" SoftLimit : {mtrcfg.Position[j].SoftLimit}",
                            IsAllowMoveMtr = false,
                        });
                    }
                }

                for (int j = 0; j < mtrcfg.Velocity.Count; j++)
                {
                    if (mtrcfg.Velocity[j].IsVisible)
                    {
                        vel.Add(new Velocity()
                        {
                            ID = mtrcfg.Velocity[j].ID,
                            RowNum = (int)(j / 2),
                            ColNum = j % 2,
                            ProfileName = mtrcfg.Velocity[j].ProfileName,
                            ProfileNameWithCulture = GetStringTableValue(mtrcfg.Velocity[j].ProfileName),
                            DriveVel = mtrcfg.Velocity[j].DriveVel,
                            Acc = mtrcfg.Velocity[j].Acc,
                            Dcc = mtrcfg.Velocity[j].Dcc,
                            MaxVel = mtrcfg.Velocity[j].MaxVel,
                            MaxAcc = mtrcfg.Velocity[j].MaxAcc,
                            JerkTime = mtrcfg.Velocity[j].JerkTime,
                            KillDcc = mtrcfg.Velocity[j].KillDcc,
                            VelUoM = mtrcfg.ViewCfg.VelUoM,
                            AccUoM = mtrcfg.ViewCfg.AccUoM,

                            IsACSMotion = m_MCTypes[1].Name == "IACSMotion" ? Visibility.Visible : Visibility.Collapsed,

                            MaxDriveVel = $"Max Drive Vel : {mtrcfg.Velocity[j].MaxVel}",
                            MaxAccVel = $"Max Acc Vel : {mtrcfg.Velocity[j].MaxAcc}",
                            MaxDccVel = $"Max Dcc Vel : {mtrcfg.Velocity[j].MaxDcc}",
                        }); ;
                    }
                }

                MotorList.Add(new MotorTabList()
                {
                    AxisName = mtrcfg.Axis.Name,
                    AxisID = mtrcfg.Axis.AxisID,
                    CardID = mtrcfg.Axis.CardID,
                    Positions = pos,
                    Velocities = vel,
                    Revolution = mtrcfg.Axis.Revolution,
                    Pitch = mtrcfg.Axis.Pitch,
                    UoM = mtrcfg.Axis.UoM,
                    Type = mtrcfg.Axis.Type.ToString(),
                    System = mtrcfg.Axis.System.ToString(),
                    // Option
                    IsChkAlarm = mtrcfg.Option.ChkAlarm,
                    IsChkReady = mtrcfg.Option.ChkReady,
                    IsChkInPos = mtrcfg.Option.ChkInPos,
                    IsChkFwdLmt = mtrcfg.Option.ChkFwdLimit,
                    IsChkRevLmt = mtrcfg.Option.ChkRevLimit,
                    IsChkAxisHome = mtrcfg.Option.ChkAxisHome,
                    AlarmContact = mtrcfg.Option.AlarmContact,
                    ReadyContact = mtrcfg.Option.ReadyContact,

                    SetZeroPosAfterGoLoad = mtrcfg.Axis.SetZeroPosAfterGoLoad,
                    Icon1 = mtrcfg.ViewCfg.Icon1,
                    Icon2 = mtrcfg.ViewCfg.Icon2,
                    Dir1 = mtrcfg.ViewCfg.Dir1,
                    Dir2 = mtrcfg.ViewCfg.Dir2,
                    Sign1 = mtrcfg.ViewCfg.Sign1,
                    Sign2 = mtrcfg.ViewCfg.Sign2,
                });
            }

            AxisHomeToolTip = GetStringTableValue("AxisHomeToolTip");

            SelectedTabIndex = 0;

            tmr_UpdateStatus = new Timer();
            tmr_UpdateStatus.Interval = 100;
            tmr_UpdateStatus.Elapsed += UpdateMotionStatus;

            TeachCommand = new DelegateCommand<TeachingPosition>(TeachMethod);
            GoCommand = new DelegateCommand<TeachingPosition>(GoMethod);
            MotorOperationCommand = new DelegateCommand<string>(MotorOperationMethod);
            ChangeSpeedCommand = new DelegateCommand<string>(ChangeSpeedMethod);
            TextBoxLostFocusCommand = new DelegateCommand<TeachingPosition>(TextBoxLostCursorMethod);
            SaveSpeedCommand = new DelegateCommand<Velocity>(SaveSpeedMethod);
            InvertCommand = new DelegateCommand(InvertMethod);
            ChangeSpeedMethod("Low");
        }

        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return true;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            m_EventAggregator.GetEvent<TestRunResult>().Unsubscribe(OnTestRunResult);
            tmr_UpdateStatus.Stop();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RaisePropertyChanged(nameof(CanAccess));
            RaisePropertyChanged(nameof(IsAllowAccess));
            tmr_UpdateStatus.Start();
            m_EventAggregator.GetEvent<TestRunResult>().Subscribe(OnTestRunResult);
            if (Global.InitDone && (Global.MachineStatus == MachineStateType.Lot_Ended || Global.MachineStatus == MachineStateType.Ready || Global.MachineStatus == MachineStateType.Init_Done))
            {
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.ReInit);
            }
        }
        #endregion

        #region Event
        private void UpdateMotionStatus(object sender, EventArgs e)
        {
            try
            {
                MotorTabList motor = MotorList[SelectedTabIndex];

                IsReadyVisible = motor.IsChkReady ? Visibility.Visible : Visibility.Collapsed;
                IsAlarmVisible = motor.IsChkAlarm ? Visibility.Visible : Visibility.Collapsed;
                IsInPosVisible = motor.IsChkInPos ? Visibility.Visible : Visibility.Collapsed;
                IsFwdLmtVisible = motor.IsChkFwdLmt ? Visibility.Visible : Visibility.Collapsed;
                IsNegLmtVisible = motor.IsChkRevLmt ? Visibility.Visible : Visibility.Collapsed;

#if !SIMULATION || ACSMotion

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (motor.IsChkAlarm)
                    {
                        bool alarmStatus = m_Motion.GetAlarmStatus(motor.CardID, motor.AxisID);
                        bool alarmContact = motor.AlarmContact == Contact.NO ? alarmStatus : !alarmStatus;

                        if (alarmContact)
                        {
                            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);
                            mtrcfg.Axis.IsHome = false;
                        }

                        alarmON = alarmContact;
                        Alarm = alarmContact ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/RedIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
                    }

                    if (motor.IsChkInPos)
                    {
                        InPos = m_Motion.GetMotionDoneStatus(motor.CardID, motor.AxisID) ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
                    }

                    Busy = m_Motion.AxisInMotion(motor.CardID, motor.AxisID) ?
                                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

                    if (motor.IsChkReady)
                    {
                        bool readyStatus = m_Motion.GetServoStatus(motor.CardID, motor.AxisID) && m_Motion.GetMotionDoneStatus(motor.CardID, motor.AxisID);
                        bool readyContact = motor.ReadyContact == Contact.NO ? readyStatus : !readyStatus;

                        Ready = readyContact ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
                    }

                    if (motor.IsChkFwdLmt)
                    {
                        PosLmt = m_Motion.GetPositiveLimitStatus(motor.CardID, motor.AxisID) ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
                    }

                    if (motor.IsChkRevLmt)
                    {
                        NegLmt = m_Motion.GetNegativeLimitStatus(motor.CardID, motor.AxisID) ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));
                    }

                    Home = m_Motion.GetHomeLimitStatus(motor.CardID, motor.AxisID) ?
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

                    AxisHome = IsAxisHome(motor.IsChkAxisHome) ?
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                    new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GrayIcon.png", uriKind: System.UriKind.RelativeOrAbsolute));

                    servoON = m_Motion.GetServoStatus(motor.CardID, motor.AxisID);

                    IsMtrServoOn = servoON;

                    if (!servoON)
                    {
                        MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);
                        mtrcfg.Axis.IsHome = false;
                    }

                    ServoOnOffIcon = servoON ? new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch On.png", uriKind: System.UriKind.RelativeOrAbsolute)) :
                                               new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch Off.png", uriKind: System.UriKind.RelativeOrAbsolute));
                });

                double CurPulse = 0;
                if (m_MCTypes[1].Name == "IGalilMotion")
                {
                    // Stepper motor using Galil controller will retrieve from TD
                    CurPulse = EnumHelper.GetValueFromDescription<MotorType>(motor.Type) == MotAxis.MotorType.Servo ? m_Motion.GetEncoderPosition(motor.CardID, motor.AxisID) : m_Motion.GetLogCnt(motor.CardID, motor.AxisID);
                }
                else
                {
                    CurPulse = m_Motion.GetEncoderPosition(motor.CardID, motor.AxisID);
                }

                if (m_MCTypes[1].Name == "IACSMotion")
                {
                    CurrentPos = Math.Round(CurPulse, 4);
                }
                else if (m_MCTypes[1].Name == "IAdvantechMotion")
                {
                    CurrentPos = EnumHelper.GetValueFromDescription<DriveMethod>(motor.System) == MotAxis.DriveMethod.Linear ? Math.Round(m_Motion.Pulse2mm(motor.Revolution, motor.Pitch, (float)CurPulse), 4) : Math.Round(m_Motion.Pulse2degree(motor.Revolution, (float)CurPulse), 4);
                }
                else
                {
                    CurrentPos = EnumHelper.GetValueFromDescription<DriveMethod>(motor.System) == MotAxis.DriveMethod.Linear ? Math.Round(m_Motion.Pulse2mm(motor.Revolution, motor.Pitch, (int)CurPulse), 4) : Math.Round(m_Motion.Pulse2degree(motor.Revolution, (float)CurPulse), 4);
                }
#endif
                UoM = EnumHelper.GetValueFromDescription<DriveMethod>(motor.System) == MotAxis.DriveMethod.Linear ? GetStringTableValue("mm") : GetStringTableValue("degree");
                Icon1 = motor.Icon1;
                Icon2 = motor.Icon2;
                Dir1 = GetStringTableValue(motor.Dir1);
                Dir2 = GetStringTableValue(motor.Dir2);

            }
            catch (Exception ex)
            {
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{GetStringTableValue("Motor")} {GetStringTableValue("Error")} : {ex.Message}" });
                tmr_UpdateStatus.Stop();
            }
        }

        private void TextBoxLostCursorMethod(TeachingPosition parameter)
        {
            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);
            double originalPoint = mtrcfg.Position[parameter.ID].Point;

            if (originalPoint != parameter.TeachingPointValue)
            {
                ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmMakeChanges"), ButtonResult.Cancel, ButtonResult.Yes);

                if (dialogResult == ButtonResult.Yes && parameter.TeachingPointValue <= parameter.SoftLimit)
                {
                    mtrcfg.Position[parameter.ID].Point = parameter.TeachingPointValue;
                    MotionConfig.Save(mtrcfg);

                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} {MotorList[SelectedTabIndex].AxisName} : {mtrcfg.Position[parameter.ID].Description}, Old Value : {originalPoint} => New Value : {parameter.TeachingPointValue}" });
                }
                else
                {
                    m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("ValueNotMoreThan") + " " + parameter.SoftLimit);
                    parameter.TeachingPointValue = originalPoint;
                }
            }
        }

        private void OnMachineState(MachineStateType status)
        {
            Global.MachineStatus = status;
            RaisePropertyChanged(nameof(CanAccess));
        }

        public override void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
            RaisePropertyChanged(nameof(IsAllowAccess));
        }

        private void OnTestRunResult(TestRunResult result)
        {
            Application.Current.Dispatcher.Invoke(() => {
                if (string.IsNullOrEmpty(result.ErrMsg))
                {
                    m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("HomeCompleted"));
                }
                else
                {
                    m_ShowDialog.Show(DialogIcon.Stop, result.ErrMsg);
                }
            });
        }

        public override void OnCultureChanged()
        {
            foreach (MotorTabList motorTabList in MotorList)
            {
                foreach (Velocity velocity in motorTabList.Velocities)
                    velocity.ProfileNameWithCulture = GetStringTableValue(velocity.ProfileName);
            }
        }
        #endregion

        #region Method
        private bool IsAxisHome(bool chkAxisHome)
        {
            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);

            if (!chkAxisHome)
            {
                for (int i = 0; i < MotorList[SelectedTabIndex].Positions.Count; i++)
                {
                    MotorList[SelectedTabIndex].Positions[i].IsAllowMoveMtr = IsMtrServoOn;
                }
                return true;
            }

            for (int i = 0; i < MotorList[SelectedTabIndex].Positions.Count; i++)
            {
                MotorList[SelectedTabIndex].Positions[i].IsAllowMoveMtr = mtrcfg.Axis.IsHome;
            }

            return mtrcfg.Axis.IsHome;
        }

        private void InvertMethod()
        {
            MovingPos *= -1;
        }

        private void TeachMethod(TeachingPosition parameter)
        {
            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmSavePosition"));

            if (dialogResult == ButtonResult.Yes)
            {
                double originalPoint = parameter.TeachingPointValue;
                parameter.TeachingPointValue = CurrentPos;

                MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);
                mtrcfg.Position[parameter.ID].Point = CurrentPos;

                MotionConfig.Save(mtrcfg);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("UpdatePosition")} : {MotorList[SelectedTabIndex].AxisName}, { mtrcfg.Position[parameter.ID].Description}, {GetStringTableValue("Position")} : {originalPoint} => {mtrcfg.Position[parameter.ID].Point}" });
            }
        }

        private void GoMethod(TeachingPosition parameter)
        {
            if (alarmON)
            {
                m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("AlarmActive"));
                return;
            }

            m_MtrIntL = m_MtrIntLCollection.FirstOrDefault(x => x.Provider == MotorList[SelectedTabIndex].MotorIndex);

            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);

            // Check Interlock
            if (m_MtrIntL != null && !m_MtrIntL.CheckMtrInterlock(mtrcfg)) return;

            if (servoON)
            {
                m_Motion.SetAxisParam(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID,
                                    mtrcfg.Velocity[SpeedSelect].DriveVel, mtrcfg.Velocity[SpeedSelect].Acc, mtrcfg.Velocity[SpeedSelect].Dcc,
                                    mtrcfg.Velocity[SpeedSelect].JerkTime, mtrcfg.Velocity[SpeedSelect].KillDcc);

                int pulse = 0;

                if (m_MCTypes[1].Name == "IACSMotion")
                {
                    pulse = (int)(parameter.TeachingPointValue * 1000);
                }
                else
                {
                    if (mtrcfg.Axis.System == DriveMethod.Linear)
                    {
                        pulse = m_MCTypes[1].Name == "IAdvantechMotion" ? m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)parameter.TeachingPointValue, true) :
                                                                          m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)parameter.TeachingPointValue);
                    }
                    else if (mtrcfg.Axis.System == DriveMethod.Rotary)
                    {
                        pulse = m_Motion.degree2pulse(mtrcfg.Axis.Revolution, (float)parameter.TeachingPointValue);
                    }
                }

                m_Motion.MoveAbs(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, pulse, mtrcfg.Dir.Opr);
            }
            else
            {
                m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("ServoNotON"));
            }
        }

        private void MotorOperationMethod(string ButtonName)
        {
            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);

            if (ButtonName == "Servo")
            {
                if (servoON)
                {
                    m_Motion.ServoOFF(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID);
                    ServoOnOffIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch Off.png", uriKind: System.UriKind.RelativeOrAbsolute));
                }
                else
                {
                    m_Motion.ServoON(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID);
                    ServoOnOffIcon = new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/Switch On.png", uriKind: System.UriKind.RelativeOrAbsolute));
                }
            }
            else if (ButtonName == "TurnOnAlarmReset")
            {
                if (mtrcfg.Option.UseIORstAlarm)
                {

                }
                else
                {
                    m_Motion.ResetMotorAlarm(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, true);
                }
            }
            else if (ButtonName == "TurnOffAlarmReset")
            {
                m_Motion.ResetMotorAlarm(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, false);
            }
            else if (ButtonName == "SetZero")
            {
                m_Motion.SetZeroPosition(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID);
            }
            else if (ButtonName == "MotorStop")
            {
                m_Motion.StopServo(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID);
            }
            else
            {
                if (m_MtrIntLCollection != null)
                    m_MtrIntL = m_MtrIntLCollection.FirstOrDefault(x => x.Provider == SelectedTabIndex);

                if (servoON)
                {
                    m_Motion.SetAxisParam(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID,
                                    mtrcfg.Velocity[SpeedSelect].DriveVel, mtrcfg.Velocity[SpeedSelect].Acc, mtrcfg.Velocity[SpeedSelect].Dcc,
                                    mtrcfg.Velocity[SpeedSelect].JerkTime, mtrcfg.Velocity[SpeedSelect].KillDcc);

                    if (ButtonName == "MoveAbs")
                    {
                        if (alarmON)
                        {
                            m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("AlarmActive"));
                            return;
                        }

                        // Check Interlock
                        if (m_MtrIntL != null)
                            if (!m_MtrIntL.CheckMtrInterlock(mtrcfg, true)) return;

                        int pulse = 0;

                        if (m_MCTypes[1].Name == "IACSMotion")
                        {
                            pulse = (int)(MovingPos * 1000);
                        }
                        else
                        {
                            if (mtrcfg.Axis.System == DriveMethod.Linear)
                            {
                                pulse = m_MCTypes[1].Name == "IAdvantechMotion" ? m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)MovingPos, true) :
                                                                                  m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)MovingPos);
                            }
                            else if (mtrcfg.Axis.System == DriveMethod.Rotary)
                            {
                                pulse = m_Motion.degree2pulse(mtrcfg.Axis.Revolution, (float)MovingPos);
                            }
                        }

                        m_Motion.MoveAbs(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, pulse, mtrcfg.Dir.Opr);
                    }
                    else if (ButtonName == "MoveRel")
                    {
                        if (alarmON)
                        {
                            m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("AlarmActive"));
                            return;
                        }

                        // Check Interlock
                        if (m_MtrIntL != null)
                            if (!m_MtrIntL.CheckMtrInterlock(mtrcfg, true)) return;

                        int pulse = 0;

                        if (m_MCTypes[1].Name == "IACSMotion")
                        {

                            pulse = (int)(MovingPos * 1000);

                            if (MovingPos > 0)
                            {
                                pulse = Math.Abs((int)pulse);
                            }
                            else
                            {
                                pulse = -(Math.Abs((int)pulse));
                            }
                        }
                        else
                        {
                            if (mtrcfg.Axis.System == DriveMethod.Linear)
                            {
                                pulse = m_MCTypes[1].Name == "IAdvantechMotion" ? m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)MovingPos, true) :
                                                                                  m_Motion.mm2Pulse(mtrcfg.Axis.Revolution, mtrcfg.Axis.Pitch, (float)MovingPos);
                            }
                            else if (mtrcfg.Axis.System == DriveMethod.Rotary)
                            {
                                pulse = m_Motion.degree2pulse(mtrcfg.Axis.Revolution, (float)MovingPos);
                            }
                        }

                        m_Motion.MoveRel(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, pulse);
                    }
                    else if (ButtonName == "StartNegJog" || ButtonName == "StartPosJog")
                    {
                        if (m_MCTypes[1].Name == "IAdvantechMotion")
                        {
                            m_Motion.SetAxisJogParam(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, mtrcfg.Velocity[SpeedSelect].DriveVel, mtrcfg.Velocity[SpeedSelect].Acc, mtrcfg.Velocity[SpeedSelect].Dcc);
                        }

                        if (alarmON)
                        {
                            m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("AlarmActive"));
                            return;
                        }

                        // Check Interlock
                        if (m_MtrIntL != null)
                            if (!m_MtrIntL.CheckMtrInterlock(mtrcfg, true)) return;

                        short direction = ButtonName == "StartNegJog" ? Convert.ToInt16(mtrcfg.ViewCfg.Sign1) : Convert.ToInt16(mtrcfg.ViewCfg.Sign2);

                        m_Motion.Jog(mtrcfg.Axis.CardID, mtrcfg.Axis.AxisID, direction, mtrcfg.Velocity[SpeedSelect].DriveVel);
                    }
                    else if (ButtonName == "MotorHome")
                    {
                        // Check Interlock
                        if (m_MtrIntL != null)
                            if (!m_MtrIntL.CheckMtrInterlock(mtrcfg)) return;

                        m_EventAggregator.GetEvent<TestRunEvent>().Publish(new TestRunEvent()
                        {
                            MtrIdx = SelectedTabIndex,
                            SeqName = (SQID)m_SystemConfig.MotCfgRef[SelectedTabIndex].SeqID,
                            TestRunSeq = TestRunEnum.SN.HomeMotor,
                        });
                    }
                }
                else
                {
                    m_ShowDialog.Show(DialogIcon.Information, GetDialogTableValue("ServoNotON"));
                }
            }
        }

        private void ChangeSpeedMethod(string ButtonName)
        {
            if (ButtonName == "High")
            {
                SpeedSelect = 0;

                HighBackground = Brushes.DeepSkyBlue;
                MedBackground = Brushes.Silver;
                LowBackground = Brushes.Silver;
            }
            else if (ButtonName == "Med")
            {
                SpeedSelect = 1;

                HighBackground = Brushes.Silver;
                MedBackground = Brushes.DeepSkyBlue;
                LowBackground = Brushes.Silver;
            }
            else if (ButtonName == "Low")
            {
                SpeedSelect = 2;

                HighBackground = Brushes.Silver;
                MedBackground = Brushes.Silver;
                LowBackground = Brushes.DeepSkyBlue;
            }
        }

        private void SaveSpeedMethod(Velocity velocity)
        {
            MotionConfig mtrcfg = MotionConfig.Open(m_SystemConfig.MotCfgRef[SelectedTabIndex].Reference);
            int velocityId = velocity.ID;
            ButtonResult dialogResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmSaveMPChanges"));

            if (dialogResult == ButtonResult.Yes && velocity.DriveVel <= mtrcfg.Velocity[velocityId].MaxVel && velocity.Acc <= mtrcfg.Velocity[velocityId].MaxAcc && velocity.Dcc <= mtrcfg.Velocity[velocityId].MaxDcc)
            {
                mtrcfg.Velocity[velocityId].DriveVel = velocity.DriveVel;
                mtrcfg.Velocity[velocityId].Acc = velocity.Acc;
                mtrcfg.Velocity[velocityId].Dcc = velocity.Dcc;
                mtrcfg.Velocity[velocityId].MaxAcc = velocity.MaxAcc;
                mtrcfg.Velocity[velocityId].MaxVel = velocity.MaxVel;
                mtrcfg.Velocity[velocityId].JerkTime = velocity.JerkTime;
                mtrcfg.Velocity[velocityId].KillDcc = velocity.KillDcc;

                MotionConfig.Save(mtrcfg);

                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Info, MsgText = $"{GetStringTableValue("User")} {m_CurrentUser.Username} {GetStringTableValue("Update")} {MotorList[SelectedTabIndex].AxisName} {GetStringTableValue("MotorProfile")} : {mtrcfg.Velocity[velocityId].ProfileName}" });
            }
            else
            {
                velocity.DriveVel = mtrcfg.Velocity[velocityId].DriveVel;
                velocity.Acc = mtrcfg.Velocity[velocityId].Acc;
                velocity.Dcc = mtrcfg.Velocity[velocityId].Dcc;
                velocity.MaxAcc = mtrcfg.Velocity[velocityId].MaxAcc;
                velocity.MaxVel = mtrcfg.Velocity[velocityId].MaxVel;
                velocity.JerkTime = mtrcfg.Velocity[velocityId].JerkTime;
                velocity.KillDcc = mtrcfg.Velocity[velocityId].KillDcc;
            }
        }
        #endregion
        
        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                // Only allow access when machine is not in production mode
                if (m_AuthService.CheckAccess(ACL.Motion) && m_AuthService.CurrentUser.IsAuthenticated
                    && Global.MachineStatus != MachineStateType.Running && Global.MachineStatus != MachineStateType.Stopped && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.CriticalAlarm)
                {
                    MotorList[SelectedTabIndex].IsAllowAccess = true;
                    return true;
                }
                MotorList[SelectedTabIndex].IsAllowAccess = false;
                return false;
            }
        }

        public bool IsAllowAccess
        {
            get
            {
                if (m_AuthService.CheckAccess(ACL.Motion) && m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
