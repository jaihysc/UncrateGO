using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UncrateGo.Modules.Csgo
{
    public class CsgoUnboxingHandler : InteractiveBase<SocketCommandContext>
    {
        public static Dictionary<ulong, string> UserSelectedCase = new Dictionary<ulong, string>();
        private static CsgoContainers csgoContainers;

        /// <summary>
        /// Gets the selected cases for all users from storage
        /// </summary>
        public static void GetUserSelectedCase()
        {
            string readCaseDataFromFile =
                FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("selectedCases.json"));

            if (!string.IsNullOrWhiteSpace(readCaseDataFromFile))
            {
                var userSelectedCaseData = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(readCaseDataFromFile);
                if (userSelectedCaseData != null)
                {
                    UserSelectedCase = userSelectedCaseData;
                }
            }
        }

        public static void FlushUserSelectedCase()
        {
            try
            {
                //Create a copy to write so that we don't get an error if it was modified mid write
                Dictionary<ulong, string> tempUserSelectedCaseData = UserSelectedCase;
                string jsonToWrite = JsonConvert.SerializeObject(tempUserSelectedCaseData);
                FileAccessManager.WriteStringToFile(jsonToWrite, true, FileAccessManager.GetFileLocation("selectedCases.json"));
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to flush user selected cases", ConsoleColor.Red);
            }
        }

        public static CsgoContainers GetCsgoContainers()
        {
            if (csgoContainers == null)
            {
                var tempCsgoContainers = XmlManager.FromXmlFile<CsgoContainers>(FileAccessManager.GetFileLocation("skinCases.xml"));

                if (tempCsgoContainers == null)
                {
                    tempCsgoContainers = new CsgoContainers
                    {
                        Containers = new List<Container>()
                    };
                }

                csgoContainers = tempCsgoContainers;
            }

            return csgoContainers;
        }

        /// <summary>
        /// Sets the CsgoContainers to the input
        /// </summary>
        /// <param name="input"></param>
        public static void SetCsgoContainers(CsgoContainers input)
        {
            csgoContainers = input;
        }

        /// <summary>
        /// Opens a virtual CS:GO case, result is sent to Context channel in a method
        /// </summary>
        /// <param name="context">Command context used to determine channel to send result</param>
        /// <returns></returns>
        public static async Task OpenCase(SocketCommandContext context)
        {
            //Get rarity
            var result = ItemDropProcessing.CalculateItemCaseRarity();

            //Get item
            var skinItem = ItemDropProcessing.GetItem(result, CsgoDataHandler.RootWeaponSkin, context, false);

            //Add item to user file inventory
            CsgoDataHandler.AddItemToUserInventory(context, skinItem);

            //Send item into
            await SendOpenedItemInfo(context, skinItem, Convert.ToInt64(skinItem.Price.AllTime.Average), UnboxType.CaseUnboxing);
        }

        /// <summary>
        /// virtual CS:GO drop given to user
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task OpenDrop(SocketCommandContext context)
        {
            //Select a rarity, this is slightly modified towards the white side of the spectrum, higher value items are harder to get as this is a drop
            var rarity = ItemDropProcessing.CalculateItemDropRarity();

            //Get item
            var skinItem = ItemDropProcessing.GetItem(rarity, CsgoDataHandler.RootWeaponSkin, context, true);

            //Add item to user file inventory
            CsgoDataHandler.AddItemToUserInventory(context, skinItem);

            //Send item into
            await SendOpenedItemInfo(context, skinItem, Convert.ToInt64(skinItem.Price.AllTime.Average), UnboxType.ItemDrop);
        }

        private static async Task SendOpenedItemInfo(SocketCommandContext context, SkinDataItem skinItem, long skinMarketValue, UnboxType unboxType)
        {
            //Get all collections skin / item is in
            string skinCaseCollections = "\u200b";
            //Do not display collection info for knives as they have a massive list of interchangeable cases
            if (skinItem.WeaponType != WeaponType.Knife)
            {
                if (skinItem.Cases != null) skinCaseCollections = string.Join("\n", skinItem.Cases.Select(i => i.CaseCollection));
            }

            //Get user selected case
            string selectedCaseIcon = csgoContainers.Containers.Where(s => s.Name == UserSelectedCase[context.Message.Author.Id]).Select(s => s.IconUrl).FirstOrDefault();

            //Set name to unboxing or item drop
            string title = "";
            if (unboxType == UnboxType.CaseUnboxing)
            {
                title = "CS:GO Case Unboxing";
            }
            else if (unboxType == UnboxType.ItemDrop)
            {
                title = "CS:GO Item drop";
            }

            //Embed
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(skinItem.RarityColor, 16)))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + context.Message.Author.ToString())
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName(title)
                        .WithIconUrl("https://i.redd.it/1s0j5e4fhws01.png");
                })
                .AddField(skinItem.Name, $"{skinCaseCollections}\n**Market Value: {skinMarketValue}**")
                .WithImageUrl("https://steamcommunity.com/economy/image/" + skinItem.IconUrlLarge);


            //Add case image URL if user is unboxing a case versus getting a drop
            if (unboxType == UnboxType.CaseUnboxing)
            {
                embedBuilder.WithThumbnailUrl(selectedCaseIcon);
            }

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        private enum UnboxType { CaseUnboxing, ItemDrop };
    }

    public class CsgoCaseSelectionHandler
    {
        /// <summary>
        /// Selects the appropriate cs go container to open, user replies with a number corrosponding to the case
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static PaginatedMessage ShowPossibleCases(SocketCommandContext context, string filter = null)
        {
            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context);

            //Create pagination entries
            List<string> leftCounter = new List<string>();
            List<string> filteredContainers = new List<string>();

            //Find containers whose name is not null
            filteredContainers = CsgoUnboxingHandler.GetCsgoContainers().Containers.Where(c => c.Name != null).Select(c => c.Name).ToList();

            //Create a list of ascending numbers to reference each container
            for (int i = 0; i < filteredContainers.Count(); i++)
            {
                leftCounter.Add(i.ToString());
            }

            //Filter pagerDesc if filter is set
            if (filter != null)
            {
                int loopAmount = filteredContainers.Count() - 1;
                for (int i = loopAmount; i >= 0; i--)
                {
                    if (!filteredContainers[i].ToLower().Contains(filter.ToLower()))
                    {
                        filteredContainers.Remove(filteredContainers[i]);
                        leftCounter.Remove(leftCounter[i]);
                    }
                }
            }


            //Generate pagination
            PaginationConfig paginationConfig = new PaginationConfig
            {
                AuthorName = "CS:GO Containers",
                AuthorUrl = "https://csgostash.com/img/containers/c259.png",

                Description = $"Select a container by typing appropriate number on left. No additional text. E.G `13`\nUse `{botCommandPrefix}open` to open cases\nFilter cases with `{botCommandPrefix}select [filter]`",

                Field1Header = "Number",
                Field2Header = "Case",

                Color = new Color(51, 204, 204)
            };

            PaginationManager paginationManager = new PaginationManager();
            var pager = paginationManager.GeneratePaginatedMessage(leftCounter, filteredContainers, paginationConfig);

            return pager;
        }

        /// <summary>
        /// Selects the appropriate cs go container to open, user replies with a number corrosponding to the case, the paginator message showing case options will also be deleted
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="sentMessage"></param>
        /// <returns></returns>
        public static async Task SelectOpenCase(SocketCommandContext context, string input, IUserMessage sentMessage)
        {
            //Delete the pagination message after receiving user input
            if (sentMessage != null)
            {
                await sentMessage.DeleteAsync();
            }

            List<Container> containers = CsgoUnboxingHandler.GetCsgoContainers().Containers.Where(c => c.Name != null).ToList();

            //Try to turn user input to string
            Container userSelectedContainer = new Container();
            try
            {
                int userInput = int.Parse(input);

                //Get the case user selected
                userSelectedContainer = containers[userInput];
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("Case Selection: " + UserInteraction.BoldUserName(context) + ", please input a valid number. No additional text. E.G `13`");
                return;
            }

            //Set user case preference
            if (!CsgoUnboxingHandler.UserSelectedCase.TryGetValue(context.Message.Author.Id, out _))
            {
                //If user does not exist, generate and set
                CsgoUnboxingHandler.UserSelectedCase.Add(context.Message.Author.Id, userSelectedContainer.Name);
            }
            else
            {
                //Don't set if the user has already selected the same case
                if (CsgoUnboxingHandler.UserSelectedCase[context.Message.Author.Id] != userSelectedContainer.Name)
                {
                    //If user does exist, only set
                    CsgoUnboxingHandler.UserSelectedCase[context.Message.Author.Id] = userSelectedContainer.Name;
                }
                else return;
            }

            await context.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you set your case to open to **{userSelectedContainer.Name}**");
        }

        public static bool GetHasUserSelectedCase(SocketCommandContext context)
        {
            if (!CsgoUnboxingHandler.UserSelectedCase.TryGetValue(context.Message.Author.Id, out _))
            {
                return false;
            }

            return true;
        }
    }

    internal class ItemDropProcessing
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
        /// <param name="skinData">Skin data to look through</param>
        /// <param name="context"></param>
        /// <param name="byPassCaseFilter"></param>
        /// <returns></returns>
        public static SkinDataItem GetItem(ItemListType itemListType, RootSkinData skinData, SocketCommandContext context, bool byPassCaseFilter)
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
                foreach (var item in skinData.ItemsList)
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
                //If bypass is true, sorted result is just root skinData
                //sortedResult = skinData.ItemsList.ToDictionary(x => x.Key, y => y.Value).ToList();

                //Add collection items, E.g Mirage collection, Nuke collection for drop, which has null for casesName
                foreach (var item in skinData.ItemsList)
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
                var selectedSkin = sortedResult[rand.Next(sortedResult.Count())];

                bool giveStatTrak = CalculateStatTrakDrop();
                //Give stattrak
                if (giveStatTrak == true)
                {
                    var selectedStatTrakItem = skinData.ItemsList
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
                    CsgoLeaderboardsManager.IncrementCaseStatTracker(context, CsgoLeaderboardsManager.CaseCategory.Sticker);
                    CsgoLeaderboardsManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardsManager.ItemCategory.Sticker);
                }
                //Souvenir
                else if (itemIsSouvenir && !byPassCaseFilter)
                {
                    CsgoLeaderboardsManager.IncrementCaseStatTracker(context, CsgoLeaderboardsManager.CaseCategory.Souvenir);
                    CsgoLeaderboardsManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardsManager.ItemCategory.Default);
                }
                //Case
                else if (!byPassCaseFilter)
                {
                    CsgoLeaderboardsManager.IncrementCaseStatTracker(context, CsgoLeaderboardsManager.CaseCategory.Case);
                    CsgoLeaderboardsManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardsManager.ItemCategory.Default);
                }
                //Drop
                else if (byPassCaseFilter)
                {
                    CsgoLeaderboardsManager.IncrementCaseStatTracker(context, CsgoLeaderboardsManager.CaseCategory.Drop);
                    CsgoLeaderboardsManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardsManager.ItemCategory.Default);
                }
                else
                {
                    CsgoLeaderboardsManager.IncrementStatTracker(context, itemListType, CsgoLeaderboardsManager.ItemCategory.Other);
                }

                return selectedSkin.Value;
            }
            else //Randomly pick a skin out of everything if it was unable to find one
            {
                var sortedResult2 = skinData.ItemsList.Where(s => s.Value.Rarity == Rarity.MilSpecGrade).ToList();
                var selectedSkin = sortedResult2[rand.Next(sortedResult2.Count())];

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

    public class CsgoContainers
    {
        public List<Container> Containers { get; set; }
    }
    public class Container
    {
        public string Name { get; set; }
        public string CollectionName { get; set; }
        public string IconUrl { get; set; }
        public bool IsSticker { get; set; }
        public bool IsSouvenir { get; set; }
        public bool IsTournamentSticker { get; set; }
        public bool SouvenirAvailable { get; set; }
        public List<ContainerEntry> ContainerEntries { get; set; }
    }
    public class ContainerEntry
    {
        //When adding skins
        // 1) Do NOT input the skin wear, wear is automatically looked for
        // 2) Do NOT put a space between the last letter of the skin name and the start of the wear
        // 3) Do NOT input stattrak, Stattrak is automatically accounted for
        public string SkinName { get; set; }
    }

    public class ItemListType
    {
        public Rarity Rarity { get; set; }
        public WeaponType? WeaponType { get; set; }
        public WeaponType? BlackListWeaponType { get; set; }
    }


    //User skin storage
    public class UserSkinStorage
    {
        public List<UserSkinEntry> UserSkinEntries { get; set; }
    }
    public class UserSkinEntry
    {
        public ulong OwnerId { get; set; }
        public string ClassId { get; set; }
        public DateTime UnboxDate { get; set; }
        public string MarketName { get; set; }
    }
}
