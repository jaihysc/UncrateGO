using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace UncrateGo.Modules.Csgo
{
    internal static class CsgoDropProcessing
    {
        private static readonly Random Rand = new Random();

        public static ItemData CalculateItemCaseRarity()
        {
            int randomNumber = Rand.Next(9999);

            if (randomNumber < 10000 && randomNumber >= 2008) return new ItemData { Rarity = Rarity.MilSpecGrade };
            if (randomNumber < 2008 && randomNumber >= 410) return new ItemData { Rarity = Rarity.Restricted };
            if (randomNumber < 410 && randomNumber >= 90) return new ItemData { Rarity = Rarity.Classified };
            if (randomNumber < 90 && randomNumber >= 26) return new ItemData { Rarity = Rarity.Covert, BlackListWeaponType = WeaponType.Knife };
            if (randomNumber < 26 && randomNumber >= 0) return new ItemData { Rarity = Rarity.Covert, WeaponType = WeaponType.Knife };

            return new ItemData { Rarity = Rarity.MilSpecGrade };
        }

        public static ItemData CalculateItemDropRarity()
        {
            int randomNumber = Rand.Next(9999);

            if (randomNumber < 10000 && randomNumber >= 2008) return new ItemData { Rarity = Rarity.ConsumerGrade };
            if (randomNumber < 2008 && randomNumber >= 410) return new ItemData { Rarity = Rarity.IndustrialGrade };
            if (randomNumber < 410 && randomNumber >= 90) return new ItemData { Rarity = Rarity.MilSpecGrade };
            if (randomNumber < 90 && randomNumber >= 26) return new ItemData { Rarity = Rarity.Restricted };
            if (randomNumber < 26 && randomNumber >= 0) return new ItemData { Rarity = Rarity.Classified };

            return new ItemData { Rarity = Rarity.ConsumerGrade };
        }

        /// <summary>
        /// Fetches and randomly retrieves a skin item of specified type
        /// </summary>
        /// <param name="itemData">Type file</param>
        /// <param name="cosmeticData">Skin data to look through</param>
        /// <param name="context"></param>
        /// <param name="getCollectionItems"></param>
        /// <returns></returns>
        public static SkinDataItem GetItem(ItemData itemData, CsgoCosmeticData cosmeticData, SocketCommandContext context, bool getCollectionItems)
        {
            string userSelectedCaseName = CsgoDataHandler.GetUserSelectedCaseName(context.Message.Author.Id);

            //Add skins matching user's case to sorted result

            //Get items in user's case or all collection
            List<KeyValuePair<string, SkinDataItem>> sortedResult = !getCollectionItems
                ? GetItemsInCase(cosmeticData,
                    userSelectedCaseName)
                : GetItemsInCollections(cosmeticData);

            Container caseContainer = CsgoDataHandler.GetCsgoCase(userSelectedCaseName);

            //Filter by rarity if not a sticker
            if (!caseContainer.IsSticker)
            {
                sortedResult = sortedResult.Where(s => s.Value.Rarity == itemData.Rarity).ToList();
            }

            //If weaponType is not null, filter by weapon type
            if (itemData.WeaponType != null)
            {
                sortedResult = sortedResult
                    .Where(s => s.Value.WeaponType == itemData.WeaponType).ToList();
            }

            //If blackListWeaponType is not null, filter by weapon type
            if (itemData.BlackListWeaponType != null)
            {
                sortedResult = sortedResult
                    .Where(s => s.Value.WeaponType != itemData.BlackListWeaponType).ToList();
            }

            //If case is not a souvenir, filter out souvenir items, if it is, filter out non souvenir items
            sortedResult = getCollectionItems == false && caseContainer.IsSouvenir
                ? sortedResult.Where(s => s.Value.Name.ToLower().Contains("souvenir")).ToList()
                : sortedResult.Where(s => !s.Value.Name.ToLower().Contains("souvenir")).ToList();


            //Filter out stattrak
            sortedResult = sortedResult
                .Where(s => !s.Value.Name.ToLower().Contains("stattrak")).ToList();


            //Randomly select a skin from the filtered list of possible skins
            if (sortedResult.Any())
            {
                KeyValuePair<string, SkinDataItem> selectedSkin = sortedResult[Rand.Next(sortedResult.Count())];

                bool giveStatTrak = CalculateStatTrakDrop();
                //Give stattrak
                if (giveStatTrak)
                {
                    KeyValuePair<string, SkinDataItem> selectedStatTrakItem = cosmeticData.ItemsList
                        .Where(s => s.Value.Name.ToLower().Contains(selectedSkin.Value.Name.ToLower()))
                        .FirstOrDefault(s => s.Value.Name.ToLower().Contains("stattrak"));

                    //If filter was unsuccessful at finding stattrak, keep to the non stattrak variant
                    if (selectedStatTrakItem.Value != null)
                    {
                        selectedSkin = selectedStatTrakItem;
                    }
                }

                //Increment stats counter
                //Sticker
                if (caseContainer.IsSticker && !getCollectionItems)
                {
                    CsgoLeaderboardManager.IncrementStatsCounter(context.Message.Author.Id,
                        CsgoLeaderboardManager.StatItemType.Sticker, itemData);
                }
                //Souvenir
                else if (caseContainer.IsSouvenir && !getCollectionItems)
                {
                    CsgoLeaderboardManager.IncrementStatsCounter(context.Message.Author.Id,
                        CsgoLeaderboardManager.StatItemType.Souvenir, itemData);
                }
                //Case
                else if (!getCollectionItems)
                {
                    CsgoLeaderboardManager.IncrementStatsCounter(context.Message.Author.Id,
                        CsgoLeaderboardManager.StatItemType.Case, itemData);
                }
                //Drop
                else
                {
                    CsgoLeaderboardManager.IncrementStatsCounter(context.Message.Author.Id,
                        CsgoLeaderboardManager.StatItemType.Drop, itemData);
                }

                return selectedSkin.Value;
            }
            else //Randomly pick a skin out of everything if it was unable to find one
            {
                List<KeyValuePair<string, SkinDataItem>> sortedResult2 = cosmeticData.ItemsList.Where(s => s.Value.Rarity == Rarity.MilSpecGrade).ToList();
                KeyValuePair<string, SkinDataItem> selectedSkin = sortedResult2[Rand.Next(sortedResult2.Count())];

                return selectedSkin.Value;
            }           
        }

        /// <summary>
        /// Gets the items inside case
        /// </summary>
        /// <param name="cosmeticData"></param>
        /// <param name="caseName"></param>
        /// <returns></returns>
        private static List<KeyValuePair<string, SkinDataItem>> GetItemsInCase(CsgoCosmeticData cosmeticData, string caseName)
        {
            //Add skins matching user's case to sorted result
            var sortedResult = new List<KeyValuePair<string, SkinDataItem>>();

            //Find items matching filter case criteria, add to sortedResult TODO Store this in the future to make this process more efficient
            foreach (KeyValuePair<string, SkinDataItem> item in cosmeticData.ItemsList)
            {
                if (item.Value.Cases != null)
                {
                    foreach (var item2 in item.Value.Cases)
                    {
                        if (item2.CaseName == caseName)
                        {
                            sortedResult.Add(item);
                        }
                    }
                }
            }

            return sortedResult;
        }

        /// <summary>
        /// Gets the items inside all collections
        /// </summary>
        /// <param name="cosmeticData"></param>
        /// <returns></returns>
        private static List<KeyValuePair<string, SkinDataItem>> GetItemsInCollections(CsgoCosmeticData cosmeticData)
        {
            //Add skins matching user's case to sorted result
            var sortedResult = new List<KeyValuePair<string, SkinDataItem>>();

            //If bypass is true, sorted result is just root cosmeticData
            //sortedResult = cosmeticData.ItemsList.ToDictionary(x => x.Key, y => y.Value).ToList();

            //Add collection items, E.g Mirage collection, Nuke collection for drop, which has null for casesName
            foreach (KeyValuePair<string, SkinDataItem> item in cosmeticData.ItemsList)
            {
                if (item.Value.Cases != null)
                {
                    foreach (var item2 in item.Value.Cases)
                    {
                        if (item2.CaseName == null)
                        {
                            sortedResult.Add(item);
                        }
                    }
                }
            }

            return sortedResult;
        }

        private static bool CalculateStatTrakDrop()
        {
            if (Rand.Next(9) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}