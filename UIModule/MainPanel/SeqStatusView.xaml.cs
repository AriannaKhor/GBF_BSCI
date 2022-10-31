using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace UIModule.MainPanel
{
    /// <summary>
    /// Interaction logic for SeqStatusView.xaml
    /// </summary>
    public partial class SeqStatusView : UserControl
    {
        public SeqStatusView()
        {
#if DEBUG || SIMULATION
            //only perform the following fix if we are in the designer
            // - the default ctor is not executed when editing the usercontrol,
            //   but is executed when usercontrol has been added to a window/page
            // NB. The Visual Studio designer might return null for Application.Current
            //     http://msdn.microsoft.com/en-us/library/bb546934.aspx
            if (DesignerProperties.GetIsInDesignMode(this) && Application.Current != null)
            {
                Uri resourceLocater =
                    new System.Uri("/GreatechApp.Core;component/Cultures/CultureRD.xaml",
                        UriKind.Relative);
                ResourceDictionary dictionary =
                    (ResourceDictionary)Application.LoadComponent(resourceLocater);
                //add the resourcedictionary containing our Resources ODP to
                //App.Current (which is the Designer / Blend)
                if (!Application.Current.Resources.MergedDictionaries.Contains(dictionary))
                    Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
#endif
            InitializeComponent();
        }

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void ListView_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lv.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
