using UIModule.DataMarkers.DiagramDesigner;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UIModule.DataMarkers.Adorners
{
    public class RubberbandAdorner : Adorner
    {
        private Point? m_StartPoint, m_EndPoint;
        private readonly Rectangle m_Rubberband;
        private readonly DesignerCanvas m_DesignerCanvas;
        private readonly VisualCollection m_Visuals;
        private readonly Canvas m_AdornerCanvas;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.m_Visuals.Count;
            }
        }

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.m_DesignerCanvas = designerCanvas;
            this.m_StartPoint = dragStartPoint;

            this.m_AdornerCanvas = new Canvas();
            this.m_AdornerCanvas.Background = Brushes.Transparent;
            this.m_Visuals = new VisualCollection(this);
            this.m_Visuals.Add(this.m_AdornerCanvas);

            this.m_Rubberband = new Rectangle();
            this.m_Rubberband.Stroke = Brushes.Navy;
            this.m_Rubberband.StrokeThickness = 1;
            this.m_Rubberband.StrokeDashArray = new DoubleCollection(new double[] { 2 });

            this.m_AdornerCanvas.Children.Add(this.m_Rubberband);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                {
                    this.CaptureMouse();
                }

                this.m_EndPoint = e.GetPosition(this);
                this.UpdateRubberband();
                this.UpdateSelection();
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
            }

            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.m_AdornerCanvas.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.m_Visuals[index];
        }

        private void UpdateRubberband()
        {
            double left = Math.Min(this.m_StartPoint.Value.X, this.m_EndPoint.Value.X);
            double top = Math.Min(this.m_StartPoint.Value.Y, this.m_EndPoint.Value.Y);

            double width = Math.Abs(this.m_StartPoint.Value.X - this.m_EndPoint.Value.X);
            double height = Math.Abs(this.m_StartPoint.Value.Y - this.m_EndPoint.Value.Y);

            this.m_Rubberband.Width = width;
            this.m_Rubberband.Height = height;
            Canvas.SetLeft(this.m_Rubberband, left);
            Canvas.SetTop(this.m_Rubberband, top);
        }

        private void UpdateSelection()
        {
            Rect rubberBand = new Rect(this.m_StartPoint.Value, this.m_EndPoint.Value);
            foreach (ContentPresenter presenter in this.m_DesignerCanvas.Children)
            {
                Rect itemRect = VisualTreeHelper.GetDescendantBounds(presenter);
                Rect itemBounds = presenter.TransformToAncestor(m_DesignerCanvas).TransformBounds(itemRect);
                IDesignerItem item = presenter.Content as IDesignerItem;
                Debug.Assert(item != null, "This DesingerItem is not added into a ItemsControl.");
                if (rubberBand.Contains(itemBounds))
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }
    }
}
