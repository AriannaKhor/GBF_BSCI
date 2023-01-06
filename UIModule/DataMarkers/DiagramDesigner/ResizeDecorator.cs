using UIModule.DataMarkers.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace UIModule.DataMarkers.DiagramDesigner
{
    public class ResizeDecorator : Control
    {
        private Adorner m_Adorner;

        public bool ShowDecorator
        {
            get { return (bool)GetValue(ShowDecoratorProperty); }
            set { SetValue(ShowDecoratorProperty, value); }
        }

        public static readonly DependencyProperty ShowDecoratorProperty =
            DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(ResizeDecorator),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(ShowDecoratorProperty_Changed)));

        public ResizeDecorator()
        {
            Unloaded += new RoutedEventHandler(this.ResizeDecorator_Unloaded);
        }

        private void HideAdorner()
        {
            if (this.m_Adorner != null)
            {
                this.m_Adorner.Visibility = Visibility.Hidden;
            }
        }

        private void ShowAdorner()
        {
            if (this.m_Adorner == null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (adornerLayer != null)
                {
                    ContentControl designerItem = this.DataContext as ContentControl;
                    Canvas canvas = VisualTreeHelper.GetParent(designerItem) as Canvas;
                    this.m_Adorner = new ResizeAdorner(designerItem);
                    adornerLayer.Add(this.m_Adorner);

                    if (this.ShowDecorator)
                    {
                        this.m_Adorner.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.m_Adorner.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                this.m_Adorner.Visibility = Visibility.Visible;
            }
        }

        private void ResizeDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.m_Adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(this.m_Adorner);
                }

                this.m_Adorner = null;
            }
        }

        private static void ShowDecoratorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResizeDecorator decorator = (ResizeDecorator)d;
            bool showDecorator = (bool)e.NewValue;

            if (showDecorator)
            {
                decorator.ShowAdorner();
            }
            else
            {
                decorator.HideAdorner();
            }
        }
    }
}
