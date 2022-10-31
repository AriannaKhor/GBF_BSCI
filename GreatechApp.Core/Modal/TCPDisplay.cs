using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GreatechApp.Core.Modal
{
    public class TCPDisplay : BindableBase
    {
        private int m_ID;
        public int ID
        {
            get { return m_ID; }
            set { SetProperty(ref m_ID, value); }
        }

        private string m_TCPName;
        public string TCPName
        {
            get { return m_TCPName; }
            set { SetProperty(ref m_TCPName, value); }
        }

        private bool m_IsConnected;
        public bool IsConnected
        {
            get { return m_IsConnected; }
            set 
            { 
                SetProperty(ref m_IsConnected, value); 
                Image = IsConnected ? new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/complete.png", uriKind: UriKind.RelativeOrAbsolute)) : new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/error.png", uriKind: UriKind.RelativeOrAbsolute));
            }
        }

        private BitmapImage m_Image;
        public BitmapImage Image
        {
            get { return m_Image; }
            set { SetProperty(ref m_Image, value); }
        }

        private StationType m_Type;
        public StationType Type
        {
            get { return m_Type; }
            set 
            { 
                SetProperty(ref m_Type, value);

                switch (value)
                {
                    case StationType.Vision:
                        TCPImageSource = $@"/GreatechApp.Core;component/Icon/Vision.png";
                        break;

                    case StationType.Tester:
                        TCPImageSource = $@"/GreatechApp.Core;component/Icon/Test.png";
                        break;

                    case StationType.Scanner:
                        TCPImageSource = $@"/GreatechApp.Core;component/Icon/Scanner.png";
                        break;

                    case StationType.SecsGem:
                        TCPImageSource = $@"/GreatechApp.Core;component/Icon/SEMI.png";
                        break;
                }
            }
        }

        private string m_TCPImageSource;
        public string TCPImageSource
        {
            get { return m_TCPImageSource; }
            set { SetProperty(ref m_TCPImageSource, value); }
        }
    }
}
