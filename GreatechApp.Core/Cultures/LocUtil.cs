using GreatechApp.Core.Variable;
using Microsoft.Win32;
using System;
using System.Globalization;

namespace GreatechApp.Core.Cultures
{
    public static class LocUtil
    {
        /// <summary>
        /// Get current culture info name base on previously saved setting if any,
        /// otherwise get from OS language
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetCurrentCultureName()
        {
            RegistryKey curLocInfo = Registry.CurrentUser.OpenSubKey("GsmLib" + @"\GreatechApp", false);

            var cultureName = CultureInfo.CurrentUICulture.Name; //Console.WriteLine(cultureName);
            if (curLocInfo != null)
            {
                cultureName = curLocInfo.GetValue("GreatechApp.localization", "en-US").ToString();
            }

            return cultureName;
        }

        /// <summary>
        /// Dynamically load a Localization ResourceDictionary from a file
        /// </summary>
        public static void SaveLanguage(string inFiveCharLang)
        {
            // Save new culture info to registry
            RegistryKey UserPrefs = Registry.CurrentUser.OpenSubKey("GsmLib" + @"\GreatechApp", true);

            if (UserPrefs == null)
            {
                // Value does not already exist so create it
                RegistryKey newKey = Registry.CurrentUser.CreateSubKey("GsmLib");
                UserPrefs = newKey.CreateSubKey("GreatechApp");
            }

            UserPrefs.SetValue("GreatechApp.localization", inFiveCharLang);
        }
    }

}
