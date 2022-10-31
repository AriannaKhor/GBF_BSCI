using TCPIPManager.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using GreatechApp.Core.Interface;

namespace TCPIPManager
{
    public class TCPIPManagerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
 
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TCPIPView>();


        }
    }
}