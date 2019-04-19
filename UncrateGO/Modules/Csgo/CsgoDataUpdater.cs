using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UncrateGo.Core;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoDataUpdater
    {
        /// <summary>
        /// Overload with object for automated timer
        /// </summary>
        /// <param name="o"></param>
        public static void UpdateRootWeaponSkin(object o)
        {
            UpdateRootWeaponSkin();
        }

        /// <summary>
        /// Fetches item info from API and formats it, replaces the current rootWeaponSkin in memory
        /// </summary>
        public static async void UpdateRootWeaponSkin()
        {
            try
            {
                var skinData = await GetCsgoCosmeticDataOnline();

                if (skinData != null)
                {
                    var processedSkinData = ProcessRawRootSkinData(skinData);

                    //Replace current one in memory
                    CsgoDataHandler.CsgoCosmeticData = processedSkinData;
                }
            }
            catch
            {
                EventLogger.LogMessage("Unable to update csgoCosmeticData", EventLogger.LogLevel.Warning);
            }
        }

        public static CsgoCosmeticData ProcessRawRootSkinData(CsgoCosmeticData csgoWeaponCosmeticInput)
        {
            if (!csgoWeaponCosmeticInput.Processed)
            {
                EventLogger.LogMessage("Formatting CS:GO skin data...");

                //Sort items
                foreach (var skin in csgoWeaponCosmeticInput.ItemsList.Values)
                {
                    //Multiply all prices by 100 to remove decimals on price
                    if (skin.Price != null)
                    {
                        csgoWeaponCosmeticInput.ItemsList[skin.Name].Price.AllTime.Average = skin.Price.AllTime.Average * 100;
                    }
                    else
                    {
                        csgoWeaponCosmeticInput.ItemsList = csgoWeaponCosmeticInput.ItemsList.Where(s => s.Key != skin.Name).ToDictionary(x => x.Key, y => y.Value);
                    }

                    //Sort each skin into corresponding cases
                    //Read from case data config
                    var skinCases = CsgoUnboxingHandler.GetCsgoContainers();

                    //Find the container for each skin
                    foreach (var skinCase in skinCases.Containers)
                    {
                        //Check for each skin in each container
                        foreach (var skinCaseItem in skinCase.ContainerEntries) //TODO Make this more efficient, perhaps only append the certain ones it knows doesn't exist yet
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
                            foreach (var skinComparisonItem in comparisonItems) //TODO errors may be slipping by due to unicode characters, all unicode should be decoded at the source
                            {
                                //If names have unicode, decode them
                                if (UnicodeManager.ContainsUnicodeCharacter(skin.Name))
                                {
                                    skin.Name = UnicodeManager.DecodeToNonAsciiCharacters(skin.Name);
                                }

                                if (skin.Name == skinComparisonItem)
                                {
                                    //If the cases the skin is in or skin.Cases is null, create a new list
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
                csgoWeaponCosmeticInput.Processed = true;

                EventLogger.LogMessage("Formatting CS:GO skin data...DONE!");

                //Return processed input
                return csgoWeaponCosmeticInput;
            }

            //If already processed, just return the original input
            return csgoWeaponCosmeticInput;
        }

        private static async Task<CsgoCosmeticData> GetCsgoCosmeticDataOnline()
        {
            try
            {
                //The API we request info from
                const string requestUrl = "http://csgobackpack.net/api/GetItemsList/v2/";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //For IP-API
                    client.BaseAddress = new Uri(requestUrl);
                    HttpResponseMessage response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var rootSkinData = CsgoCosmeticData.FromJson(result);

                        return rootSkinData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to retrieve CS:GO item info" + ex.Message);
            }

            return null;

        }

        /// <summary>
        /// Gets items available in souvenir version, generates souvenir version and add to container list
        /// </summary>
        public static void GenerateSouvenirCollections()
        {
            //Create a temporary csgoContainers to work with while main one is in a foreach loop
            var csgoContainersTemp = CsgoUnboxingHandler.GetCsgoContainers();

            if (csgoContainersTemp.Containers.Count < 1)
            {
                EventLogger.LogMessage("skinCases.xml not found, containers and collections will not be available", EventLogger.LogLevel.Warning);
            }

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

    }
}
