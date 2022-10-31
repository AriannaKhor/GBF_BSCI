using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace UIModule.DataMarkers.Resources
{
    public class CirclePanel : Panel
    {
        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }
        public static readonly DependencyProperty OuterRadiusProperty =
          DependencyProperty.Register("OuterRadius", typeof(double), typeof(CirclePanel), new UIPropertyMetadata(0.0,
           (o, e) =>
           {
               (o as CirclePanel).Width = (double)e.NewValue * 2;
               (o as CirclePanel).Height = (double)e.NewValue * 2;
           }));
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }
        public static readonly DependencyProperty InnerRadiusProperty =
          DependencyProperty.Register("InnerRadius", typeof(double), typeof(CirclePanel), new UIPropertyMetadata(0.0));


        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }
            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point currentPosition = new Point(OuterRadius, (OuterRadius - InnerRadius) / 2);

            double OffsetX = 0;
            double OffsetY = 0;
            int childCount = Children.Count;
            double perAngle = 2 * Math.PI / childCount;
            for (int i = 0; i < childCount; i++)
            {
                UIElement child = Children[i];
                double angle = (i+1) * perAngle;

                OffsetX = Math.Sin(angle) * (OuterRadius + InnerRadius) / 2;
                OffsetY = (1 - Math.Cos(angle)) * (OuterRadius + InnerRadius) / 2;
                Rect childRect = new Rect(
                 new Point(currentPosition.X - child.DesiredSize.Width / 2, currentPosition.Y - child.DesiredSize.Height / 2),
                 new Point(currentPosition.X + child.DesiredSize.Width / 2, currentPosition.Y + child.DesiredSize.Height / 2));
                child.Arrange(childRect);
                currentPosition.X = OffsetX + OuterRadius;
                currentPosition.Y = OffsetY + (OuterRadius - InnerRadius) / 2;


                /// This formula is apply center point of circle on top of inner radius circle
                //Point center = new Point(Width/2, Height/2);
                //double angle = i * perAngle;
                //Point chilCenter = new Point(center.X + (InnerRadius * Math.Sin(angle)), center.Y - (InnerRadius * Math.Cos(angle)));

                //Rect childRect = new Rect(
                // new Point(chilCenter.X - child.DesiredSize.Width / 2, chilCenter.Y - child.DesiredSize.Height / 2),
                // new Point(chilCenter.X + child.DesiredSize.Width / 2, chilCenter.Y + child.DesiredSize.Height / 2));
                //child.Arrange(childRect);
            }
            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

    }
}
