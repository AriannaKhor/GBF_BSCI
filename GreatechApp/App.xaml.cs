using ConfigManager;
using DialogManager;
using GreatechApp.CompactView;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Interface;
using GreatechApp.Services.UserServices;
using GreatechApp.Services.Utilities;
using GreatechApp.Shell;
using GreatechApp.SplashScreen;
using InterlockManager.IO;
using IOManager;
using Prism.Ioc;
using Prism.Modularity;
using Sequence;
using SerialPortManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using TCPIPManager;
using UIModule.StandardViews.Services;
using Unity;

namespace GreatechApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void Initialize()
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Another {Application.ResourceAssembly.GetName().Name} Application already launched ! ", "System", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                });

                Application.Current.Shutdown();
            }
            else
            {
                try
                {
                    //DeleteAllOEETempFiles();

                    //OEEConfig oeecfg = OEEConfig.Open(@"..\Config Section\OEE\OEE.Config");

                    base.Initialize();
                }
                catch (Exception ex)
                {
                    if (ReplaceWithTempFile())
                    {
                        base.Initialize();
                    }
                    //else
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        MessageBox.Show($"{ex.Message}", "OEE Config fail to load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //    });

                    //    Application.Current.Shutdown();
                    //}
                }
            }
        }

        private bool ReplaceWithTempFile()
        {
            if (File.Exists(@"..\Config Section\OEE\OEE.Config"))
            {
                File.Delete(@"..\Config Section\OEE\OEE.Config");
            }

            if (File.Exists(@"..\Config Section\OEE\OEE_Temp.Config"))
            {
                File.Move(@"..\Config Section\OEE\OEE_Temp.Config", @"..\Config Section\OEE\OEE.Config");
                return true;
            }
            else
            {
                File.Copy(@"..\Config Section\OEE\OEE_Backup.Config", @"..\Config Section\OEE\OEE.Config");
                return true;
            }
        }

        private void DeleteAllOEETempFiles()
        {
            string[] filePaths = Directory.GetFiles(@"..\Config Section\OEE\");
            foreach (string filePath in filePaths)
            {
                var name = new FileInfo(filePath).Name;
                if (name != "OEE.Config" && name != "OEE_Temp.Config" && name != "OEE_Backup.Config")
                {
                    File.Delete(filePath);
                }
            }
        }

        protected override Window CreateShell()
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var myview = Container.Resolve<SplashScreenView>();
            myview.ShowDialog();

            SystemConfig config = SystemConfig.Open(@"..\Config Section\General\System.Config");
            if (config.General.IsCompactView)
                return Container.Resolve<CompactShellView>();
            else
                return Container.Resolve<ShellView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterManySingleton(typeof(AuthService),
                                                    typeof(IAuthService),
                                                    typeof(IAccessService));

            containerRegistry.RegisterSingleton(typeof(DefaultUser));
            containerRegistry.RegisterSingleton(typeof(BaseIO));
            containerRegistry.RegisterSingleton(typeof(TCPIPBase));
            containerRegistry.RegisterSingleton(typeof(InSightVision));
            containerRegistry.RegisterSingleton(typeof(CodeReader));
            containerRegistry.RegisterSingleton(typeof(SerialPortBase));
            containerRegistry.RegisterSingleton(typeof(ErrorOperation));
            containerRegistry.RegisterSingleton(typeof(DelegateSeq));
            containerRegistry.RegisterSingleton(typeof(SystemConfig));
            containerRegistry.RegisterSingleton(typeof(TowerLight));
            containerRegistry.RegisterSingleton(typeof(CultureResources));

            SystemConfig sysConfig = SystemConfig.Open(@"..\Config Section\General\System.Config");
            containerRegistry.RegisterInstance(sysConfig);

            containerRegistry.Register<ITCPIP, TCPIPBase>();
            containerRegistry.Register<IInsightVision, InSightVision>();
            containerRegistry.Register<ICodeReader, CodeReader>();
            containerRegistry.Register<ISerialPort, SerialPortBase>();
            containerRegistry.Register<IError, ErrorOperation>();
            containerRegistry.Register<ISQLOperation, SQLOperation>();
            containerRegistry.Register<IShowDialog, ShowDialog>();
            containerRegistry.Register<IDelegateSeq, DelegateSeq>();

            MapObject<IBaseIO> baseIO = new MapObject<IBaseIO>();
            IBaseIO ioInstance = baseIO.CreateObject(sysConfig.DigitalIO.ClassName);
            containerRegistry.RegisterInstance<IBaseIO>(ioInstance);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }
    }
}
