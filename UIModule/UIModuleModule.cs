using GreatechApp.Core;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UIModule.MainPanel;
using UIModule.StandardViews;
using UIModule.RecipeViews;
using UIModule.StationViews;
using DialogManager.Login;
using DialogManager.Message;
using DialogManager.ErrorMsg;
using ConfigManager;

namespace UIModule
{
    public class UIModuleModule : IModule
    {
        public SystemConfig m_SystemConfig;
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            m_SystemConfig = (SystemConfig)ContainerLocator.Container.Resolve(typeof(SystemConfig));

            regionManager.RegisterViewWithRegion(RegionNames.CenterContentRegion, typeof(Home));
            regionManager.RegisterViewWithRegion(RegionNames.TopContentRegion, typeof(Toolbar));

            //Compact View Region
            regionManager.RegisterViewWithRegion(RegionNames.CompactRegion, typeof(CompactView));

            IRegion homeRegion = regionManager.Regions[RegionNames.HomeTabControlRegion];
            homeRegion.Add(containerProvider.Resolve<EquipmentView>(), "EquipmentView");
            homeRegion.Add(containerProvider.Resolve<OperatorView>(), "OperatorView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // MainPanel
            containerRegistry.RegisterForNavigation<Home>();

            // Standard Views
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<AlarmView>();
            containerRegistry.RegisterForNavigation<UserView>();
            containerRegistry.RegisterForNavigation<LotHistoryView>();
            containerRegistry.RegisterForNavigation<SettingView>();
            containerRegistry.RegisterForNavigation<ErrorAnalysisView>();
            containerRegistry.RegisterForNavigation<SeqParameterView>();
            containerRegistry.RegisterForNavigation<TowerLightView>();
            containerRegistry.RegisterForNavigation<IOView>();
            containerRegistry.RegisterForNavigation<TCPIPView>();

            //Dialog Views
            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
            containerRegistry.RegisterDialog<MessageView, MessageViewModel>();
            containerRegistry.RegisterDialog<ErrMessageView, ErrMessageViewModel>();
            containerRegistry.RegisterDialog<ErrVerificationView, ErrVerificationViewModel>();
            containerRegistry.RegisterDialog<ForcedEndLotView, ForcedEndLotViewModel>();

            // Recipe Views
            containerRegistry.RegisterForNavigation<RecipeView>();

        }
    }
}