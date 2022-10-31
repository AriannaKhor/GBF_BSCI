namespace UIModule.StandardViews
{
    using GreatechApp.Core;
    using GreatechApp.Core.Cultures;
    using GreatechApp.Core.Enums;
    using GreatechApp.Core.Events;
    using GreatechApp.Services.UserServices;
    using Prism.Events;
    using Prism.Ioc;
    using Prism.Mvvm;
    using Prism.Regions;
    using UIModule.MainPanel;
    using UIModule.StandardViews;

    public class SettingViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable
        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        IContainerProvider m_ContainerProvider;
        IRegion tabControlRegion;
        #endregion

        #region Constructor
        public SettingViewModel(IContainerProvider containerProvider)
        {
            m_ContainerProvider = containerProvider;

            Title = GetStringTableValue("Setting");
        }
        #endregion

        #region Event
        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("Setting");
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
            navigationContext.NavigationService.Region.RegionManager.Regions.Remove(RegionNames.SettingTabControlRegion);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (tabControlRegion == null)
            {
                tabControlRegion = m_RegionManager.Regions[RegionNames.SettingTabControlRegion];

                tabControlRegion.Add(m_ContainerProvider.Resolve<SeqParameterView>());
                tabControlRegion.Add(m_ContainerProvider.Resolve<OEESettingView>());
                tabControlRegion.Add(m_ContainerProvider.Resolve<TowerLightView>());
                tabControlRegion.Add(m_ContainerProvider.Resolve<SecsGemSettingsView>());
            }
        }
        #endregion

        #region Access Implementation
        public override bool CanAccess
        {
            get
            {
                if (m_AuthService.CheckAccess(ACL.Setting) && m_AuthService.CurrentUser.IsAuthenticated)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
