using System;
using System.Collections.Generic;
using System.Linq;

namespace UncrateGo.Core
{
    public class SettingsManager
    {
        /// <summary>
        /// Returns the value of input config item as string
        /// </summary>
        /// <param name="settingName">Name of the config item</param>
        /// <returns></returns>
        public static string RetrieveFromConfigFile(string settingName)
        {
            try
            {
                //Get settings
                var retrievedSettings = XmlManager.FromXmlFile<SettingsConfig>("Settings.xml");

                //Filter settings by name
                var filteredRetrievedSettings = retrievedSettings.Settings.Where(s => s.SettingName == settingName).ToList();

                //Return filtered setting value
                return filteredRetrievedSettings.FirstOrDefault().SettingValue;
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Writes input setting and setting value to setting file; Will overwrite existing setting if name exists
        /// </summary>
        /// <param name="settingName">Name of the setting to add/overwrite</param>
        /// <param name="settingValue">Value of setting to add/overwrite</param>
        public static void WriteToConfigFile(string settingName, string settingValue)
        {
            try
            {
                //Get settings
                var retrievedSettings = XmlManager.FromXmlFile<SettingsConfig>("Settings.xml");

                //Filter out settings not selected
                var filteredSettings = retrievedSettings.Settings.Where(s => s.SettingName != settingName).ToList();

                //Add new setting to filtered list
                filteredSettings.Add(new Setting { SettingName = settingName, SettingValue = settingValue });

                //Write new setting with old ones
                var settingsConfig = new SettingsConfig
                {
                    Settings = filteredSettings
                };

                XmlManager.ToXmlFile(settingsConfig, "Settings.xml");
            }
            catch (Exception)
            {
            }

        }
    }

    public class SettingsConfig
    {
        public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
