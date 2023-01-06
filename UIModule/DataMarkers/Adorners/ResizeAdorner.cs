using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace UIModule.DataMarkers.Adorners
{
    public class ResizeAdorner : Adorner
    {
        private VisualCollection m_Visuals;
        private ResizeChrome m_Chrome;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.m_Visuals.Count;
            }
        }

        public ResizeAdorner(ContentControl designerItem)
            : base(designerItem)
        {
            this.m_Chrome = new ResizeChrome();
            this.m_Visuals = new VisualCollection(this);
            this.m_Visuals.Add(this.m_Chrome);
            this.m_Chrome.DataContext = designerItem;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.m_Chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.m_Visuals[index];
        }
    }
}
