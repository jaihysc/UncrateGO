using UncrateGo.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoDataHandler
    {
        private static CsgoCosmeticData _csgoCosmeticData;
        private static UserSkinStorage _userSkinStorage;

        /// <summary>
        /// Gathers weapon skin data, if it has not been processed, it will process it
        /// </summary>
        /// <returns></returns>
        public static CsgoCosmeticData GetCsgoCosmeticData()
        {
            if (_csgoCosmeticData == null)
            {
                EventLogger.LogMessage("Gathering CS:GO cosmetic data, this may take a while");

                //Read skin data from local json file
                string path = FileManager.GetFileLocation("skinData.json");
                if (File.Exists(path))
                {
                    CsgoCosmeticData csgoWeaponCosmeticTemp;
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        var rootWeaponSkin = CsgoCosmeticData.FromJson(json);

                        csgoWeaponCosmeticTemp = rootWeaponSkin;
                    }

                    //It json has not been formatted yet for use, format it
                    if (!csgoWeaponCosmeticTemp.Processed)
                    {
                        //Format it
                        csgoWeaponCosmeticTemp = CsgoDataUpdater.ProcessRawRootSkinData(csgoWeaponCosmeticTemp);

                        //Write results to skin data file
                        string jsonToWrite = JsonConvert.SerializeObject(csgoWeaponCosmeticTemp);
                        FileManager.WriteStringToFile(jsonToWrite, true, FileManager.GetFileLocation("skinData.json"));
                    }

                    _csgoCosmeticData = csgoWeaponCosmeticTemp;

                }
                else //If a json file containing the skins is not found, fetch it from online
                {
                    EventLogger.LogMessage($"CS:GO cosmetic data not found in local json file, fetching from online...");

                    CsgoDataUpdater.UpdateRootWeaponSkin();
                }

                EventLogger.LogMessage($"Gathering CS:GO cosmetic data, this may take a while --- Done!");
            }

            return _csgoCosmeticData;
        }

        public static void SetCsgoCosmeticData(CsgoCosmeticData cosmeticData)
        {
            _csgoCosmeticData = cosmeticData;
        }


        /// <summary>
        /// Gets the case selection of a specified user, if not existent, it sets it to danger zone case
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetUserSelectedCaseName(ulong userId)
        {
            if (!CsgoUnboxingHandler.UserSelectedCase.TryGetValue(userId, out string userSelectedCaseName))
            {
                //Default to danger zone case if user has not made a selection
                CsgoUnboxingHandler.UserSelectedCase.Add(userId, "Danger Zone Case");
            }

            return userSelectedCaseName;
        }

        public static List<UserSkinEntry> GetUserItems(ulong userId)
        {
            var userSkinStorage = GetUserSkinStorage();

            List<UserSkinEntry> foundUserItems = userSkinStorage.UserSkinEntries
                .Where(s => s.OwnerId == userId).ToList();

            return foundUserItems;
        }

        /// <summary>
        /// Gets the container of the specified name
        /// </summary>
        /// <param name="caseName"></param>
        /// <returns></returns>
        public static Container GetCsgoCase(string caseName)
        {
            return CsgoUnboxingHandler.GetCsgoContainers().Containers.FirstOrDefault(i => i.Name == caseName);
        }

        /// <summary>
        /// Gets the user skin storage
        /// </summary>
        /// <returns></returns>
        public static UserSkinStorage GetUserSkinStorage()
        {
            //Read from file if this is null
            if (_userSkinStorage == null)
            {
                var userSkin = JsonConvert.DeserializeObject<UserSkinStorage>(FileManager.ReadFromFile(FileManager.GetFileLocation("UserSkinStorage.json")));

                if (userSkin == null)
                {
                    userSkin = new UserSkinStorage
                    {
                        UserSkinEntries = new List<UserSkinEntry>()
                    };

                    EventLogger.LogMessage("UserSkinStorage.json not found, creating one", EventLogger.LogLevel.Info);
                }

                _userSkinStorage = userSkin;
            }

            return _userSkinStorage;
        }


        /// <summary>
        /// Adds the specified item to the user inventory
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="skinItem"></param>
        public static void AddItemToUserInventory(ulong userId, SkinDataItem skinItem) //TODO, add item limit of n to prevent bot abuse
        {
            var userSkin = GetUserSkinStorage();
            userSkin.UserSkinEntries.Add(new UserSkinEntry
            {
                ClassId = skinItem.Classid,
                OwnerId = userId,
                UnboxDate = DateTime.UtcNow, MarketName = skinItem.Name
            });
        }

        /// <summary>
        /// Sets the current user skin storage to the input
        /// </summary>
        /// <param name="input"></param>
        public static void SetUserSkinStorage(UserSkinStorage input)
        {
            _userSkinStorage = input;
        }

        /// <summary>
        /// Writes to the user skin storage
        /// </summary>
        public static void FlushUserSkinStorage()
        {
            try
            {
                var tempUserSkinStorage = _userSkinStorage;
                var json = JsonConvert.SerializeObject(tempUserSkinStorage);
                FileManager.WriteStringToFile(json, true, FileManager.GetFileLocation("UserSkinStorage.json"));
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to flush user skin storage", EventLogger.LogLevel.Error);
            }

        }
    }
}
