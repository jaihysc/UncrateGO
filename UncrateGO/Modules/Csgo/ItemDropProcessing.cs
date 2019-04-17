using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace UncrateGo.Modules.Csgo
{
    internal static class ItemDropProcessing
    {
        static Random rand = new Random();

        public static ItemListType CalculateItemCaseRarity()
        {
            int randomNumber = rand.Next(9999);

            if (randomNumber < 10000 && randomNumber >= 2008) return new ItemListType { Rarity = Rarity.MilSpecGrade };
            if (randomNumber < 2008 && randomNumber >= 410) return new ItemListType { Rarity = Rarity.Restricted };
            if (randomNumber < 410 && randomNumber >= 90) return new ItemListType { Rarity = Rarity.Classified };
            if (randomNumber < 90 && randomNumber >= 26) return new ItemListType { Rarity = Rarity.Covert, BlackListWeaponType = WeaponType.Knife };
            if (randomNumber < 26 && randomNumber >= 0) return new ItemListType { Rarity = Rarity.Covert, WeaponType = WeaponType.Knife };

            return new ItemListType { Rarity = Rarity.MilSpecGrade };
        }

        public static ItemListType CalculateItemDropRarity()
        {
            int randomNumber = rand.Next(9999);

            if (randomNumber < 10000 && randomNumber >= 2008) return new ItemListType { Rarity = Rarity.ConsumerGrade };
            if (randomNumber < 2008 && randomNumber >= 410) return new ItemListType { Rarity = Rarity.IndustrialGrade };
            if (randomNumber < 410 && randomNumber >= 90) return new ItemListType { Rarity = Rarity.MilSpecGrade };
            if (randomNumber < 90 && randomNumber >= 26) return new ItemListType { Rarity = Rarity.Restricted };
            if (randomNumber < 26 && randomNumber >= 0) return new ItemListType { Rarity = Rarity.Classified };

            return new ItemListType { Rarity = Rarity.ConsumerGrade };
        }

        /// <summary>
        /// Fetches and randomly retrieves a skin item of specified type
        /// </summary>
        /// <param name="itemListType">Type file</param>
        /// <param name="cosmeticData">Skin data to look through</param>
        /// <param name="context"></param>
        /// <param name="byPassCaseFilter"></param>
        /// <returns></returns>
        public static SkinDataItem GetItem(ItemListType itemListType, CsgoCosmeticData cosmeticData, SocketCommandContext context, bool byPassCaseFilter)
        {
            var sortedResult = new List<KeyValuePair<string, SkinDataItem>>();

            //Get user from dictionary
            if (!CsgoUnboxingHandler.UserSelectedCase.TryGetValue(context.Message.Author.Id, out var userSelectedCaseName))
            {
                //Default to danger zone case if user has not made a selection
                CsgoUnboxingHandler.UserSelectedCase.Add(context.Message.Author.Id, "Danger Zone Case");
            }

            //Filter skins to those in user's case
            string selectedCase = CsgoUnboxingHandler.GetCsgoContainers().Containers.Where(s => s.Name == CsgoUnboxingHandler.UserSelectedCase[context.Message.Author.Id]).Select(s => s.Name).FirstOrDefault();

            //Add skins matching user's case to sorted result
            if (byPassCaseFilter == false)
            {
                //Find items matching filter case criteria, add to sortedResult ...!!!!Store this in the future to make this process more efficient
                foreach (KeyValuePair<string, SkinDataItem> item in cosmeticData.ItemsList)
                {
                    if (item.Value.Cases != null)
                    {
                        foreach (var item2 in item.Value.Cases)
                        {
                            if (item2.CaseName == selectedCase)
                            {
                                sortedResult.Add(item);
                            }
                        }
                    }
                }
            }
            else
            {
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
            }

            Container designatedStickerItem = CsgoUnboxingHandler.GetCsgoContainers().Containers.FirstOrDefault(i => i.Name == userSelectedCaseName);
            bool itemIsSticker = false;
            if (designatedStickerItem != null)
            {
                itemIsSticker = designatedStickerItem.IsSticker;
            }
            //Filter by rarity if not a sticker
            if (!itemIsSticker)
            {
                sortedResult = sortedResult.Where(s => s.Value.Rarity == itemListType.Rarity).ToList();
            }

            //If weaponType is not null, filter by weapon type
            if (itemListType.WeaponType != null)
            {
                sortedResult = sortedResult
                    .Where(s => s.Value.WeaponType == itemListType.WeaponType).ToList();
            }

            //If blackListWeaponType is not null, filter by weapon type
            if (itemListType.BlackListWeaponType != null)
            {
                sortedResult = sortedResult
                    .Where(s => s.Value.WeaponType != itemListType.BlackListWeaponType).ToList();
            }

            //If case is not a souvenir, filter out souvenir items, if it is, filter out non souvenir items
            var caseItem = CsgoUnboxingHandler.GetCsgoContainers().Containers
                .FirstOrDefault(c => c.Name == selectedCase);
            if (caseItem != null && (byPassCaseFilter == false && caseItem.IsSouvenir))
            {
                //True
                sortedResult = sortedResult.Where(s => s.Value.Name.ToLower().Contains("souvenir")).ToList();
            }
            else
            {
                //False
                sortedResult = sortedResult.Where(s => !s.Value.Name.ToLower().Contains("souvenir")).ToList();
            }


            //Filter out stattrak
            sortedResult = sortedResult
                .Where(s => !s.Value.Name.ToLower().Contains("stattrak")).ToList();


            //Randomly select a skin from the filtered list of possible skins
            if (sortedResult.Any())
            {
                KeyValuePair<string, SkinDataItem> selectedSkin = sortedResult[rand.Next(sortedResult.Count())];

                bool giveStatTrak = CalculateStatTrakDrop();
                //Give stattrak
                if (giveStatTrak)
                {
                    KeyValuePair<string, SkinDataItem> selectedStatTrakItem = cosmeticData.ItemsList
                        .Where(s => s.Value.Name.ToLower().Contains(selectedSkin.Value.Name.ToLower()))
                        .Where(s => s.Value.Name.ToLower().Contains("stattrak")).FirstOrDefault();

                    //If filter was unsuccessful at finding stattrak, do not assign item
                    if (selectedStatTrakItem.Value != null)
                    {
                        selectedSkin = selectedStatTrakItem;
                    }
                }

                bool itemIsSouvenir = CsgoUnboxingHandler.GetCsgoContainers().Containers.Where(i => i.Name == userSelectedCaseName).FirstOrDefault().IsSouvenir;
                //Increment stats counter
                //Sticker
                if (itemIsSticker && !byPassCaseFilter)
                {
                    CsgoLeaderboardManager.IncrementCaseStatTracker(context, CsgoLeaderboardManager.CaseCategory.Sticker);
                    CsgoLeaderboardManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardManager.ItemCategory.Sticker);
                }
                //Souvenir
                else if (itemIsSouvenir && !byPassCaseFilter)
                {
                    CsgoLeaderboardManager.IncrementCaseStatTracker(context, CsgoLeaderboardManager.CaseCategory.Souvenir);
                    CsgoLeaderboardManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardManager.ItemCategory.Default);
                }
                //Case
                else if (!byPassCaseFilter)
                {
                    CsgoLeaderboardManager.IncrementCaseStatTracker(context, CsgoLeaderboardManager.CaseCategory.Case);
                    CsgoLeaderboardManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardManager.ItemCategory.Default);
                }
                //Drop
                else if (byPassCaseFilter)
                {
                    CsgoLeaderboardManager.IncrementCaseStatTracker(context, CsgoLeaderboardManager.CaseCategory.Drop);
                    CsgoLeaderboardManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardManager.ItemCategory.Default);
                }
                else
                {
                    CsgoLeaderboardManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardManager.ItemCategory.Other);
                }

                return selectedSkin.Value;
            }
            else //Randomly pick a skin out of everything if it was unable to find one
            {
                List<KeyValuePair<string, SkinDataItem>> sortedResult2 = cosmeticData.ItemsList.Where(s => s.Value.Rarity == Rarity.MilSpecGrade).ToList();
                KeyValuePair<string, SkinDataItem> selectedSkin = sortedResult2[rand.Next(sortedResult2.Count())];

                return selectedSkin.Value;
            }           
        }

        private static bool CalculateStatTrakDrop()
        {
            if (rand.Next(9) == 0)
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