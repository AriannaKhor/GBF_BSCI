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
    /// Interaction logic for InputIcon.xaml
    /// </summary>
    public partial class InputIcon : UserControl
    {
        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register("IsOn", typeof(bool), typeof(InputIcon), new FrameworkPropertyMetadata(false, OnIsOn));

        #region Public Properties
        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }
        #endregion

        #region Constructor
        public InputIcon()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private static void OnIsOn(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var input = d as InputIcon;
            var onProperty = (bool)e.NewValue;

            if (input != null && onProperty)
            {
                input.Grad0.Color = Colors.GreenYellow;
                input.Grad1.Color = Colors.Green;
            }
            else if (input != null && !onProperty)
            {
                input.Grad0.Color = Colors.DarkGreen;
                input.Grad1.Color = Colors.DarkOliveGreen;
            }
        }
        #endregion
    }
}
