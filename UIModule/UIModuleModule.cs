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
            regionManager.RegisterViewWithRegion(RegionNames.CenterContentRegion, typeof(MachinePerformanceView));
            regionManager.RegisterViewWithRegion(RegionNames.TopContentRegion, typeof(Toolbar));

            //Compact View Region
            regionManager.RegisterViewWithRegion(RegionNames.CompactRegion, typeof(CompactView));
            regionManager.RegisterViewWithRegion(RegionNames.CompactRegion, typeof(MachinePerformanceView));

            if(!m_SystemConfig.General.IsCompactView)
            {
                //Home TabItem Region
                IRegion tabControlRegion = regionManager.Regions[RegionNames.HomeTabControlRegion];
                tabControlRegion.Add(containerProvider.Resolve<EquipmentView>());
            }

            // Lot Entry Region
            IRegion lotentryRegion = regionManager.Regions[RegionNames.LotEntryRegion];
            lotentryRegion.Add(containerProvider.Resolve<LotEntryView>());
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
            containerRegistry.RegisterForNavigation<LifeCycleView>();
            containerRegistry.RegisterForNavigation<OEEMainView>();
            containerRegistry.RegisterForNavigation<OEELiveView>();
            containerRegistry.RegisterForNavigation<OEEAnalysisView>();
            containerRegistry.RegisterForNavigation<OEESettingView>();
            containerRegistry.RegisterForNavigation<ErrorAnalysisView>();
            containerRegistry.RegisterForNavigation<MachinePerformanceView>();
            containerRegistry.RegisterForNavigation<ModulePerformanceView>();
            containerRegistry.RegisterForNavigation<SeqParameterView>();
            containerRegistry.RegisterForNavigation<TowerLightView>();
            containerRegistry.RegisterForNavigation<IOView>();
            containerRegistry.RegisterForNavigation<MotorView>();
            containerRegistry.RegisterForNavigation<TCPIPView>();
            containerRegistry.RegisterForNavigation<SerialPortView>();
            containerRegistry.RegisterForNavigation<SecsGemSettingsView>();

            //Dialog Views
            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
            containerRegistry.RegisterDialog<MessageView, MessageViewModel>();
            containerRegistry.RegisterDialog<ErrMessageView, ErrMessageViewModel>();

            // Recipe Views
            containerRegistry.RegisterForNavigation<RecipeView>();

            // Module View
            containerRegistry.RegisterForNavigation<SampleSeqView>();

        }
    }
}