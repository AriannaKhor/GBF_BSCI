using ConfigManager;
using GreatechApp.Core;
using GreatechApp.Core.Cultures;
using GreatechApp.Core.Events;
using GreatechApp.Core.Interface;
using GreatechApp.Core.Resources;
using GreatechApp.Core.Variable;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using UIModule.MainPanel;

namespace UIModule.StandardViews
{
    public class AboutViewModel : BaseUIViewModel, INavigationAware, IRegionMemberLifetime
    {
        #region Variable

        private string m_title;
        public string Title
        {
            get { return m_title; }
            set { SetProperty(ref m_title, value); }
        }

        private string m_SoftwareBuildDate;
        public string SoftwareBuildDate
        {
            get { return m_SoftwareBuildDate; }
            set { SetProperty(ref m_SoftwareBuildDate, value); }
        }

        private string m_MachineModel;
        public string MachineModel
        {
            get { return m_MachineModel; }
            set { SetProperty(ref m_MachineModel, value); }
        }

        private string m_SerialNumber;
        public string SerialNumber
        {
            get { return m_SerialNumber; }
            set { SetProperty(ref m_SerialNumber, value); }
        }

        private string m_MacBuildDate;
        public string MacBuildDate
        {
            get { return m_MacBuildDate; }
            set { SetProperty(ref m_MacBuildDate, value); }
        }

        private string m_Current;
        public string Current
        {
            get { return m_Current; }
            set { SetProperty(ref m_Current, value); }
        }

        private string m_Vac;
        public string Vac
        {
            get { return m_Vac; }
            set { SetProperty(ref m_Vac, value); }
        }

        private string m_Frequency;
        public string Frequency
        {
            get { return m_Frequency; }
            set { SetProperty(ref m_Frequency, value); }
        }

        private string m_PowerVA;
        public string PowerVA
        {
            get { return m_PowerVA; }
            set { SetProperty(ref m_PowerVA, value); }
        }

        private string m_SoftwareVersion;
        public string SoftwareVersion
        {
            get { return m_SoftwareVersion; }
            set { SetProperty(ref m_SoftwareVersion, value); }
        }

        private string m_MachName;
        public string MachName
        {
            get { return m_MachName; }
            set { SetProperty(ref m_MachName, value); }
        }

        private string m_CopyRight;
        public string CopyRight
        {
            get { return m_CopyRight; }
            set { SetProperty(ref m_CopyRight, value); }
        }

        private string m_Country;
        public string Country
        {
            get { return m_Country; }
            set { SetProperty(ref m_Country, value); }
        }

        private string m_CompanyAddress;
        public string CompanyAddress
        {
            get { return m_CompanyAddress; }
            set { SetProperty(ref m_CompanyAddress, value); }
        }

        private CultureInfo m_SelectedCulture;
        public CultureInfo SelectedCulture
        {
            get { return m_SelectedCulture; }
            set 
            { 
                SetProperty(ref m_SelectedCulture, value);
                OnLanguageChanged();
            }
        }
        #endregion

        #region Constructor

        public AboutViewModel()
        {
            string exePath = Assembly.GetExecutingAssembly().GetName().CodeBase;
            exePath = exePath.Replace("file:///", string.Empty);
            exePath = exePath.Replace("file://", string.Empty);
            exePath = exePath.Replace("/", @"\");
            FileInfo fi = new FileInfo(exePath);
            SoftwareBuildDate = fi.LastWriteTime.ToString();

            MachName = Global.MachName;
            SoftwareVersion = Global.SoftwareVersion;
            MachineModel = m_SystemConfig.Machine.EquipName;
            SerialNumber = m_SystemConfig.Machine.MachineID;

            Title = GetStringTableValue("About");

            SelectedCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(LocUtil.GetCurrentCultureName());
        }
        #endregion

        #region Method
        private void OnLanguageChanged()
        {
            if (CultureInfo.CurrentUICulture != null && !CultureInfo.CurrentUICulture.Equals(SelectedCulture))
            {
                //change resources to new culture
                m_CultureResources.ChangeCulture(SelectedCulture);

                //save language in registry
                LocUtil.SaveLanguage(SelectedCulture.ToString());

                Title = GetStringTableValue("About");

                //Publish language changed too subscriber
                m_EventAggregator.GetEvent<CultureChanged>().Publish();
            }
        }
        #endregion

        #region Properties
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
        #endregion

    }
}
