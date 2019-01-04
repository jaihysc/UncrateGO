using UncrateGo.Core;
using UncrateGo.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Csgo
{
    public class CsgoDataHandler
    {
        public static RootSkinData rootWeaponSkin;

        /// <summary>
        /// Gathers weapon skin data, if it has not been processed, it will process it
        /// </summary>
        /// <returns></returns>
        public static RootSkinData GetRootWeaponSkin()
        {
            if (rootWeaponSkin == null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                EventLogger.LogMessage("Gathering CS:GO skin data, this may take a while");


                RootSkinData rootWeaponSkinTemp = new RootSkinData();
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
                    EventLogger.LogMessage("CS:GO skin data has not been formatted yet, formatting... this WILL take a while");

                    //Sort items
                    foreach (var skin in rootWeaponSkinTemp.ItemsList.Values)
                    {
                        //Multiply all prices by 100 to remove decimals on price
                        if (skin.Price != null)
                        {
                            rootWeaponSkinTemp.ItemsList[skin.Name].Price.AllTime.Average = skin.Price.AllTime.Average * 100;
                        }
                        else
                        {
                            rootWeaponSkinTemp.ItemsList = rootWeaponSkinTemp.ItemsList.Where(s => s.Key != skin.Name).ToDictionary(x => x.Key, y => y.Value);
                        }

                        //Sort each skin into corropsonding cases
                        //Read from case data config
                        var skinCases = CsgoUnboxingHandler.csgoContiners;

                        //Find the container for each skin
                        foreach (var skinCase in skinCases.Containers)
                        {
                            //Check for each skin in each container
                            foreach (var skinCaseItem in skinCase.ContainerEntries)
                            {
                                List<string> comparisonItems = new List<string>();

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

                                else if (skinCase.IsSouvenir && !skinCase.IsSticker)
                                {
                                    comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Factory New)");
                                    comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Minimal Wear)");
                                    comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Field-Tested)");
                                    comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Well-Worn)");
                                    comparisonItems.Add("Souvenir " + skinCaseItem.SkinName + " (Battle-Scarred)");
                                }

                                else if (skinCase.IsSticker)
                                {
                                    comparisonItems.Add("Sticker | " + skinCaseItem.SkinName);
                                    comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Foil)");
                                    comparisonItems.Add("Sticker | " + skinCaseItem.SkinName + " (Holo)");
                                }

                                //Check for possible matches, matching CASE skin name
                                foreach (var comparisonItem in comparisonItems)
                                {
                                    //Use UnicodeLiteralConverter.DecodeToNonAsciiCharacters() before comparason to decode unicode
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
                    rootWeaponSkinTemp.Processed = true;

                    //Write results to skin data file
                    string jsonToWrite = JsonConvert.SerializeObject(rootWeaponSkinTemp);
                    FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("skinData.json"));
                }

                rootWeaponSkin = rootWeaponSkinTemp;

                stopwatch.Stop();
                EventLogger.LogMessage($"Gathering CS:GO skin data, this may take a while --- Done! - Took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
            }


            return rootWeaponSkin;
        }

        public static void RefreshRootWeaponSkin()
        {
            //Clear root Weapon skin
            rootWeaponSkin = null;

            //Get root weapon data again
            GetRootWeaponSkin();
        }

        /// <summary>
        /// Gets items available in souvenir version, generates souvenir version and add to container list
        /// </summary>
        public static void GenerateSouvenirCollections()
        {
            //Create a tempory csgoContainers to work with while main one is in a foreach loop
            var csgoContainersTemp = CsgoUnboxingHandler.csgoContiners;
            foreach (var container in CsgoUnboxingHandler.csgoContiners.Containers.ToList())
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
                        IconURL = container.IconURL,
                        ContainerEntries = container.ContainerEntries
                    };

                    csgoContainersTemp.Containers.Add(souvenirDuplicateContainer);

                }
            }

            CsgoUnboxingHandler.csgoContiners = csgoContainersTemp;

        }

        /// <summary>
        /// Increments the user stat tracker
        /// </summary>
        /// <param name="itemListType"></param>
        public static void IncrementUserStatTracker(SocketCommandContext context, ItemListType itemListType, bool UseOthersCategory = false, bool UseSpecialCategory = false)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Get case stats
            var userCaseStats = userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage;

            if (userCaseStats == null) userCaseStats = new UserCsgoStatsStorage();

            //Increment
            userCaseStats.CasesOpened++;

            if (!UseOthersCategory)
            {
                switch (itemListType.Rarity)
                {
                    case Rarity.ConsumerGrade:
                        userCaseStats.ConsumerGrade++;
                        break;
                    case Rarity.IndustrialGrade:
                        userCaseStats.IndustrialGrade++;
                        break;
                    case Rarity.MilSpecGrade:
                        userCaseStats.MilSpecGrade++;
                        break;
                    case Rarity.Restricted:
                        userCaseStats.Restricted++;
                        break;
                    case Rarity.Classified:
                        userCaseStats.Classified++;
                        break;
                }

                if (itemListType.Rarity == Rarity.Covert && itemListType.BlackListWeaponType == WeaponType.Knife) userCaseStats.Covert++;
                else if (itemListType.Rarity == Rarity.Covert && itemListType.WeaponType == WeaponType.Knife) userCaseStats.Special++;
            }
            else
            {
                userCaseStats.Other++;
            }

            if (UseSpecialCategory) userCaseStats.Special++;

            //Set stats back to master list
            userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage = userCaseStats;

            UserDataManager.WriteUserStorage(userStorage);
        }

        public static async Task DisplayUserStats(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Get case stats
            var userCaseStats = userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage;

            string[] statFields = { "**Cases Opened**", "Consumer Grade", "Industrial Grade", "MilSpec Grade", "Restricted", "Classified", "Covert", "Special", "Other" };

            //Add stats to string list
            List<string> statFieldVal = new List<string>();

            if (userCaseStats != null)
            {
                statFieldVal.Add(userCaseStats.CasesOpened.ToString());
                statFieldVal.Add(userCaseStats.ConsumerGrade.ToString());
                statFieldVal.Add(userCaseStats.IndustrialGrade.ToString());
                statFieldVal.Add(userCaseStats.MilSpecGrade.ToString());
                statFieldVal.Add(userCaseStats.Restricted.ToString());
                statFieldVal.Add(userCaseStats.Classified.ToString());
                statFieldVal.Add(userCaseStats.Covert.ToString());
                statFieldVal.Add(userCaseStats.Special.ToString());
                statFieldVal.Add(userCaseStats.Other.ToString());
            }
            else
            {
                statFieldVal.Add("0");
            }
            

            //Send embed
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(255, 127, 80))
                .WithFooter(footer =>
                {
                    footer
                        .WithText($"Sent by " + context.Message.Author.ToString())
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName(UserInteraction.UserName(context) + " statistics")
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .AddField("\u200b", string.Join("\n", statFields), true)
                .AddField("\u200b", string.Join("\n", statFieldVal), true);

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }
    }
}
