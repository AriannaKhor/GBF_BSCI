namespace UIModule.StandardViews
{
    using ConfigManager;
    using GreatechApp.Core.Enums;
    using GreatechApp.Core.Events;
    using GreatechApp.Core.Interface;
    using GreatechApp.Core.Modal;
    using GreatechApp.Services.UserServices;
    using GreatechApp.Core.Resources;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Prism.Regions;
    using Prism.Services.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static ConfigManager.TowerLightConfig;
    using GreatechApp.Core.Cultures;
    using UIModule.MainPanel;

    public class TowerLightViewModel : BaseUIViewModel
    {
        #region Variable
        public TowerLightConfig TowerLightCfg;
        public DelegateCommand SaveCommand { get; private set; }

        private string m_TabPageHeader;
        public string TabPageHeader
        {
            get { return m_TabPageHeader; }
            set { SetProperty(ref m_TabPageHeader, value); }
        }

        private List<Towerlightlist> m_towerlightlist;
        public List<Towerlightlist> TowerlightList
        {
            get { return m_towerlightlist; }
            set { SetProperty(ref m_towerlightlist, value); }
        }

        private int m_LightTimerInt;
        public int LightTimerInt
        {
            get { return m_LightTimerInt; }
            set { SetProperty(ref m_LightTimerInt, value); }
        }

        private int m_BuzzerTimerInt;
        public int BuzzerTimerInt
        {
            get { return m_BuzzerTimerInt; }
            set { SetProperty(ref m_BuzzerTimerInt, value); }
        }
        #endregion

        #region Constructor
        public TowerLightViewModel()
        {
            TowerlightList = new List<Towerlightlist>();
            SaveCommand = new DelegateCommand(SaveSetting);
            TabPageHeader = GetStringTableValue("TLSetting");

            #region Mahine State List 
            // Add Machine State into list
            foreach (MachineStateType mstate in (MachineStateType[])Enum.GetValues(typeof(MachineStateType)))
            {
                TowerlightList.Add(new Towerlightlist()
                {
                    Name = mstate.ToString(),
                    Tag = mstate,
                });
            }
            TowerlightList = TowerlightList.OrderBy(key => key.Name).ToList();
            TowerLightCfg = TowerLightConfig.Open(m_SystemConfig.TowerLightRef[0].Reference);
            foreach (TLSetting item in TowerLightCfg.Setting)
            {
                int index = TowerlightList.IndexOf(TowerlightList.FirstOrDefault(key => key.Name == item.Name));

                #region Red tower light
                if (item.RedState == TowerLightState.ON)
                    TowerlightList[index].RSteady = true;
                else if (item.RedState == TowerLightState.Blinking)
                    TowerlightList[index].RBlinking = true;
                else if (item.RedState == TowerLightState.OFF)
                    TowerlightList[index].ROff = true;
                #endregion

                #region Yellow tower light
                if (item.YellowState == TowerLightState.ON)
                    TowerlightList[index].YSteady = true;
                else if (item.YellowState == TowerLightState.Blinking)
                    TowerlightList[index].YBlinking = true;
                else if (item.YellowState == TowerLightState.OFF)
                    TowerlightList[index].YOff = true;
                #endregion

                #region Green tower light
                if (item.GreenState == TowerLightState.ON)
                    TowerlightList[index].GSteady = true;
                else if (item.GreenState == TowerLightState.Blinking)
                    TowerlightList[index].GBlinking = true;
                else if (item.GreenState == TowerLightState.OFF)
                    TowerlightList[index].GOff = true;
                #endregion

                #region Buzzer
                // Buzzer have only blinking and off state
                if (item.BuzzerState == TowerLightState.Blinking)
                    TowerlightList[index].BuzBlinking = true;
                else if (item.BuzzerState == TowerLightState.OFF)
                    TowerlightList[index].BuzOff = true;
                #endregion
            }
            #endregion

            LightTimerInt = TowerLightCfg.TimerInterval.LightTimerInterval;
            BuzzerTimerInt = TowerLightCfg.TimerInterval.BuzzerTimerInterval;

            OnNavigatedTo(null);
        }

        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            TabPageHeader = GetStringTableValue("TLSetting");
        }
        #endregion

        #region Method
        // Save Setting
        private void SaveSetting()
        {
            ButtonResult buttonResult = m_ShowDialog.Show(DialogIcon.Question, GetDialogTableValue("AskConfirmUpdateTLSetting"));

            if (buttonResult == ButtonResult.Yes)
            {
                foreach (Towerlightlist item in TowerlightList)
                {
                    // Check Config file to identify is machine state existing
                    if (TowerLightCfg.Setting.Where(key => key.Name == item.Name).Count() > 0)
                    {
                        int tLSettingID = TowerLightCfg.Setting.FirstOrDefault(key => key.Name == item.Name).ID;
                        TowerLightCfg.Setting[tLSettingID].RedState = item.RedState;
                        TowerLightCfg.Setting[tLSettingID].YellowState = item.YellowState;
                        TowerLightCfg.Setting[tLSettingID].GreenState = item.GreenState;
                        TowerLightCfg.Setting[tLSettingID].BuzzerState = item.BuzzerState;
                    }
                    // Add new item into config file when the machine state is not exist
                    else
                    {
                        TLSetting newTLSetting = new TLSetting()
                        {
                            ID = TowerLightCfg.Setting.Count,
                            Name = item.Name,
                            RedState = item.RedState,
                            YellowState = item.YellowState,
                            GreenState = item.GreenState,
                            BuzzerState = item.BuzzerState
                        };
                        TowerLightCfg.Setting.Add(newTLSetting);
                    }
                }

                TowerLightCfg.TimerInterval.LightTimerInterval = LightTimerInt;
                TowerLightCfg.TimerInterval.BuzzerTimerInterval = BuzzerTimerInt;

                // Save setting into config file
                TowerLightConfig.Save(TowerLightCfg);
                m_ShowDialog.Show(DialogIcon.Complete, GetDialogTableValue("SettingSaved"));
            }
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }
        #endregion
    }
}
