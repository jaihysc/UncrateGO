using Discord.Commands;
using UncrateGo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoTransactionHandler
    {
        //Buy
        public static async Task BuyItemFromMarketAsync(SocketCommandContext context, string itemMarketHash)
        {
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();
            string userName = UserInteraction.BoldUserName(context.Message.Author.ToString());

            //Get the weapon skin specified
            SkinDataItem marketSkin = FuzzyFindSkinDataItem(cosmeticData.ItemsList.Values.ToList(), itemMarketHash).FirstOrDefault();
            //Send error if skin does not exist
            if (marketSkin == null)
            {
                await context.Message.Channel.SendMessageAsync(userName + $", `{itemMarketHash}` does not exist in the current market");
                return;
            }

            ulong userId = context.Message.Author.Id;

            //Get market skin cost
            long weaponSkinValue = 0;

            SkinDataItem selectedMarketSkin = new SkinDataItem();

            //If skin does exist, get info on it
            SkinDataItem weaponSkin = cosmeticData.ItemsList.Values.FirstOrDefault(s => s.Name == marketSkin.Name);
            if (weaponSkin != null)
            {
                weaponSkinValue = Convert.ToInt64(weaponSkin.Price.AllTime.Average);
                selectedMarketSkin.Classid = marketSkin.Classid;
                selectedMarketSkin.Name = marketSkin.Name;
            }

            long userCredits = UserDataManager.GetUserCredit(userId);
            //Make sure user has enough credits to buy skin
            if (userCredits < weaponSkinValue)
            {
                await context.Message.Channel.SendMessageAsync(
                    $"{userName}, you do not have enough credits to buy `{selectedMarketSkin.Name}` | **{BankingHandler.CurrencyFormatter(weaponSkinValue)}** - **{BankingHandler.CurrencyFormatter(userCredits)}** ");
            }
            else
            {
                //Checks are true, now give user skin and remove credits

                //Remove user credits
                if (BankingHandler.AddCredits(userId, -weaponSkinValue))
                {
                    CsgoDataHandler.AddItemToUserInventory(context.Message.Author.Id, selectedMarketSkin);

                    //Send receipt
                    await context.Channel.SendMessageAsync(
                        userName + $", you bought `{selectedMarketSkin.Name}`" +
                        $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
                }
            }
        }

        //Sell
        public static async Task SellInventoryItemAsync(SocketCommandContext context, string itemMarketHash)
        {
            //Get skin data
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            string userName = UserInteraction.BoldUserName(context.Message.Author.ToString());

            List<UserSkinEntry> userItems = CsgoDataHandler.GetUserItems(context.Message.Author.Id);

            //Find user selected item, make sure it is owned by user
            UserSkinEntry selectedSkinToSell = FuzzyFindUserSkinEntries(userItems, itemMarketHash).FirstOrDefault();

            if (selectedSkinToSell == null)
            {
                //Send error if user does not have item
                await context.Channel.SendMessageAsync($"{userName}, you do not have `{itemMarketHash}` in your inventory");
                return;
            }

            //Get item price
            var itemData = cosmeticData.ItemsList.Values.FirstOrDefault(s => s.Name == selectedSkinToSell.MarketName);

            long weaponSkinValue = 0;
            if (itemData != null)
            {
                weaponSkinValue += Convert.ToInt64(itemData.Price.AllTime.Average);
            }
                
            //Give user credits
            BankingHandler.AddCredits(context.Message.Author.Id, weaponSkinValue);

            //Remove items that were selected to be sold
            userSkin.UserSkinEntries.Remove(selectedSkinToSell);

            //Set skin storage
            CsgoDataHandler.SetUserSkinStorage(userSkin);

            //Send receipt
            await context.Channel.SendMessageAsync(
                userName + $", you sold your `{selectedSkinToSell.MarketName}`" +
                $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");

        }

        public static async Task SellAllSelectedInventoryItemAsync(SocketCommandContext context, string itemMarketHash)
        {
            List<UserSkinEntry> userItems = CsgoDataHandler.GetUserItems(context.Message.Author.Id);

            //Find user selected item, make sure it is owned by user
            List<UserSkinEntry> selectedItemsToSell = FuzzyFindUserSkinEntries(userItems, itemMarketHash);

            string userName = UserInteraction.BoldUserName(context.Message.Author.ToString());

            if (!selectedItemsToSell.Any())
            {
                //Send error if user does not have item
                await context.Channel.SendMessageAsync(
                    $"{userName}, you do not have anything containing `{itemMarketHash}` in your inventory");
                return;
            }

            var csgoCosmeticData = CsgoDataHandler.GetCsgoCosmeticData();

            //Get item prices
            long weaponSkinValue = GetItemValue(selectedItemsToSell, csgoCosmeticData);

            //Give user credits
            BankingHandler.AddCredits(context.Message.Author.Id, weaponSkinValue);

            var userSkin = CsgoDataHandler.GetUserSkinStorage();
            //Remove selected skins from inventory
            var filterUserSkinNames = new List<string>();
            foreach (UserSkinEntry item in selectedItemsToSell)
            {
                //Remove items that were selected to be sold
                userSkin.UserSkinEntries.Remove(item);

                filterUserSkinNames.Add(item.MarketName); //Add them to a counter to be logged
            }

            if (filterUserSkinNames.Any())
            {
                //Set skin storage
                CsgoDataHandler.SetUserSkinStorage(userSkin);

                //join weapon string
                string soldWeaponsString = string.Join("\n", filterUserSkinNames);
                //Cut string off if length is greater than 1000
                if (soldWeaponsString.Length > 1000) soldWeaponsString = soldWeaponsString.Substring(0, 1000) + "...";

                //Send receipt
                await context.Channel.SendMessageAsync(
                    userName + $", you sold your \n`{soldWeaponsString}`" +
                    $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
            }
        }

        public static async Task SellAllInventoryItemAsync(SocketCommandContext context)
        {
            //Get price data
            var csgoCosmeticData = CsgoDataHandler.GetCsgoCosmeticData();
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            List<UserSkinEntry> userSkins = CsgoDataHandler.GetUserItems(context.Message.Author.Id);

            string userName = UserInteraction.BoldUserName(context.Message.Author.ToString());

            //If player does not have items in inventory, send error
            if (!userSkins.Any())
            {
                //Send error user does not have any items
                await context.Channel.SendMessageAsync(userName +
                                                       $", your inventory is empty! Go unbox some with `{GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel)}open`");
                return;
            }

            long weaponSkinValue =
                GetItemValue(userSkin.UserSkinEntries.Where(s => s.OwnerId == context.Message.Author.Id).ToList(), csgoCosmeticData);

            //Give user credits
            BankingHandler.AddCredits(context.Message.Author.Id, weaponSkinValue);

            //Remove user skins from inventory
            foreach (UserSkinEntry item in userSkins)
            {
                userSkin.UserSkinEntries.Remove(item);
            }

            //Send receipt
            await context.Channel.SendMessageAsync(userName +
                                                   $", you sold your inventory for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
        }

        public static async Task DisplayCsgoItemStatistics(SocketCommandContext context, string filterString)
        {
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();

            //Get the weapon skin specified
            SkinDataItem skinItem = FuzzyFindSkinDataItem(cosmeticData.ItemsList.Values.ToList(), filterString).FirstOrDefault();

            EmbedBuilder embedBuilder;
            Embed embed;
            //Send error if skin does not exist
            if (skinItem == null)
            {
                //Send embed
                embedBuilder = new EmbedBuilder()
                    .WithColor(new Color(0, 200, 0))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText("Sent by " + context.Message.Author.ToString())
                            .WithIconUrl(context.Message.Author.GetAvatarUrl());
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Item Info")
                            .WithIconUrl("https://i.redd.it/1s0j5e4fhws01.png");
                    })
                    .AddField("The selected item could not be found", "Broaden your search parameters and try again");

                embed = embedBuilder.Build();

                await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
                return;
            }

            //Get all collections skin / item is in
            string skinCaseCollections = "\u200b"; //Default text is blank

            //Do not display collection info for knives as they have a massive list of interchangeable cases
            if (skinItem.WeaponType != WeaponType.Knife)
            {
                if (skinItem.Cases != null) skinCaseCollections = string.Join("\n", skinItem.Cases.Select(i => i.CaseCollection));
            }

            //Get item price
            long weaponSkinPrice = Convert.ToInt64(skinItem.Price.AllTime.Average);

            //Send embed
            embedBuilder = new EmbedBuilder()
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
                        .WithName("Item Info")
                        .WithIconUrl("https://i.redd.it/1s0j5e4fhws01.png");
                })
                .AddField(skinItem.Name, $"{skinCaseCollections}\nMarket Value: {weaponSkinPrice}")
                .WithImageUrl("https://steamcommunity.com/economy/image/" + skinItem.IconUrlLarge);

            embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        //Market
        public static PaginatedMessage GetCsgoMarketInventory(SocketCommandContext context, string filterString)
        {
            //Get skin data
            var cosmeticData = CsgoDataHandler.GetCsgoCosmeticData();

            var filteredRootWeaponSkin = new List<string>();
            var filteredRootWeaponSkinPrice = new List<string>();

            //Only show if they specified a filter
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                //Get the weapon skin specified
                List<SkinDataItem> filteredItems = FuzzyFindSkinDataItem(cosmeticData.ItemsList.Values.ToList(), filterString);

                foreach (var skin in filteredItems)
                {
                    string skinQualityEmote = CsgoInventoryManager.GetEmoteBySkinRarity(skin.Rarity, skin.WeaponType);

                    //Add skin entry
                    Emote emote = Emote.Parse(skinQualityEmote);

                    //Add weapon skin
                    filteredRootWeaponSkin.Add(emote + " " + skin.Name);

                    //Get item value
                    long weaponSkinValue = Convert.ToInt64(skin.Price.AllTime.Average);

                    //Add weapon skin price
                    filteredRootWeaponSkinPrice.Add(emote + " " + weaponSkinValue.ToString());
                }
            }

            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel);
            //Configure paginated message
            var paginationConfig = new PaginationConfig
            {
                AuthorName = "CS:GO Market",
                AuthorUrl = "https://i.redd.it/1s0j5e4fhws01.png",

                Description = $"Buy item: `{botCommandPrefix}buy [name]`\nFilter market items by name: `{botCommandPrefix}market [name]`\nView item: `{botCommandPrefix}view [name]`",

                DefaultFieldHeader = "Unable to find specified item!",
                DefaultFieldDescription = "Broaden your search parameters and try again",

                Field1Header = "Name",
                Field2Header = "Price",

                Color = new Color(0, 204, 0)
            };

            var paginationManager = new PaginationManager();

            //Generate paginated message
            var pager = paginationManager.GeneratePaginatedMessage(filteredRootWeaponSkin, filteredRootWeaponSkinPrice, paginationConfig);

            return pager;
        }

        //Helper
        private static long GetItemValue(List<UserSkinEntry> userSkins, CsgoCosmeticData csgoCosmeticData)
        {
            long weaponSkinValue = 0;
            foreach (var item in userSkins)
            {
                try
                {
                    var itemData = csgoCosmeticData.ItemsList.Values.FirstOrDefault(s => s.Name == item.MarketName);

                    if (itemData != null)
                    {
                        weaponSkinValue += Convert.ToInt64(itemData.Price.AllTime.Average);
                    }

                }
                catch (Exception)
                {
                    EventLogger.LogMessage("Error trying to get item value at item | " + item, EventLogger.LogLevel.Error);
                }

            }

            return weaponSkinValue;
        }

        //Filtering
        /// <summary>
        /// Utilities 3 search methods to find a specified item near the item specified
        /// </summary>
        /// <param name="skinDataItems"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public static List<SkinDataItem> FuzzyFindSkinDataItem(List<SkinDataItem> skinDataItems, string itemName)
        {
            //Get the weapon skin specified
            List<SkinDataItem> marketSkins = skinDataItems.Where(s => s.Name.ToLower() == itemName.ToLower()).ToList();

            //If searching by direct result cannot be found, search by anything that contains the input
            if (!marketSkins.Any())
            {
                marketSkins = skinDataItems.Where(s => s.Name.ToLower().Contains(itemName.ToLower())).ToList();

                //If it still cannot be found, search by whole words
                if (!marketSkins.Any())
                {
                    string foundSkinName = FuzzySearch.FindSimilarItemsByWords(skinDataItems.Select(c => c.Name), itemName).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(foundSkinName)) marketSkins = skinDataItems.Where(s => s.Name == foundSkinName).ToList();
                }
            }

            return marketSkins;
        }

        public static List<UserSkinEntry> FuzzyFindUserSkinEntries(List<UserSkinEntry> userItems, string itemName)
        {
            List<UserSkinEntry> selectedItems = userItems.Where(s => s.MarketName.ToLower() == itemName.ToLower()).ToList();

            //If searching by direct comparison results in nothing, search by contain
            if (!selectedItems.Any())
            {
                selectedItems = userItems.Where(s => s.MarketName.ToLower().Contains(itemName.ToLower())).ToList();

                //Try to search by whole words if still null
                if (!selectedItems.Any())
                {
                    string foundItem = FuzzySearch.FindSimilarItemsByWords(userItems.Select(i => i.MarketName), itemName).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(foundItem)) selectedItems = userItems.Where(s => s.MarketName == foundItem).ToList();
                }
            }

            return selectedItems;
        }
    }
}
