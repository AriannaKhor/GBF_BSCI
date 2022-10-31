using GreatechApp.Core.Variable;
using System.Threading.Tasks;
using System.Windows;

namespace GreatechApp.SplashScreen
{
	/// <summary>
	/// Interaction logic for SplashScreenView.xaml
	/// </summary>
	public partial class SplashScreenView : Window
    {
        SplashScreenViewModel vm;

        public SplashScreenView()
        {
            InitializeComponent();
            vm = (SplashScreenViewModel)DataContext;
        }

        public void UpdateStatus(string title, int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                MachineName.Text = Global.MachName;
                SoftVersion.Content = Global.SoftwareVersion;

                tbLoadTitle.Text = title;
                progBar.Value = value;
            });
        }

        public void CloseDialog()
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.CloseDialogCallback = CloseDialog;
            vm.UpdateStatusCallback = UpdateStatus;

            Task.Factory.StartNew(() =>
            {
                vm.Initialization();
            });
        }
    }
}
