using ConfigManager;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Modal;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using InterlockManager.IO;
using IOManager;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Timers;
using System.Windows;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class IOViewModel : BaseUIViewModel, IAccessService, INavigationAware
    {
        #region Variable
        private string _title = "IO";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private ObservableCollection<string> m_ModuleStations;
        public ObservableCollection<string> ModuleStations
        {
            get { return m_ModuleStations; }
            set { SetProperty(ref m_ModuleStations, value); }
        }

        private string m_SelectedModuleStation;
        public string SelectedModuleStation
        {
            get { return m_SelectedModuleStation; }
            set { SetProperty(ref m_SelectedModuleStation, value); }
        }

        private ObservableCollection<IOList> _InputList;
        public ObservableCollection<IOList> InputList
        {
            get { return _InputList; }
            set { SetProperty(ref _InputList, value); }
        }

        private ObservableCollection<IOList> _OutputList;
        public ObservableCollection<IOList> OutputList
        {
            get { return _OutputList; }
            set { SetProperty(ref _OutputList, value); }
        }


        private List<DictionaryEntry> m_InputResources { get; set; }
        private List<DictionaryEntry> m_OutputResources { get; set; }

        public DelegateCommand<IOList> IOCommand { get; private set; }


        private IEnumerable<IIOInterlock> m_IOIntLCollection;
        private IIOInterlock m_IOIntL;
        private Timer tmr_UpdateStatus;

        public DelegateCommand SelectedItemChanged { get; set; }
        #endregion

        #region Constructor

        public IOViewModel()
        {
            m_IOIntLCollection = ContainerLocator.Container.Resolve<Func<IEnumerable<IIOInterlock>>>()();
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            RaisePropertyChanged(nameof(CanAccess));

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);

            LoadResources();

            var MachineSeqList = Enum.GetNames(typeof(SQID)).Where(x => !x.Contains("CriticalScan") && !x.Contains("None"));

            ModuleStations = new ObservableCollection<string>() { "ALL" };
            foreach (var module in MachineSeqList)
                ModuleStations.Add(module);

            IOCommand = new DelegateCommand<IOList>(Command);
            SelectedItemChanged = new DelegateCommand(OnSelectedItemChanged);

            tmr_UpdateStatus = new Timer();
            tmr_UpdateStatus.Interval = m_SystemConfig.General.IOScanRate;
            tmr_UpdateStatus.Elapsed += UpdateIOList;

            SelectedModuleStation = "ALL";
            OnSelectedItemChanged();
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
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            tmr_UpdateStatus.Stop();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            tmr_UpdateStatus.Start();
            UpdateIOList(this, null);
            if (Global.InitDone && (Global.MachineStatus == MachineStateType.Lot_Ended || Global.MachineStatus == MachineStateType.Ready || Global.MachineStatus == MachineStateType.Init_Done))
            {
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.ReInit);
            }
        }

        #endregion

        #region Event
        private void OnMachineState(MachineStateType status)
        {
            Global.MachineStatus = status;
            RaisePropertyChanged(nameof(CanAccess));
        }

        public override void OnCultureChanged()
        {
            LoadResources();

            foreach (IOList io in InputList)
            {
                io.Assignment = (string)m_InputResources[io.Tag].Value;
                io.Description = (string)m_InputResources[io.Tag].Value;
            }

            foreach (IOList io in OutputList)
            {
                io.Assignment = (string)m_OutputResources[io.Tag].Value;
                io.Description = (string)m_OutputResources[io.Tag].Value;
            }
        }
        #endregion

        #region Method
        private void LoadResources()
        {
            ResourceManager inputResources = new ResourceManager(typeof(InputTable));
            ResourceSet inputResourceSet = inputResources.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            m_InputResources = new List<DictionaryEntry>();
            foreach (DictionaryEntry entry in inputResourceSet)
                m_InputResources.Add(entry);
            m_InputResources = m_InputResources.OrderBy(x => x.Key).Select(x => x).ToList();

            ResourceManager outputResources = new ResourceManager(typeof(OutputTable));
            ResourceSet outputResourceSet = outputResources.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            m_OutputResources = new List<DictionaryEntry>();
            foreach (DictionaryEntry entry in outputResourceSet)
                m_OutputResources.Add(entry);
            m_OutputResources = m_OutputResources.OrderBy(x => x.Key).Select(x => x).ToList();
        }

        private void Command(IOList ioParam)
        {
            m_IOIntL = m_IOIntLCollection.FirstOrDefault();

            string seqName = m_IOIntL.GetSeqName((OUT)ioParam.Tag);

            m_IOIntL = m_IOIntLCollection.FirstOrDefault(x => x.Provider == EnumHelper.GetValueFromDescription<SQID>(seqName));

            if (Global.MachineStatus != MachineStateType.Running && Global.MachineStatus != MachineStateType.Initializing)
            {
                m_IO.WriteBit(ioParam.Tag, ioParam.Status);
            }
        }

        private void UpdateIOList(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (InputList != null && OutputList != null)
                {
                    foreach (IOList io in InputList)
                    {
                        io.Status = m_IO.ReadBit(io.Tag);
                    }

                    foreach (IOList io in OutputList)
                    {
                        io.Status = m_IO.ReadOutBit(io.Tag);
                    }
                }
            }
            catch (Exception ex)
            {
                tmr_UpdateStatus.Stop();
                MessageBox.Show(ex.Message, "IO Error");
            }
        }

        private void OnSelectedItemChanged()
        {
            tmr_UpdateStatus.Stop();
            DisplayIO();
            tmr_UpdateStatus.Start();
        }

        private void DisplayIO()
        {
            try
            {
                InputList = new ObservableCollection<IOList>();
                OutputList = new ObservableCollection<IOList>();

                if (SelectedModuleStation == "ALL")
                {
                    //foreach (IN input in Enum.GetValues(typeof(IN)))
                    //{
                    //    InputList.Add(new IOList()
                    //    {
                    //        Assignment = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
                    //        Description = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
                    //        Tag = (int)input,
                    //        Status = m_IO.ReadBit((int)input),
                    //    });
                    //}

                    foreach (OUT output in Enum.GetValues(typeof(OUT)))
                    {
                        OutputList.Add(new IOList()
                        {
                            Assignment = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
                            Description = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
                            Tag = (int)output,
                            Status = m_IO.ReadBit((int)output),
                        });
                    }
                }
                else
                {
                    //if (m_IO.InputMapList.Where(x => x.Key == (SQID)Enum.Parse(typeof(SQID), SelectedModuleStation)).Count() > 0)
                    //{
                    //    var inputs = m_IO.InputMapList.Where(x => x.Key == (SQID)Enum.Parse(typeof(SQID), SelectedModuleStation)).First().Value;

                    //    foreach (object inputObj in inputs)
                    //    {
                    //        IN input = (IN)Enum.Parse(typeof(IN), inputObj.ToString());

                    //        InputList.Add(new IOList()
                    //        {
                    //            Assignment = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
                    //            Description = (string)m_InputResources.Where(x => x.Key.ToString() == input.ToString()).FirstOrDefault().Value,
                    //            Tag = (int)input,
                    //            Status = m_IO.ReadBit((int)input),
                    //        });
                    //    }
                    //}

                    if (m_IO.OutputMapList.Where(x => x.Key == (SQID)Enum.Parse(typeof(SQID), SelectedModuleStation)).Count() > 0)
                    {
                        var outputs = m_IO.OutputMapList.Where(x => x.Key == (SQID)Enum.Parse(typeof(SQID), SelectedModuleStation)).First().Value;

                        foreach (object outputObj in outputs)
                        {
                            OUT output = (OUT)Enum.Parse(typeof(OUT), outputObj.ToString());

                            OutputList.Add(new IOList()
                            {
                                Assignment = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
                                Description = (string)m_OutputResources.Where(x => x.Key.ToString() == output.ToString()).FirstOrDefault().Value,
                                Tag = (int)output,
                                Status = m_IO.ReadOutBit((int)output),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "IO Mapping Error");
            }
        }
        #endregion

        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CheckAccess(ACL.IO) && m_AuthService.CurrentUser.IsAuthenticated
                    && Global.MachineStatus != MachineStateType.Running && Global.MachineStatus != MachineStateType.Initializing && Global.MachineStatus != MachineStateType.CriticalAlarm)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }

}
