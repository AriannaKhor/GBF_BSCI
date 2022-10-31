using GreatechApp.Core.Modal;
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

namespace UIModule.DataMarkers.Resources
{
    /// <summary>
    /// Interaction logic for IconEllipseStatus.xaml
    /// </summary>
    public partial class IconStatus : UserControl
    {
        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(string), typeof(IconStatus), new FrameworkPropertyMetadata("/GreatechApp.Core;component/Icon/Test.png", ImagePropertyChanged));

        public string StationName
        {
            get { return (string)GetValue(StationNameProperty); }
            set { SetValue(StationNameProperty, value); }
        }
        public static readonly DependencyProperty StationNameProperty = DependencyProperty.Register("StationName", typeof(string), typeof(IconStatus), new FrameworkPropertyMetadata("N/A", StationNamePropertyChanged));

        public StationType StationType
        {
            get { return (StationType)GetValue(StationTypeProperty); }
            set { SetValue(StationTypeProperty, value); }
        }
        public static readonly DependencyProperty StationTypeProperty = DependencyProperty.Register("StationType", typeof(StationType), typeof(IconStatus), new PropertyMetadata(StationTypePropertyChanged));

        public StationResult Result
        {
            get { return (StationResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register("Result", typeof(StationResult), typeof(IconStatus), new FrameworkPropertyMetadata(StationResult.NONE, ResultPropertyChanged));

        private static void ImagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var icon = obj as IconStatus;
            string iconPath = (string)e.NewValue;
        }

        private static void ResultPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var icon = obj as IconStatus;
            StationResult result = (StationResult)e.NewValue;

            if (result == StationResult.NONE)
            {
                icon.Status.Fill = Brushes.LightGray;
            }
            else if (result == StationResult.Pass)
            {
                icon.Status.Fill = Brushes.GreenYellow;
            }
            else if (result == StationResult.Fail)
            {
                icon.Status.Fill = Brushes.IndianRed;
            }
            icon.ResultLabel.Content = result.ToString();
        }

        private static void StationNamePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var icon = obj as IconStatus;
            string result = (string)e.NewValue;
            icon.NameLabel.Content = result;
        }

        private static void StationTypePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var icon = obj as IconStatus;
            StationType stationType = (StationType)Enum.Parse(typeof(StationType),e.NewValue.ToString());
            string iconPath = stationType == StationType.Vision ? "/GreatechApp.Core;component/Icon/Vision.png" : "/GreatechApp.Core;component/Icon/Test.png";
            BitmapImage newImage = new BitmapImage(new System.Uri(iconPath, uriKind: UriKind.RelativeOrAbsolute));
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                icon.IconImage.Source = newImage;
            });

        }

        public IconStatus()
        {
            InitializeComponent();
        }
    }
}
