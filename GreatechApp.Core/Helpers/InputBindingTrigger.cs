using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;

namespace GreatechApp.Core.Helpers
{
    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty =
          DependencyProperty.Register("InputBinding", typeof(InputBinding)
            , typeof(InputBindingTrigger)
            , new UIPropertyMetadata(null));

        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                AssociatedObject.Loaded += delegate
                {
                    var window = GetWindow(AssociatedObject);
                    // Note: need to avoid creating multiple InputBinding when the view changes.
                    if (window != null && !window.InputBindings.Contains(InputBinding))
                    {
                        window.InputBindings.Add(InputBinding);
                    }
                };
            }
            base.OnAttached();
        }

        private Window GetWindow(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                // When logout.
                return null;
            }
            if (frameworkElement is Window)
            {
                return frameworkElement as Window;
            }
            var parent = frameworkElement.Parent as FrameworkElement;
            // When logout, it is possible that parent is null.
            return GetWindow(parent);
        }
    }
}
