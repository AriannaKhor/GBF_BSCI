using System.Collections.ObjectModel;

namespace GreatechApp.Core.Helpers
{
    public class FixedSizeObservableCollection<T> : ObservableCollection<T>
    {
        public int MaxCollectionSize { get; private set; }

        public FixedSizeObservableCollection(int maxCollectionSize = 0)
            : base()
        {
            MaxCollectionSize = maxCollectionSize;
        }

        protected override void InsertItem(int index, T item)
        {
            // So that the datalog list item won't grow too large - especially when application runs continously for a long
            // period of time.
            if (MaxCollectionSize > 0 && Count >= MaxCollectionSize)
            {
                // As optimization, the while loop is removed. 
                this.RemoveItem(0);

                index -= 1;
            }
            base.InsertItem(index, item);
        }
    }
}
