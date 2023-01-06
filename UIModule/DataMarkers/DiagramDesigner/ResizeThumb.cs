using GreatechApp.Core.Events;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace UIModule.DataMarkers.DiagramDesigner
{
    public class ResizeThumb : Thumb
    {
        private ContentControl m_DesignerItem;
        private DesignerCanvas m_DesignerCanvas;

        public ResizeThumb()
        {
            DragStarted += new DragStartedEventHandler(this.ResizeThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
            DragCompleted += new DragCompletedEventHandler(this.ResizeThumb_DragCompleted);
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.m_DesignerItem = this.DataContext as ContentControl;
            if (this.m_DesignerItem != null)
            {
                this.m_DesignerCanvas = this.m_DesignerItem.FindAncestor<DesignerCanvas>();
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.m_DesignerItem != null && this.m_DesignerCanvas != null)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;
                double minDeltaHorizontal = double.MaxValue;
                double minDeltaVertical = double.MaxValue;
                double dragDeltaVertical, dragDeltaHorizontal;
                foreach (ContentPresenter presenter in this.m_DesignerCanvas.SelectedItems)
                {
                    IDesignerItem item = presenter.Content as IDesignerItem;
                    Debug.Assert(item != null, "This DesingerItem is not added into a ItemsControl.");
                    minLeft = Math.Min(item.X, minLeft);
                    minTop = Math.Min(item.Y, minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, item.Height - item.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.Width - item.MinWidth);
                }
                foreach (ContentPresenter presenter in this.m_DesignerCanvas.SelectedItems)
                {
                    IDesignerItem item = presenter.Content as IDesignerItem;
                    Debug.Assert(item != null, "This DesingerItem is not added into a ItemsControl.");
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            item.Height = item.Height - dragDeltaVertical;
                            break;

                        case VerticalAlignment.Top:
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            item.Y = item.Y + dragDeltaVertical;
                            item.Height = item.Height - dragDeltaVertical;
                            break;
                    }

                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            item.X = item.X + dragDeltaHorizontal;
                            item.Width = item.Width - dragDeltaHorizontal;
                            break;

                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            item.Width = item.Width - dragDeltaHorizontal;
                            break;
                    }
                }
                e.Handled = true;
            }
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ContainerLocator.Container.Resolve<IEventAggregator>().GetEvent<RecordDesignerItem>().Publish();
        }
    }
}
