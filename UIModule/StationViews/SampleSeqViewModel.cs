using GreatechApp.Core.Enums;
using GreatechApp.Core.Modal;
using Prism.Commands;
using System.Collections.Generic;
using System.Windows;

namespace UIModule.StationViews
{
    public class SampleSeqViewModel : BaseSeqViewModel
    {
        #region Variables
        #region Preset
        public DelegateCommand<string> PresetCommand { get; private set; }

        private List<OUT> m_Task1Output;

        public List<OUT> Task1Output
        {
            get { return m_Task1Output; }
            set { SetProperty(ref m_Task1Output, value); }
        }
        private List<OUT> m_Task2Output;

        public List<OUT> Task2Output
        {
            get { return m_Task2Output; }
            set { SetProperty(ref m_Task2Output, value); }
        }

        #endregion

        #endregion

        #region Constructor
        public SampleSeqViewModel()
        {
            Title = "Sample Seq";
            CurrentSeq = SQID.TopVisionSeq;
            GetConfig(CurrentSeq);

            Init();

            // Set to visible if preset is in use
            PresetVis = Visibility.Visible;

            #region Preset
            PresetCommand = new DelegateCommand<string>(OnPresetCommand);

            Task1Output = new List<OUT>() { OUT.DO0100_RedTowerLight, OUT.DO0101_AmberTowerLight, OUT.DO0102_GreenTowerLight, OUT.DO0103_Buzzer };
            Task2Output = new List<OUT>() { OUT.DO0100_RedTowerLight, OUT.DO0101_AmberTowerLight };
            #endregion
        }
        #endregion

        

        #region Method

        #region Preset
        private void OnPresetCommand(string command)
        {

            if (command == "VacuumOn")
            {
                foreach (VacuumIOParameters item in VacuumList)
                    WriteBit(item.Vacuum, true);
            }
            else if (command == "VacuumOff")
            {
                foreach (VacuumIOParameters item in VacuumList)
                    WriteBit(item.Vacuum, false);

            }
            else if (command == "PurgeOn")
            {
                foreach (VacuumIOParameters item in VacuumList)
                    if(item.IsPurgeAvail)
                        WriteBit(item.Purge.Value, true);

            }
            else if (command == "PurgeOff")
            {
                foreach (VacuumIOParameters item in VacuumList)
                    if(item.IsPurgeAvail)
                        WriteBit(item.Purge.Value, false);
            }
            else if (command == "Task1")
            {
                foreach (OUT output in Task1Output)
                    WriteBit(output, true);
            }
            else if (command == "Task2")
            {
                foreach (OUT output in Task1Output)
                    WriteBit(output, true);
            }
        }
        #endregion

        #endregion
    }
}
