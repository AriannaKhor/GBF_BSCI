using GreatechApp.Core.Events;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace UIModule.DataMarkers.DiagramDesigner
{
    public class MoveThumb : Thumb
    {
        private RotateTransform m_RotateTransform;
        private DesignerItem m_DesignerItem;
        private DesignerCanvas m_DesignerCanvas;

        public MoveThumb()
        {
            DragStarted += new DragStartedEventHandler(this.MoveThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
            DragCompleted += new DragCompletedEventHandler(this.MoveThumb_DragCompleted);
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.m_DesignerItem = DataContext as DesignerItem;

            if (this.m_DesignerItem != null)
            {
                this.m_RotateTransform = this.m_DesignerItem.RenderTransform as RotateTransform;
                this.m_DesignerCanvas = this.m_DesignerItem.FindAncestor<DesignerCanvas>();
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.m_DesignerItem != null && this.m_DesignerCanvas != null)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                foreach (ContentPresenter presenter in this.m_DesignerCanvas.SelectedItems)
                {
                    IDesignerItem item = presenter.Content as IDesignerItem;
                    Debug.Assert(item != null, "This DesingerItem is not added into a ItemsControl.");
                    minLeft = Math.Min(item.X, minLeft);
                    minTop = Math.Min(item.Y, minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (ContentPresenter presenter in this.m_DesignerCanvas.SelectedItems)
                {
                    IDesignerItem item = presenter.Content as IDesignerItem;
                    Debug.Assert(item != null, "This DesingerItem is not added into a ItemsControl.");
                    item.X = item.X + deltaHorizontal;
                    item.Y = item.Y + deltaVertical;
                }
                this.m_DesignerCanvas.InvalidateMeasure();
                e.Handled = true;
            }
        }

        private void MoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ContainerLocator.Container.Resolve<IEventAggregator>().GetEvent<RecordDesignerItem>().Publish();
        }
    }
}
