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
    /// Interaction logic for ErrVerificationView.xaml
    /// </summary>
    public partial class ErrVerificationView : UserControl
    {
        public ErrVerificationView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UnMaskPass.Text = ((PasswordBox)sender).Password.ToString();
        }
    }
}
 