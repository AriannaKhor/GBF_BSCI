using System.Windows.Controls;
using UIModule.DataMarkers;

namespace UIModule.DataMarkers
{
    /// <summary>
    /// Interaction logic for LinearDataMarkerView.xaml
    /// </summary>
    public partial class LinearDataMarkerView : UserControl
    {
        public LinearDataMarkerView()
        {
            InitializeComponent();
        }
        private void Storyboard_Completed(object sender, System.EventArgs e)
        {
            ((LinearDataMarkerViewModel)this.DataContext).OnBlinkComplete();
        }
    }
}
