using MahApps.Metro.Controls;
using System.Windows.Controls;
using UIModule.DataMarkers;

namespace UIModule.DataMarkers
{
    /// <summary>
    /// Interaction logic for CircularDataMarkerView.xaml
    /// </summary>
    public partial class CircularDataMarkerView : UserControl
    {
        public CircularDataMarkerView()
        {
            InitializeComponent();
        }

        private void Storyboard_Completed(object sender, System.EventArgs e)
        {
            ((CircularDataMarkerViewModel)this.DataContext).OnBlinkComplete();
        }
    }
}
