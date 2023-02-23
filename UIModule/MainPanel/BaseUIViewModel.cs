using GreatechApp.Services.UserServices;
using Prism.Mvvm;
using GreatechApp.Core.Events;
using Prism.Events;
using Sequence;
using Prism.Ioc;
using ConfigManager;
using Prism.Regions;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Interface;
using TCPIPManager;
using IOManager;

namespace UIModule.MainPanel
{
    public class BaseUIViewModel : BindableBase, IAccessService
    {
        #region Variable
        public IRegionManager m_RegionManager;
        public IEventAggregator m_EventAggregator;
        public IDelegateSeq m_DelegateSeq;
        public ISQLOperation m_SQLOperation;
        public ITCPIP m_TCPIP;
        public IInsightVision m_InsightVision;
        public IShowDialog m_ShowDialog;
        public IBaseIO m_IO;
        public IUser m_CurrentUser;
        public AuthService m_AuthService;
        public SystemConfig m_SystemConfig;
        public CultureResources m_CultureResources;
        #endregion

        #region Constructor
        public BaseUIViewModel()
        {
            m_RegionManager = (IRegionManager)ContainerLocator.Container.Resolve(typeof(IRegionManager));
            m_SQLOperation = (ISQLOperation)ContainerLocator.Container.Resolve(typeof(ISQLOperation));
            m_EventAggregator = (IEventAggregator)ContainerLocator.Container.Resolve(typeof(IEventAggregator));
            m_DelegateSeq = (IDelegateSeq)ContainerLocator.Container.Resolve(typeof(IDelegateSeq));
            m_TCPIP = (ITCPIP)ContainerLocator.Container.Resolve(typeof(ITCPIP));
            m_InsightVision = (IInsightVision)ContainerLocator.Container.Resolve(typeof(IInsightVision));
            m_IO = (IBaseIO)ContainerLocator.Container.Resolve(typeof(IBaseIO));
            m_ShowDialog = (IShowDialog)ContainerLocator.Container.Resolve(typeof(IShowDialog));
            m_CurrentUser = (DefaultUser)ContainerLocator.Container.Resolve(typeof(DefaultUser));
            m_CultureResources = (CultureResources)ContainerLocator.Container.Resolve(typeof(CultureResources));
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));
            m_AuthService = (AuthService)ContainerLocator.Container.Resolve(typeof(AuthService));

            m_EventAggregator.GetEvent<ValidateLogin>().Subscribe(OnValidateLogin);
            m_EventAggregator.GetEvent<CultureChanged>().Subscribe(OnCultureChanged);

            RaisePropertyChanged(nameof(CanAccess));
        }
        #endregion

        #region Event
        public virtual void OnValidateLogin(bool IsAuthenticated)
        {
            RaisePropertyChanged(nameof(CanAccess));
        }

        public virtual void OnCultureChanged()
        {

        }
        #endregion

        #region Access Implementation
        public IUser CurrentUser { get; }

        public virtual bool CanAccess
        {
            get
            {
                if (m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Method
        public string GetStringTableValue(string key)
        {
            return m_CultureResources.GetStringValue(key);
        }
        public string GetDialogTableValue(string key)
        {
            return m_CultureResources.GetDialogValue(key);
        }
        #endregion
    }
}
