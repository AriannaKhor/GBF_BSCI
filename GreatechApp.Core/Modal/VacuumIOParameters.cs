using GreatechApp.Core.Enums;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GreatechApp.Core.Modal
{
    public class VacuumIOParameters : BindableBase
    {
        private BitmapImage m_VacuumImage;
        public BitmapImage VacuumImage
        {
            get { return m_VacuumImage; }
            set { SetProperty(ref m_VacuumImage, value); }
        }

        private BitmapImage m_PurgeImage;
        public BitmapImage PurgeImage
        {
            get { return m_PurgeImage; }
            set { SetProperty(ref m_PurgeImage, value); }
        }

        private BitmapImage m_VacOnImage;
        public BitmapImage VacOnImage
        {
            get { return m_VacOnImage; }
            set { SetProperty(ref m_VacOnImage, value); }
        }

        private BitmapImage m_PickedUpImage;
        public BitmapImage PickedUpImage
        {
            get { return m_PickedUpImage; }
            set { SetProperty(ref m_PickedUpImage, value); }
        }

        private string m_VacuumName;
        public string VacuumName
        {
            get { return m_VacuumName; }
            set { SetProperty(ref m_VacuumName, value); }
        }

        private bool m_IsPurgeAvail = false;
        public bool IsPurgeAvail
        {
            get { return m_IsPurgeAvail; }
            set { SetProperty(ref m_IsPurgeAvail, value); }
        }

        private bool m_IsVacuumOn;
        public bool IsVacuumOn
        {
            get { return m_IsVacuumOn; }
            set { SetProperty(ref m_IsVacuumOn, value); }
        }

        private bool? m_IsPurgeOn;
        public bool? IsPurgeOn
        {
            get { return m_IsPurgeOn; }
            set { SetProperty(ref m_IsPurgeOn, value); }
        }

        private bool? m_IsVacuumSnsOn;
        public bool? IsVacuumSnsOn
        {
            get { return m_IsVacuumSnsOn; }
            set
            {
                SetProperty(ref m_IsVacuumSnsOn, value);
                if (value != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        VacOnImage = (bool)value ?
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
                    });
                }
            }
        }

        private bool? m_IsPickedUpSnsOn;
        public bool? IsPickedUpSnsOn
        {
            get { return m_IsPickedUpSnsOn; }
            set 
            { 
                SetProperty(ref m_IsPickedUpSnsOn, value);
                if (value != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PickedUpImage = (bool)value ?
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/GreenIcon.png", uriKind: UriKind.RelativeOrAbsolute)) :
                            new BitmapImage(new System.Uri("/GreatechApp.Core;component/Icon/G_OFF.png", uriKind: UriKind.RelativeOrAbsolute));
                    });
                }
            }
        }

        public string VacuumOnTooltip
        {
            get { return VacuumSns == null? null: VacuumSns.ToString(); }
        }

        public string PickedUpTooltip
        {
            get { return PickedUpSns == null? null: PickedUpSns.ToString(); }
        }

        public string VacuumOutputTooltip
        {
            get { return Vacuum.ToString(); }
        }

        public string PurgeOutputTooltip
        {
            get { return Purge.ToString(); }
        }

        private OUT m_Vacuum;
        public OUT Vacuum
        {
            get { return m_Vacuum; }
            set { SetProperty(ref m_Vacuum, value); }
        }

        private OUT? m_Purge;
        public OUT? Purge
        {
            get { return m_Purge; }
            set
            {
                SetProperty(ref m_Purge, value);
                IsPurgeAvail = value != null;
            }
        }

        private IN? m_VacuumSns;
        public IN? VacuumSns
        {
            get { return m_VacuumSns; }
            set { SetProperty(ref m_VacuumSns, value); }
        }

        private IN? m_PickedUpSns;
        public IN? PickedUpSns
        {
            get { return m_PickedUpSns; }
            set { SetProperty(ref m_PickedUpSns, value); }
        }

    }
}
