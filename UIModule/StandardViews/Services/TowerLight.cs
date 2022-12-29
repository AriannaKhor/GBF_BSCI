using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using IOManager;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Timers;
using static ConfigManager.TowerLightConfig;

namespace UIModule.StandardViews.Services
{
    public class TowerLight : BindableBase
    {
        #region Variable
        private readonly IEventAggregator m_EventAggregator;
        private readonly SystemConfig m_SysConfig;
        private readonly IBaseIO IO;
        private readonly CultureResources m_CultureResources;
        public TowerLightConfig TowerLightCfg;

        Timer LightTimer = null;
        Timer BuzzerTimer = null;

        private bool m_IsRedBlinking;
        public bool IsRedBlinking
        {
            get { return m_IsRedBlinking; }
            set { SetProperty(ref m_IsRedBlinking, value); }

        }

        private bool m_IsYellowBlinking;
        public bool IsYellowBlinking
        {
            get { return m_IsYellowBlinking; }
            set { SetProperty(ref m_IsYellowBlinking, value); }
        }

        private bool m_IsGreenBlinking;
        public bool IsGreenBlinking
        {
            get { return m_IsGreenBlinking; }
            set { SetProperty(ref m_IsGreenBlinking, value); }
        }

        private bool m_IsLightBlinking;
        public bool IsLightBlinking
        {
            get { return m_IsLightBlinking; }
            set { SetProperty(ref m_IsLightBlinking, value); }
        }

        private bool m_IsBuzzerBlinking;
        public bool IsBuzzerBlinking
        {
            get { return m_IsBuzzerBlinking; }
            set { SetProperty(ref m_IsBuzzerBlinking, value); }
        }
        #endregion

        #region Constructor
        public TowerLight(IEventAggregator eventAggregator, SystemConfig sysConfig, IBaseIO baseIO, CultureResources cultureResources)
        {
            m_EventAggregator = eventAggregator;
            m_SysConfig = sysConfig;
            IO = baseIO;
            m_CultureResources = cultureResources;

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineStateChange);
            TowerLightCfg = TowerLightConfig.Open(m_SysConfig.TowerLightRef[0].Reference);

            // Set light and buzzer timer properties 
            LightTimer = new Timer();
            BuzzerTimer = new Timer();
            LightTimer.Interval = TowerLightCfg.TimerInterval.LightTimerInterval;
            BuzzerTimer.Interval = TowerLightCfg.TimerInterval.BuzzerTimerInterval;
            LightTimer.Elapsed += LightTimer_Elapsed;
            BuzzerTimer.Elapsed += BuzzerTimer_Elapsed;
        }
        #endregion

        #region Event
        // Buzzer blinking timer
        private void BuzzerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (sender)
            {
                IO.WriteBit((int)OUT.DO0100_Buzzer, IsBuzzerBlinking);
                IsBuzzerBlinking = !IsBuzzerBlinking;
            }
        }

        // Light Blinking timer
        private void LightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (sender)
            {
                if (IsRedBlinking)
                    IO.WriteBit((int)OUT.DO0101_RedTowerLight, IsLightBlinking);
                if (IsYellowBlinking)
                    IO.WriteBit((int)OUT.DO0102_AmberTowerLight, IsLightBlinking);
                if (IsGreenBlinking)
                    IO.WriteBit((int)OUT.DO0103_GreenTowerLight, IsLightBlinking);

                IsLightBlinking = !IsLightBlinking;
            }
        }
        private void OnMachineStateChange(MachineStateType machineState)
        {
            lock (this)
            {
                try
                {
                    ResetBlinking();
                    TLSetting currentState = TowerLightCfg.Setting.Where(key => key.Name == machineState.ToString()).FirstOrDefault();

                    // Check RedState in TowerlightList and change red tower light
                    #region Red tower light
                    if (currentState.RedState == TowerLightState.ON)
                        IO.WriteBit((int)OUT.DO0101_RedTowerLight, true);

                    else if (currentState.RedState == TowerLightState.Blinking)
                        IsRedBlinking = true;

                    else if (currentState.RedState == TowerLightState.OFF)
                        IO.WriteBit((int)OUT.DO0101_RedTowerLight, false);
                    #endregion

                    // Check YellowState in TowerlightList and change yellow tower light
                    #region Yellow tower light
                    if (currentState.YellowState == TowerLightState.ON)
                        IO.WriteBit((int)OUT.DO0102_AmberTowerLight, true);

                    else if (currentState.YellowState == TowerLightState.Blinking)
                        IsYellowBlinking = true;

                    else if (currentState.YellowState == TowerLightState.OFF)
                        IO.WriteBit((int)OUT.DO0102_AmberTowerLight, false);
                    #endregion

                    // Check GreenState in TowerlightList and change green tower light
                    #region Green tower light
                    if (currentState.GreenState == TowerLightState.ON)
                        IO.WriteBit((int)OUT.DO0103_GreenTowerLight, true);

                    else if (currentState.GreenState == TowerLightState.Blinking)
                        IsGreenBlinking = true;

                    else if (currentState.GreenState == TowerLightState.OFF)
                        IO.WriteBit((int)OUT.DO0103_GreenTowerLight, false);
                    #endregion

                    // Check BuzzerState in TowerlightList and change buzzer tower light
                    #region Buzzer
                    // Buzzer have only blinking and off state
                    if (currentState.BuzzerState == TowerLightState.Blinking)
                    {
                        IsBuzzerBlinking = true;
                        BuzzerTimer.Start();
                    }
                    else if (currentState.BuzzerState == TowerLightState.OFF)
                        IO.WriteBit((int)OUT.DO0100_Buzzer, false);
                    #endregion

                    // if any tower light is blinking state, start the light timer
                    if (IsRedBlinking || IsYellowBlinking || IsGreenBlinking)
                    {
                        IsLightBlinking = true;
                        LightTimer.Start();
                    }
                }
                catch(Exception ex)
                {
                    BuzzerTimer.Stop();
                    LightTimer.Stop();
                    m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("TowerLight")} {m_CultureResources.GetStringValue("Error")} : {ex.Message}" });
                }
            }
        }
        #endregion

        #region Method
        // Reset flags before check Light State
        private void ResetBlinking()
        {
            // Stop timer when machine state is changed
            LightTimer.Stop();
            BuzzerTimer.Stop();
            LightTimer.Interval = TowerLightCfg.TimerInterval.LightTimerInterval;
            BuzzerTimer.Interval = TowerLightCfg.TimerInterval.BuzzerTimerInterval;

            // Reset flags
            IsBuzzerBlinking = false;
            IsRedBlinking = false;
            IsYellowBlinking = false;
            IsGreenBlinking = false;
            IsLightBlinking = false;

            // turn off before loadin the setting 
            IO.WriteBit((int)OUT.DO0101_RedTowerLight, false);
            IO.WriteBit((int)OUT.DO0102_AmberTowerLight, false);
            IO.WriteBit((int)OUT.DO0103_GreenTowerLight, false);
            IO.WriteBit((int)OUT.DO0100_Buzzer, false);
        }
        #endregion
    }
}
