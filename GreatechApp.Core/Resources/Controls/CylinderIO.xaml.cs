using GreatechApp.Core.Modal;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToggleSwitch;

namespace GreatechApp.Core.Resources.Controls
{
    /// <summary>
    /// Interaction logic for CylinderIO.xaml
    /// </summary>
    public partial class CylinderIO : UserControl
    {
        public static readonly DependencyProperty CylinderNameProperty = DependencyProperty.Register("CylinderName", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Cylinder", CylinderNamePropertyChanged));
        public static readonly DependencyProperty WorkSnsTooltip1Property = DependencyProperty.Register("WorkSnsTooltip1", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Work Sensor 1", WorkSnsTooltip1PropertyChanged));
        public static readonly DependencyProperty WorkSnsTooltip2Property = DependencyProperty.Register("WorkSnsTooltip2", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Work Sensor 2", WorkSnsTooltip2PropertyChanged));
        public static readonly DependencyProperty RestSnsTooltip1Property = DependencyProperty.Register("RestSnsTooltip1", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Rest Sensor 1", RestSnsTooltip1PropertyChanged));
        public static readonly DependencyProperty RestSnsTooltip2Property = DependencyProperty.Register("RestSnsTooltip2", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Rest Sensor 2", RestSnsTooltip2PropertyChanged));
        public static readonly DependencyProperty WorkTooltipProperty = DependencyProperty.Register("WorkTooltip", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Work Cylinder", WorkTooltipPropertyChanged));
        public static readonly DependencyProperty RestTooltipProperty = DependencyProperty.Register("RestTooltip", typeof(string), typeof(CylinderIO), new FrameworkPropertyMetadata("Rest Cylinder", RestTooltipPropertyChanged));

        public static readonly DependencyProperty IsCylinderWorkProperty = DependencyProperty.Register("IsCylinderWork", typeof(bool?), typeof(CylinderIO), new FrameworkPropertyMetadata(null, IsCylinderWorkPropertyChanged));
        public static readonly DependencyProperty IsWorkSns1Property = DependencyProperty.Register("IsWorkSns1", typeof(bool?), typeof(CylinderIO), new FrameworkPropertyMetadata(null, IsWorkSns1PropertyChanged));
        public static readonly DependencyProperty IsWorkSns2Property = DependencyProperty.Register("IsWorkSns2", typeof(bool?), typeof(CylinderIO), new FrameworkPropertyMetadata(null, IsWorkSns2PropertyChanged));
        public static readonly DependencyProperty IsRestSns1Property = DependencyProperty.Register("IsRestSns1", typeof(bool?), typeof(CylinderIO), new FrameworkPropertyMetadata(null, IsRestSns1PropertyChanged));
        public static readonly DependencyProperty IsRestSns2Property = DependencyProperty.Register("IsRestSns2", typeof(bool?), typeof(CylinderIO), new FrameworkPropertyMetadata(null, IsRestSns2PropertyChanged));
        public static readonly DependencyProperty WorkCommandProperty = DependencyProperty.Register("WorkCommand", typeof(ICommand), typeof(CylinderIO));
        public static readonly DependencyProperty RestCommandProperty = DependencyProperty.Register("RestCommand", typeof(ICommand), typeof(CylinderIO));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CylinderIO), new FrameworkPropertyMetadata(null, CommandParameterPropertyChanged));

        private bool CheckedPropertyChanged = false;
        //public static readonly DependencyProperty WorkCommandProperty = DependencyProperty.Register("WorkCommand", typeof(DelegateCommand), typeof(CylinderIO), new FrameworkPropertyMetadata(WorkCommandPropertyChanged));
        //public static readonly DependencyProperty RestCommandProperty = DependencyProperty.Register("RestCommand", typeof(DelegateCommand), typeof(CylinderIO), new FrameworkPropertyMetadata(RestCommandPropertyChanged));

        #region Public Properties
        public string CylinderName
        {
            get { return (string)GetValue(CylinderNameProperty); }
            set { SetValue(CylinderNameProperty, value); }
        }
        public string WorkTooltip
        {
            get { return (string)GetValue(WorkTooltipProperty); }
            set { SetValue(WorkTooltipProperty, value); }
        }
        public string RestTooltip
        {
            get { return (string)GetValue(RestTooltipProperty); }
            set { SetValue(RestTooltipProperty, value); }
        }
        public string WorkSnsTooltip1
        {
            get { return (string)GetValue(WorkSnsTooltip1Property); }
            set { SetValue(WorkSnsTooltip1Property, value); }
        }
        public string WorkSnsTooltip2
        {
            get { return (string)GetValue(WorkSnsTooltip2Property); }
            set { SetValue(WorkSnsTooltip2Property, value); }
        }
        public string RestSnsTooltip1
        {
            get { return (string)GetValue(RestSnsTooltip1Property); }
            set { SetValue(RestSnsTooltip1Property, value); }
        }
        public string RestSnsTooltip2
        {
            get { return (string)GetValue(RestSnsTooltip2Property); }
            set { SetValue(RestSnsTooltip2Property, value); }
        }
        public bool IsCylinderWork
        {
            get { return (bool)GetValue(IsCylinderWorkProperty); }
            set { SetValue(IsCylinderWorkProperty, value); }
        }
        public bool IsWorkSns1
        {
            get { return (bool)GetValue(IsWorkSns1Property); }
            set { SetValue(IsWorkSns1Property, value); }
        }
        public bool IsWorkSns2
        {
            get { return (bool)GetValue(IsWorkSns2Property); }
            set { SetValue(IsWorkSns2Property, value); }
        }
        public bool IsRestSns1
        {
            get { return (bool)GetValue(IsRestSns1Property); }
            set { SetValue(IsRestSns1Property, value); }
        }
        public bool IsRestSns2
        {
            get { return (bool)GetValue(IsRestSns2Property); }
            set { SetValue(IsRestSns2Property, value); }
        }

        public ICommand WorkCommand
        {
            get { return (ICommand)GetValue(WorkCommandProperty); }
            set { SetValue(WorkCommandProperty, value); }
        }
        public ICommand RestCommand
        {
            get { return (ICommand)GetValue(RestCommandProperty); }
            set { SetValue(RestCommandProperty, value); }
        }
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        #endregion

        public CylinderIO()
        {
            InitializeComponent();

        }

        #region Event
        private static void CylinderNamePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.Title.Content = e.NewValue;
        }
        private static void WorkTooltipPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.WorkCylinder.ToolTip = e.NewValue;
        }
        private static void RestTooltipPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            if(e.NewValue != null)
            {
                IO.RestCylinder.ToolTip = e.NewValue;
            }
            else
            {
                IO.RestCylinder.Foreground = Brushes.Gray;
            }
        }
        private static void WorkSnsTooltip1PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.WorkSns1.ToolTip = e.NewValue;
        }
        private static void WorkSnsTooltip2PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.WorkSns2.ToolTip = e.NewValue;
        }
        private static void RestSnsTooltip1PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.RestSns1.ToolTip = e.NewValue;
        }
        private static void RestSnsTooltip2PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.RestSns2.ToolTip = e.NewValue;
        }

        private static void IsCylinderWorkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            Application.Current.Dispatcher.Invoke(() =>
            {
                IO.ToggleSW.IsChecked = (bool)e.NewValue;
            });
        }
        private static void IsWorkSns1PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            Application.Current.Dispatcher.Invoke(() =>
            {
                IO.WorkSns1.Source = (bool)e.NewValue ?
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
            });
        }
        private static void IsWorkSns2PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            Application.Current.Dispatcher.Invoke(() =>
            {
                IO.WorkSns2.Source = (bool)e.NewValue ?
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
            });
        }
        private static void IsRestSns1PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            Application.Current.Dispatcher.Invoke(() =>
            {
                IO.RestSns1.Source = (bool)e.NewValue ?
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
            });
        }
        private static void IsRestSns2PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            Application.Current.Dispatcher.Invoke(() =>
            {
                IO.RestSns2.Source = (bool)e.NewValue ?
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
            });
        }

        private static void CommandParameterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var IO = obj as CylinderIO;
            IO.CommandParameter = (object)e.NewValue;
        }
        #endregion

        //private void ToggleSW_Checked(object sender, RoutedEventArgs e)
        //{
        //    IsCylinderWork = true;
        //    if(WorkCommand != null)
        //        WorkCommand.Execute(CommandParameter);
        //}

        //private void ToggleSW_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (RestCommand != null)
        //        RestCommand.Execute(CommandParameter);
        //    IsCylinderWork = false;
        //}

        private void ToggleSW_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HorizontalToggleSwitch ts = (HorizontalToggleSwitch)sender;

            if (!ts.IsChecked)
            {
                ts.IsChecked = true;
                if (WorkCommand != null)
                    WorkCommand.Execute(CommandParameter);
                
            }

            else
            {
                ts.IsChecked = false;
                if (RestCommand != null)
                    RestCommand.Execute(CommandParameter);
                
            }
        }
    }
}
