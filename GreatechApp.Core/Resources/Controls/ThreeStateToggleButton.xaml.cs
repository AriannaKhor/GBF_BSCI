using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Mvvm;

namespace GreatechApp.Core.Resources.Controls
{
    /// <summary>
    /// Interaction logic for ThreeStateToggleButton.xaml
    /// </summary>
    public partial class ThreeStateToggleButton : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty Content1 = DependencyProperty.Register("ButtonContent1", typeof(string), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata("Content1", Content1PropertyChanged));
        public static readonly DependencyProperty Content2 = DependencyProperty.Register("ButtonContent2", typeof(string), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata("Content2", Content2PropertyChanged));
        public static readonly DependencyProperty Content3 = DependencyProperty.Register("ButtonContent3", typeof(string), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata("Content3", Content3PropertyChanged));

        public static readonly DependencyProperty BackgroundColourProperty = DependencyProperty.Register("BackgroundColour", typeof(SolidColorBrush), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(Brushes.Orange,BackgroundColourPropertyChanged));
        public static readonly DependencyProperty TextColourProperty = DependencyProperty.Register("TextColour", typeof(SolidColorBrush), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(Brushes.White, TextColourPropertyChanged));

        public static readonly DependencyProperty Ischecked1Property = DependencyProperty.Register("IsChecked1", typeof(bool), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(false, OnIsChecked1));
        public static readonly DependencyProperty Ischecked2Property = DependencyProperty.Register("IsChecked2", typeof(bool), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(false, OnIsChecked2));
        public static readonly DependencyProperty Ischecked3Property = DependencyProperty.Register("IsChecked3", typeof(bool), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(false, OnIsChecked3));
        public static readonly DependencyProperty TagNameProperty = DependencyProperty.Register("TagName", typeof(string), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(""));
        public static readonly DependencyProperty NumOfButtonProperty = DependencyProperty.Register("NumOfButton", typeof(int), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(3, OnNumOfButtonChanged));

        #region Public Properties
        public int NumOfButton
        {
            get { return (int)GetValue(NumOfButtonProperty); }
            set { SetValue(NumOfButtonProperty, value); }
        }
        public bool IsChecked1
        {
            get { return (bool)GetValue(Ischecked1Property); }
            set 
            { 
                SetValue(Ischecked1Property, value);
                NotifyPropertyChanged("IsChecked1");
            }
        }
        public bool IsChecked2
        {
            get { return (bool)GetValue(Ischecked2Property); }
            set
            {
                SetValue(Ischecked2Property, value);
                NotifyPropertyChanged("IsChecked2");
            }
        }
        public bool IsChecked3
        {
            get { return (bool)GetValue(Ischecked3Property); }
            set 
            { 
                SetValue(Ischecked3Property, value); 
                NotifyPropertyChanged("IsChecked3");
            }
        }
        public SolidColorBrush BackgroundColour
        {
            get { return (SolidColorBrush)GetValue(BackgroundColourProperty); }
            set { SetValue(BackgroundColourProperty, value); }
        }

        public SolidColorBrush TextColour
        {
            get { return (SolidColorBrush)GetValue(TextColourProperty); }
            set { SetValue(TextColourProperty, value); }
        }

        public string ButtonContent1
        {
            get { return (string)GetValue(Content1); }
            set 
            { 
                SetValue(Content1, value);
                NotifyPropertyChanged("ButtonContent1");
            }
        }
        public string ButtonContent2
        {
            get { return (string)GetValue(Content2); }
            set { SetValue(Content2, value); }
        }
        public string ButtonContent3
        {
            get { return (string)GetValue(Content3); }
            set { SetValue(Content3, value); }
        }
        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        private ObservableCollection<Visibility> m_ButtonVisibility;
        public ObservableCollection<Visibility> ButtonVisibility 
        {
            get { return m_ButtonVisibility; }
            set 
            { 
                m_ButtonVisibility = value;
                NotifyPropertyChanged("ButtonVisibility");
            } 
        }
        #endregion

        #region Constructor
        public ThreeStateToggleButton()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private static void OnNumOfButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = d as ThreeStateToggleButton;
            var numOfButton = (int)e.NewValue;

            if (toggleButton != null && numOfButton != 0)
            {
                toggleButton.ButtonVisibility = new ObservableCollection<Visibility> { Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed };
                for (int index = 0; index < numOfButton; index++)
                {
                    toggleButton.ButtonVisibility[index] = Visibility.Visible;
                }
            }
        }
        private static void OnIsChecked1(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = d as ThreeStateToggleButton;
            var buttonResult = (bool)e.NewValue;
            if (toggleButton != null)
            {
                toggleButton.Button1.IsChecked = buttonResult;
            }
        }
        private static void OnIsChecked2(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = d as ThreeStateToggleButton;
            var buttonResult = (bool)e.NewValue;
            if (toggleButton != null)
            {
                toggleButton.Button2.IsChecked = buttonResult;
            }
        }
        private static void OnIsChecked3(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = d as ThreeStateToggleButton;
            var buttonResult = (bool)e.NewValue;
            if (toggleButton != null)
            {
                toggleButton.Button3.IsChecked = buttonResult;
            }
        }

        private void Button1_Checked(object sender, RoutedEventArgs e)
        {
            Button2.IsChecked = false;
            Button3.IsChecked = false;
            IsChecked1 = true;
        }

        private void Button2_Checked(object sender, RoutedEventArgs e)
        {
            Button1.IsChecked = false;
            Button3.IsChecked = false;
            IsChecked2 = true;

        }

        private void Button3_Checked(object sender, RoutedEventArgs e)
        {
            Button2.IsChecked = false;
            Button1.IsChecked = false;
            IsChecked3 = true;

        }
        private static void Content1PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = obj as ThreeStateToggleButton;
            toggleButton.Button1.Content = e.NewValue;
        }
        private static void Content2PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = obj as ThreeStateToggleButton;
            toggleButton.Button2.Content = e.NewValue;
        }
        private static void Content3PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = obj as ThreeStateToggleButton;
            toggleButton.Button3.Content = e.NewValue;
        }

        private static void BackgroundColourPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = obj as ThreeStateToggleButton;
            toggleButton.BackgroundColour = (SolidColorBrush)e.NewValue;
        }
        private static void TextColourPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var toggleButton = obj as ThreeStateToggleButton;
            toggleButton.TextColour = (SolidColorBrush)e.NewValue;
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
