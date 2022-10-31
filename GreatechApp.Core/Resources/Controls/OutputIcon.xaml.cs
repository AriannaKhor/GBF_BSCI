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

namespace GreatechApp.Core.Resources.Controls
{
    /// <summary>
    /// Interaction logic for OutputIcon.xaml
    /// </summary>
    public partial class OutputIcon : UserControl
    {
        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register("IsOn", typeof(bool), typeof(OutputIcon), new FrameworkPropertyMetadata(false, OnIsOn));

        #region Public Properties
        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }
        #endregion

        #region Constructor
        public OutputIcon()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private static void OnIsOn(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var output = d as OutputIcon;
            var onProperty = (bool)e.NewValue;

            if (output != null && onProperty)
            {
                output.Grad0.Color = Colors.OrangeRed;
                output.Grad1.Color = Colors.Red;
            }
            else if (output != null && !onProperty)
            {
                output.Grad0.Color = Colors.DarkRed;
                output.Grad1.Color = Colors.DarkRed;
            }
        }
        #endregion
    }
}
