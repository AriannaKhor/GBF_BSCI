using GreatechApp.Core.Variable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace GreatechApp.Core.Cultures
{
    /// <summary>
    /// Wraps up XAML access to instance of WPFLocalize.Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources
    {
        //only fetch installed cultures once
        private bool bFoundInstalledCultures = false;

        private List<CultureInfo> pSupportedCultures = new List<CultureInfo>();
        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public List<CultureInfo> SupportedCultures
        {
            get { return pSupportedCultures; }
        }

        public CultureResources()
        {
            pSupportedCultures.Add(CultureInfo.GetCultureInfo("en-US"));
            if (!bFoundInstalledCultures)
            {
                //determine which cultures are available to this application
                Debug.WriteLine("Get Installed cultures:");
                CultureInfo tCulture = new CultureInfo("");
                foreach (string dir in Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory))
                {
                    try
                    {
                        //see if this directory corresponds to a valid culture name
                        DirectoryInfo dirinfo = new DirectoryInfo(dir);
                        tCulture = CultureInfo.GetCultureInfo(dirinfo.Name);

                        //determine if a resources dll exists in this directory that matches the executable name
                        if (dirinfo.GetFiles(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".resources.dll").Length > 0)
                        {
                            pSupportedCultures.Add(tCulture);
                            Debug.WriteLine(string.Format(" Found Culture: {0} [{1}]", tCulture.DisplayName, tCulture.Name));
                        }
                    }
                    catch (ArgumentException) //ignore exceptions generated for any unrelated directories in the bin folder
                    {
                    }
                }
                bFoundInstalledCultures = true;
            }
        }

        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the WPFLocalize.Properties.Resources class
        /// </summary>
        /// <returns></returns>
        public StringTableResource GetStringTable()
        {
            return new StringTableResource();
        }

        public DialogTableResource GetDialogTable()
        {
            return new DialogTableResource();
        }

        public string GetStringValue(string key)
        {
            return new StringTableResource().GetValue(key);
        }

        public string GetDialogValue(string key)
        {
            return new DialogTableResource().GetValue(key);
        }

        private ObjectDataProvider m_StringTableODP;
        public ObjectDataProvider StringTableODP
        {
            get
            {
                if (m_StringTableODP == null)
                    m_StringTableODP = (ObjectDataProvider)Application.Current.FindResource("StringTableODP");
                return m_StringTableODP;
            }
        }

        private ObjectDataProvider m_DialogTableODP;
        public ObjectDataProvider DialogTableODP
        {
            get
            {
                if (m_DialogTableODP == null)
                    m_DialogTableODP = (ObjectDataProvider)Application.Current.FindResource("DialogTableODP");
                return m_DialogTableODP;
            }
        }


        /// <summary>
        /// Change the current culture used in the application.
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        /// <param name="culture">Culture to change to</param>
        public void ChangeCulture(CultureInfo culture)
        {
            //remain on the current culture if the desired culture cannot be found
            // - otherwise it would revert to the default resources set, which may or may not be desired.
            if (pSupportedCultures.Contains(culture))
            {
                CultureInfo.CurrentUICulture = culture;
                Global.CurrentCulture = culture;
                //System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                StringTableODP.Refresh();
                DialogTableODP.Refresh();
            }
            else
                Debug.WriteLine(string.Format("Culture [{0}] not available", culture));
        }
    }
}
