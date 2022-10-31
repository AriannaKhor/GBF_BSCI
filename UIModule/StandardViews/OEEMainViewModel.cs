using GreatechApp.Core;
using GreatechApp.Core.Events;
using GreatechApp.Services.UserServices;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class OEEMainViewModel : BaseUIViewModel, INavigationAware
    {
        IContainerProvider m_ContainerProvider;
        IRegion tabControlRegion;

        public OEEMainViewModel(IContainerProvider containerProvider)
        {
            m_ContainerProvider = containerProvider;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (tabControlRegion == null)
            {
                tabControlRegion = m_RegionManager.Regions[RegionNames.OEETabControlRegion];

                m_RegionManager.RequestNavigate(RegionNames.OEETabControlRegion, "OEELiveView");
                m_RegionManager.RequestNavigate(RegionNames.OEETabControlRegion, "OEEAnalysisView");
                m_RegionManager.RequestNavigate(RegionNames.OEETabControlRegion, "OEELiveView");
            }
        }
    }
}
