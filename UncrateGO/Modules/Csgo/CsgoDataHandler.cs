﻿using UncrateGo.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoDataHandler
    {
        public static RootSkinData RootWeaponSkin;
        private static UserSkinStorage _userSkinStorage;

        /// <summary>
        /// Gathers weapon skin data, if it has not been processed, it will process it
        /// </summary>
        /// <returns></returns>
        public static RootSkinData GetRootWeaponSkin()
        {
            if (RootWeaponSkin == null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                EventLogger.LogMessage("Gathering CS:GO skin data, this may take a while");


                RootSkinData rootWeaponSkinTemp;
                //Read skin data from local json file
                using (StreamReader r = new StreamReader(FileAccessManager.GetFileLocation("skinData.json")))
                {
                    string json = r.ReadToEnd();
                    var rootWeaponSkin = RootSkinData.FromJson(json);

                    rootWeaponSkinTemp = rootWeaponSkin;
                }

                //It json has not been formatted yet for use, format it
                if (!rootWeaponSkinTemp.Processed)
                {
                    //Format it
                    rootWeaponSkinTemp = ProcessRawRootSkinData(rootWeaponSkinTemp);

                    //Write results to skin data file
                    string jsonToWrite = JsonConvert.SerializeObject(rootWeaponSkinTemp);
                    FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("skinData.json"));
                }

                RootWeaponSkin = rootWeaponSkinTemp;

                stopwatch.Stop();
                EventLogger.LogMessage($"Gathering CS:GO skin data, this may take a while --- Done! - Took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
            }


            return RootWeaponSkin;
        }

        /// <summary>
        /// Fetches item info from API and formats it, replaces the current rootWeaponSkin in memory
        /// </summary>
        public static async void UpdateRootWeaponSkin(object state)
        {
            try
            {
                var skinData = await GetRootSkinDataOnline();

                if (skinData != null)
                {
                    var processedSkinData = ProcessRawRootSkinData(skinData);

                    //Replace current one in memory
                    RootWeaponSkin = processedSkinData;
                }
            }
            catch
            {
                EventLogger.LogMessage("Unable to update rootSkinData", ConsoleColor.Red);
            }
        }

        private static RootSkinData ProcessRawRootSkinData(RootSkinData rootWeaponSkinInput)
        {
            if (!rootWeaponSkinInput.Processed)
            {
                EventLogger.LogMessage("Formatting CS:GO skin data...");

                //Sort items
                foreach (var skin in rootWeaponSkinInput.ItemsList.Values)
                {
                    //Multiply all prices by 100 to remove decimals on price
                    if (skin.Price != null)
                    {
                        rootWeaponSkinInput.ItemsList[skin.Name].Price.AllTime.Average = skin.Price.AllTime.Average * 100;
                    }
                    else
                    {
                        rootWeaponSkinInput.ItemsList = rootWeaponSkinInput.ItemsList.Where(s => s.Key != skin.Name).ToDictionary(x => x.Key, y => y.Value);
                    }

                    //Sort each skin into corresponding cases
                    //Read from case data config
                    var skinCases = CsgoUnboxingHandler.GetCsgoContainers();

                    //Find the container for each skin
                    foreach (var skinCase in skinCases.Containers) //TODO, make this process way more efficient
                    {
                        //Check for each skin in each container
                        foreach (var skinCaseItem in skinCase.ContainerEntries)
                        {
                            var comparisonItems = new List<string>();

                            //if FN, MW, ETC, it will find all skin conditions + stattrak

                            //For above, append statements for wear 
                            if (!skinCase.IsSouvenir && !skinCase.IsSticker)
                            {
                                comparisonItems.Add(skinCaseItem.SkinName + " (Factory New)");
                                comparisonItems.Add(skinCaseItem.SkinName + " (Minimal Wear)");
                                comparisonItems.Add(skinCaseItem.SkinName + " (Field-Tested)");
                                comparisonItems.Add(skinCaseItem.SkinName + " (Well-Worn)");
                                comparisonItems.Add(skinCaseItem.SkinName + " (Battle-Scarred)");

                                //Add StatTrak\u2122 before to check for stattrak
                                comparisonItems.Add("StatTrak\u2122 " + skinCaseItem.SkinName + " (Factory New)");
                                comparisonItems.Add("StatTrak\u2122 " + skinCaseItem.SkinName + " (Minimal Wear)");
                                comparisonItems.Add("StatTrak\u2122 " + skinCaseItem.SkinName + " (Field-Tested)");
                                comparisonItems.Add("StatTrak\u2122 " + skinCaseItem.SkinName + " (Well-Worn)");
                                comparisonItems.Add("StatTrak\u2122 " + skinCaseItem.SkinName + " (Battle-Scarred)");

                                //KNIVES

                                //\u2605 for knives
                                comparisonItems.Add("\u2605 " + skinCaseItem.SkinName + " (Factory New)");
                                comparisonItems.Add("\u2605 " + skinCaseItem.SkinName + " (Minimal Wear)");
                                comparisonItems.Add("\u2605 " + skinCaseItem.SkinName + " (Field-Tested)");
                                comparisonItems.Add("\u2605 " + skinCaseItem.SkinName + " (Well-Worn)");
                                comparisonItems.Add("\u2605 " + skinCaseItem.SkinName + " (Battle-Scarred)");

                                //\u2605 StatTrak\u2122 for knife stattrak
                                comparisonItems.Add("\u2605 StatTrak\u2122 " + skinCaseItem.SkinName + " (Factory New)");
                                comparisonItems.Add("\u2605 StatTrak\u2122 " + skinCaseItem.SkinName + " (Minimal Wear)");
                                comparisonItems.Add("\u2605 StatTrak\u2122 " + skinCaseItem.SkinName + " (Field-Tested)");
                                comparisonItems.Add("\u2605 StatTrak\u2122 " + skinCaseItem.SkinName + " (Well-Worn)");
                                comparisonItems.Add("\u2605 StatTrak\u2122 " + skinCaseItem.SkinName + " (Battle-Scarred)");
                            }

                            //Souvenir non sticker
                            else if (skinCase.IsSouvenir && !skinCase.IsSticker)
                            {
                                comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Factory New)");
                                comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Minimal Wear)");
                                comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Field-Tested)");
                                comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Well-Worn)");
                                comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Battle-Scarred)");
                            }

                            //not a tournament sticker
                            else if (skinCase.IsSticker && !skinCase.IsTournamentSticker)
                            {
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName);
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Foil)");
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Holo)");
                            }

                            //is tournament sticker
                            else if (skinCase.IsSticker && skinCase.IsTournamentSticker)
                            {
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " | " + skinCase.CollectionName);
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Foil)" + " | " + skinCase.CollectionName);
                                comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Holo)" + " | " + skinCase.CollectionName);
                            }

                            //Check for possible matches, matching CASE skin name
                            foreach (var comparisonItem in comparisonItems)
                            {
                                //Use UnicodeLiteralConverter.DecodeToNonAsciiCharacters() before comparison to decode unicode
                                if (UnicodeLiteralConverter.DecodeToNonAsciiCharacters(skin.Name) == UnicodeLiteralConverter.DecodeToNonAsciiCharacters(comparisonItem))
                                {
                                    //If skin.Cases is null, create a new list
                                    if (skin.Cases == null) skin.Cases = new List<Case>();

                                    //If item matches, set the cases property of the item to current name of the case it is checking
                                    skin.Cases.Add(new Case
                                    {
                                        CaseName = skinCase.Name,
                                        CaseCollection = skinCase.CollectionName
                                    });
                                    break;
                                }
                            }
                        }

                    }
                }
                rootWeaponSkinInput.Processed = true;

                EventLogger.LogMessage("Formatting CS:GO skin data...DONE!");

                //Return processed input
                return rootWeaponSkinInput;
            }

            //If already processed, just return the original input
            return rootWeaponSkinInput;
        }

        private static async Task<RootSkinData> GetRootSkinDataOnline()
        {
            try
            {
                //The API we request info from
                const string requestUrl = "http://csgobackpack.net/api/GetItemsList/v2/";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    //For IP-API
                    client.BaseAddress = new Uri(requestUrl);
                    HttpResponseMessage response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var rootSkinData = RootSkinData.FromJson(result);

                        return rootSkinData;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to retrieve CS:GO item info" + ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Gets items available in souvenir version, generates souvenir version and add to container list
        /// </summary>
        public static void GenerateSouvenirCollections()
        {
            //Create a temporary csgoContainers to work with while main one is in a foreach loop
            var csgoContainersTemp = CsgoUnboxingHandler.GetCsgoContainers();
            foreach (var container in CsgoUnboxingHandler.GetCsgoContainers().Containers.ToList())
            {
                if (container.SouvenirAvailable)
                {
                    //If item has available souvenir version, generate souvenir version and add to container list
                    var souvenirDuplicateContainer = new Container
                    {
                        IsSouvenir = true,
                        SouvenirAvailable = false,
                        Name = container.CollectionName + " Souvenir",
                        CollectionName = container.CollectionName + " Souvenir",
                        IconUrl = container.IconUrl,
                        ContainerEntries = container.ContainerEntries
                    };

                    csgoContainersTemp.Containers.Add(souvenirDuplicateContainer);

                }
            }

            CsgoUnboxingHandler.SetCsgoContainers(csgoContainersTemp);

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
                var userSkin = JsonConvert.DeserializeObject<UserSkinStorage>(FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("UserSkinStorage.json")));

                if (userSkin == null)
                {
                    userSkin = new UserSkinStorage
                    {
                        UserSkinEntries = new List<UserSkinEntry>()
                    };

                }

                _userSkinStorage = userSkin;
            }

            return _userSkinStorage;
        }

        /// <summary>
        /// Adds the specified item to the user inventory
        /// </summary>
        /// <param name="context"></param>
        /// <param name="skinItem"></param>
        public static void AddItemToUserInventory(SocketCommandContext context, SkinDataItem skinItem)
        {
            var userSkin = GetUserSkinStorage();
            userSkin.UserSkinEntries.Add(new UserSkinEntry
            {
                ClassId = skinItem.Classid,
                OwnerId = context.Message.Author.Id,
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
                FileAccessManager.WriteStringToFile(json, true, FileAccessManager.GetFileLocation("UserSkinStorage.json"));
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to flush user skin storage", ConsoleColor.Red);
            }

        }

    }
}
