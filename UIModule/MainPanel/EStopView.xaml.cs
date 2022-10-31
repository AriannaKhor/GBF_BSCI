using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UIModule.MainPanel
{
    /// <summary>
    /// Interaction logic for EStopView.xaml
    /// </summary>
    /// 
    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    public partial class EStopView : Window
    {
        const int WM_SIZING = 0x0214;
        const int WM_MOVING = 0x0216;
        private Point WindowTopLeft;
        private Point WindowBottomRight;

        public EStopView()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndMessageHook);

            WindowTopLeft = new Point(0, 0);
            WindowBottomRight = new Point(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }

        private IntPtr HwndMessageHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            switch (msg)
            {
                case WM_SIZING:
                case WM_MOVING:
                    {
                        WIN32Rectangle rectangle = (WIN32Rectangle)Marshal.PtrToStructure(lParam, typeof(WIN32Rectangle));

                        if (rectangle.Left < this.WindowTopLeft.X)
                        {
                            rectangle.Left = (int)this.WindowTopLeft.X;
                            rectangle.Right = (int)this.Left + (int)this.Width;

                            bHandled = true;
                        }

                        if (rectangle.Top < this.WindowTopLeft.Y)
                        {
                            rectangle.Top = (int)this.WindowTopLeft.Y;
                            rectangle.Bottom = (int)this.Top + (int)this.Height;

                            bHandled = true;
                        }

                        if (rectangle.Left + this.Width > this.WindowBottomRight.X)
                        {
                            rectangle.Left = (int)(this.WindowBottomRight.X - this.Width);
                            rectangle.Right = (int)this.WindowBottomRight.X + (int)this.Width;

                            bHandled = true;
                        }

                        if (rectangle.Top + this.Height > this.WindowBottomRight.Y)
                        {
                            rectangle.Top = (int)(WindowBottomRight.Y - this.Height);
                            rectangle.Bottom = (int)WindowBottomRight.Y;

                            bHandled = true;
                        }

                        if (bHandled)
                        {
                            Marshal.StructureToPtr(rectangle, lParam, true);
                        }
                    }
                    break;

            }
            return IntPtr.Zero;
        }


        //private void Window_LocationChanged(object sender, EventArgs e)
        //{
        //    CheckBounds();
        //}

        //private void Window1_SourceInitialized()
        //{
        //    WindowInteropHelper helper = new WindowInteropHelper(this);
        //    source = HwndSource.FromHwnd(helper.Handle);
        //    source.AddHook(WndProc);
        //}

        //const int WM_SYSCOMMAND = 0x0112;
        //const int SC_MOVE = 0xF010;

        //private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{

        //    switch (msg)
        //    {
        //        case WM_SYSCOMMAND:
        //            int command = wParam.ToInt32() & 0xfff0;
        //            if (command == SC_MOVE)
        //            {
        //                handled = true;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return IntPtr.Zero;
        //}

        //private void CheckBounds()
        //{
        //    var height = SystemParameters.PrimaryScreenHeight;
        //    var width = SystemParameters.PrimaryScreenWidth;

        //    if (this.Left < 0)
        //        this.Left = 0;
        //    if (this.Top < 0)
        //        this.Top = 0;
        //    if (this.Top + this.Height > height)
        //        this.Top = height - this.Height;
        //    if (this.Left + this.Width > width)
        //        this.Left = width - this.Width;
        //}
    }
}
