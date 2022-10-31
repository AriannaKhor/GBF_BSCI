using GreatechApp.Core;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Enums;
using GreatechApp.Core.Events;
using GreatechApp.Core.Variable;
using GreatechApp.Services.UserServices;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using UIModule.MainPanel;
using UIModule.RecipeViews;

namespace UIModule.RecipeViews
{
    public class RecipeViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable
        private string m_Title;
        public string Title
        {
            get { return m_Title; }
            set { SetProperty(ref m_Title, value); }
        }

        IContainerProvider m_ContainerProvider;
        IRegion tabControlRegion;
        #endregion

        public RecipeViewModel(IContainerProvider containerProvider)
        {
            m_ContainerProvider = containerProvider;

            m_EventAggregator.GetEvent<MachineState>().Subscribe(OnMachineState);

            Title = GetStringTableValue("Recipe");
        }

        #region Event
        private void OnMachineState(MachineStateType status)
        {
            Global.MachineStatus = status;
            RaisePropertyChanged(nameof(CanAccess));
        }

        public override void OnCultureChanged()
        {
            Title = GetStringTableValue("Recipe");
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
            navigationContext.NavigationService.Region.RegionManager.Regions.Remove(RegionNames.RecipeTabControlRegion);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (tabControlRegion == null)
            {
                tabControlRegion = m_RegionManager.Regions[RegionNames.RecipeTabControlRegion];

                tabControlRegion.Add(m_ContainerProvider.Resolve<ProductRecipeView>());
            }

            if (Global.InitDone && (Global.MachineStatus == MachineStateType.Lot_Ended || Global.MachineStatus == MachineStateType.Ready || Global.MachineStatus == MachineStateType.Init_Done))
            {
                m_EventAggregator.GetEvent<MachineState>().Publish(MachineStateType.ReInit);
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
