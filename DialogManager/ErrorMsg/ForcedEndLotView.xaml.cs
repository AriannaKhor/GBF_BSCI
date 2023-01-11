using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DialogManager.ErrorMsg
{
    /// <summary>
    /// Interaction logic for ForcedEndLotView.xaml
    /// </summary>
    public partial class ForcedEndLotView : UserControl
    {
        private bool IsMaskPass = false;

        public ForcedEndLotView()
        {
            InitializeComponent();
            PassImage.Source = new BitmapImage(new Uri(@"/GreatechApp.Core;component/Icon/hidden.png", UriKind.Relative));
        }

        private void Password_Click(object sender, RoutedEventArgs e)
        {
            if (IsMaskPass)
            {
                IsMaskPass = false;
                MaskPass.Visibility = Visibility.Visible;
                UnMaskPass.Visibility = Visibility.Collapsed;
                PassImage.Source = new BitmapImage(new Uri(@"/GreatechApp.Core;component/Icon/hidden.png", UriKind.Relative));
                MaskPass.Focus();
            }
            else
            {
                IsMaskPass = true;
                MaskPass.Visibility = Visibility.Collapsed;
                UnMaskPass.Visibility = Visibility.Visible;
                PassImage.Source = new BitmapImage(new Uri(@"/GreatechApp.Core;component/Icon/view.png", UriKind.Relative));
                UnMaskPass.Focus();
            }
        }

        private void MaskPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UnMaskPass.Text = ((PasswordBox)sender).Password.ToString();
        }
    }
}
