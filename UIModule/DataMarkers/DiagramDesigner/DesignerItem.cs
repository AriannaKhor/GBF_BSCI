using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UIModule.DataMarkers.DiagramDesigner
{
    public class DesignerItem : ContentControl
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        public bool CanSelect
        {
            get { return (bool)GetValue(CanSelectProperty); }
            set { SetValue(CanSelectProperty, value); }
        }
        public static readonly DependencyProperty CanSelectProperty =
          DependencyProperty.Register("CanSelect", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false, CanSelectPropertyChanged));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
          DependencyProperty.Register("Command", typeof(ICommand), typeof(DesignerItem), new FrameworkPropertyMetadata(null, CommandPropertyChanged));

        public string CommandParameter
        {
            get { return (string)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
          DependencyProperty.Register("CommandParameter", typeof(string), typeof(DesignerItem), new FrameworkPropertyMetadata(null));



        public static readonly DependencyProperty MoveThumbTemplateProperty =
            DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetMoveThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
        }

        public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(MoveThumbTemplateProperty, value);
        }

        static DesignerItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem()
        {
            this.Loaded += new RoutedEventHandler(this.DesignerItem_Loaded);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            ContentPresenter presenter = this.FindAncestor<ContentPresenter>();
            DesignerCanvas canvas = this.FindAncestor<DesignerCanvas>();

            if (presenter != null && presenter.Content is IDesignerItem)
            {
                IDesignerItem item = (IDesignerItem)presenter.Content;
                if (CanSelect)
                {
                    if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        // designerItem is hosted by an ItemsControl - where its parent is ContentPresenter, not
                        // Canvas - this layout suits better for MVVM pattern.
                        item.IsSelected = !item.IsSelected;
                    }
                    else
                    {
                        if (!item.IsSelected)
                        {
                            // This is the first selected item. Deselect others in the panel.
                            // Note: This places the contraint to user to put their items in ItemsControl.
                            canvas.DeselectAll();
                            item.IsSelected = true;
                        }
                    }
                }
                else
                {
                    Command.Execute(CommandParameter);
                }
            }

            e.Handled = false;
        }

        private void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;

                MoveThumb thumb =
                    this.Template.FindName("PART_MoveThumb", this) as MoveThumb;

                if (contentPresenter != null && thumb != null)
                {
                    int childCnt = VisualTreeHelper.GetChildrenCount(contentPresenter);
                    UIElement contentVisual =
                        VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

                    if (contentVisual != null)
                    {
                        ControlTemplate template =
                            DesignerItem.GetMoveThumbTemplate(contentVisual) as ControlTemplate;

                        if (template != null)
                        {
                            thumb.Template = template;
                        }
                    }
                }
            }
        }

        private static void CanSelectPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as DesignerItem;
            if (control.CanSelect)
            {
                control.Cursor = Cursors.SizeAll;
            }
            else
            {
                control.Cursor = Cursors.Hand;
            }
        }

        private static void CommandPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as DesignerItem;


        }

        private static void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var control = sender as ContentPresenter;

        }
    }
}
