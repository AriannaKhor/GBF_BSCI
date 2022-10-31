using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using ProtoBuf;
using Sequence;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using UIModule.DataMarkers.DiagramDesigner;
using UIModule.DataMarkers.Interfaces;

namespace UIModule.DataMarkers
{
    [ProtoContract(Name = "BaseDataMarkerVM"), ProtoInclude(11, typeof(CircularDataMarkerViewModel)), ProtoInclude(12, typeof(LinearDataMarkerViewModel)), ProtoInclude(13, typeof(TrayDataMarkerViewModel))]
    public class BaseDataMarkerViewModel : BindableBase, IDesignerItem, IMachineData
    {
        public IDelegateSeq m_DelegateSeq;
        private ObservableCollection<BaseMarkerSlotViewModel> m_DataMarkers;
        private double x, y;
        private bool m_IsSelected = false;
        private string m_Title;
        protected double m_Width, m_Height, m_Offset;
        protected bool m_IsSimple;
        protected DispatcherTimer tmrDataMarker;
        internal UnitFlowDir m_UnitFlowDir;
        private IEventAggregator m_EventAggregator;
        private CultureResources m_CultureResources;

        public DelegateCommand<string> MarkerCommand { get; set; }
        public DelegateCommand ToggledCommand { get; set; }
        public DelegateCommand ReAdjustCommand { get; set; }
        public BaseDataMarkerViewModel()
        {
            m_EventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            m_DelegateSeq = ContainerLocator.Container.Resolve<IDelegateSeq>();
            m_CultureResources = ContainerLocator.Container.Resolve<CultureResources>();
            DataMarkers = new ObservableCollection<BaseMarkerSlotViewModel>();
            MarkerCommand = new DelegateCommand<string>(OnMarkerCommand);
            ToggledCommand = new DelegateCommand(OnToggledCommand);
            ReAdjustCommand = new DelegateCommand(OnReAdjustCommand);
            tmrDataMarker = new DispatcherTimer();
            tmrDataMarker.IsEnabled = false;
            tmrDataMarker.Tick += new EventHandler(tmrDataMarker_Tick);
            tmrDataMarker.Interval = new TimeSpan(0, 0, 0, 0, 300);

            m_EventAggregator.GetEvent<RefreshMarkerLayout>().Subscribe(OnReAdjustCommand);
        }

        private void OnToggledCommand()
        {
            m_EventAggregator.GetEvent<RecordDesignerItem>().Publish();
        }

        public void OnReAdjustCommand()
        {
            RotateCW();
            RotateCCW();
        }

        public void OnBlinkComplete()
        {
            IsBlinking = false;
        }

        private void ChangeSimpleMode()
        {
            foreach (BaseMarkerSlotViewModel slot in DataMarkers)
            {
                slot.SimpleVisibility = IsSimple ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void OnMarkerCommand(string markCmd)
        {
            switch(markCmd)
            {
                case "RotateCW":
                    RotateCW();
                    break;

                case "RotateCCW":
                    RotateCCW();
                    break;

                case "0":
                    SetMarkerAngle(0);
                    break;

                case "90":
                    SetMarkerAngle(90);
                    break;

                case "180":
                    SetMarkerAngle(180);
                    break;

                case "270":
                    SetMarkerAngle(270);
                    break;
            }
        }

        protected void tmrDataMarker_Tick(object sender, EventArgs e)
        {
            try
            {
                // Update Marker Data
                foreach(var marker in DataMarkers)
                {
                    marker.RefreshMarkerData();
                }
            }
            catch (Exception ex)
            {
                tmrDataMarker.Stop();
                m_EventAggregator.GetEvent<DatalogEntity>().Publish(new DatalogEntity() { MsgType = LogMsgType.Error, MsgText = $"{m_CultureResources.GetStringValue("DataMarker")} : {Title}, {m_CultureResources.GetStringValue("Sequence")} : {SeqName}, {m_CultureResources.GetStringValue("Message")} : {ex.Message}" });
            }
        }

        protected void ConfigureMarker(string title, SQID seqName)
        {
            Title = title;
            SeqName = seqName;
            NumOfMarker = m_DelegateSeq.GetSlotNum(seqName);
            NumOfStation = m_DelegateSeq.GetStationNum(seqName);
        }

        public ObservableCollection<BaseMarkerSlotViewModel> DataMarkers
        {
            get { return m_DataMarkers; }
            set { SetProperty(ref m_DataMarkers, value); }
        }

        public void RotateCW()
        {
            Offset += 0.1;
            m_EventAggregator.GetEvent<RecordDesignerItem>().Publish();
        }

        public void RotateCCW()
        {
            Offset -= 0.1;
            m_EventAggregator.GetEvent<RecordDesignerItem>().Publish();
        }

        public void SetMarkerAngle(double degree)
        {
            // change from degree to radians.
            Offset = degree * Math.PI / 180.0;
            m_EventAggregator.GetEvent<RecordDesignerItem>().Publish();
        }

        #region IDesignerItem Interface Implementation
        [ProtoMember(1)]
        public double X
        {
            get { return x; }
            set { SetProperty(ref x, value); }
        }

        [ProtoMember(2)]
        public double Y
        {
            get { return y; }
            set { SetProperty(ref y, value); }
        }

        [ProtoMember(3)]
        public virtual double Width
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public virtual double Height
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public virtual double Offset
        {
            get;
            set;
        }

        public double MinWidth
        {
            get;
            set;
        }

        public double MinHeight
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get { return m_IsSelected; }
            set 
            { 
                SetProperty(ref m_IsSelected, value);
                if (!value)
                    IsExpended = false;
            }
        }

        private bool m_IsExpended;
        public bool IsExpended
        {
            get { return m_IsExpended; }
            set { SetProperty(ref m_IsExpended, value); }
        }

        private bool m_CanSelect;
        public bool CanSelect
        {
            get { return m_CanSelect; }
            set 
            { 
                SetProperty(ref m_CanSelect, value);
                if(!value)
                    IsSelected = value;
            }
        }

        private string m_NavigateView;
        public string NavigateView
        {
            get { return m_NavigateView; }
            set { SetProperty(ref m_NavigateView, value); }
        }

        #endregion

        public MarkerType DataMarkerType { get; set; }

        [ProtoMember(6)]
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }


        [ProtoMember(7)]
        public bool IsSimple
        {
            get { return m_IsSimple; }
            set 
            { 
                SetProperty(ref m_IsSimple, value); 
                ChangeSimpleMode();
            }
        }

        public int NumOfMarker { get; set; }

        public int NumOfStation { get; set; }

        public SQID SeqName { get; set; }

        public bool IsScanStarted 
        {
            set { tmrDataMarker.IsEnabled = value; }
        }
        private bool m_IsBlinking;
        public bool IsBlinking 
        {
            get { return m_IsBlinking; }
            set
            {
                SetProperty(ref m_IsBlinking, value);
            }
        }
    }
}
