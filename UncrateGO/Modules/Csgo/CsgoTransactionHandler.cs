﻿using Discord.Commands;
using UncrateGo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //Get skin data
            var rootWeaponSkins = CsgoDataHandler.GetCsgoCosmeticData();

            SkinDataItem selectedMarketSkin = new SkinDataItem();

            //Get market skin cost
            long weaponSkinValue = 0;

            //Make sure skin exists in market
            var marketSkin = rootWeaponSkins.ItemsList.Values.Where(s => s.Name.ToLower() == itemMarketHash.ToLower()).ToList().FirstOrDefault();

            //If searching by direct result cannot be found, search by anything that contains the input
            if (marketSkin == null)
            {
                marketSkin = rootWeaponSkins.ItemsList.Values.Where(s => s.Name.ToLower().Contains(itemMarketHash.ToLower())).ToList().FirstOrDefault();
                //If it still cannot be found, search by whole words
                if (marketSkin == null)
                {
                    marketSkin = FindSimilarItemsByWords(rootWeaponSkins, itemMarketHash).FirstOrDefault();
                    //Send error if skin does not exist
                    if (marketSkin == null)
                    {
                        await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", `{itemMarketHash}` does not exist in the current skin market");
                        return;
                    }
                }
            }

            ulong userId = context.Message.Author.Id;

            //If skin does exist, get info on it
            SkinDataItem weaponSkin = rootWeaponSkins.ItemsList.Values.FirstOrDefault(s => s.Name == marketSkin.Name);
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
                    $"{UserInteraction.BoldUserName(context)}, you do not have enough credits to buy `{selectedMarketSkin.Name}` | **{BankingHandler.CurrencyFormatter(weaponSkinValue)}** - **{BankingHandler.CurrencyFormatter(userCredits)}** ");
            }
            else
            {
                //Checks are true, now give user skin and remove credits

                //Remove user credits
                if (BankingHandler.AddCredits(userId, -weaponSkinValue))
                {
                    //Add skin to inventory
                    CsgoDataHandler.AddItemToUserInventory(context, selectedMarketSkin);

                    //Send receipt
                    await context.Channel.SendMessageAsync(
                        UserInteraction.BoldUserName(context) + $", you bought `{selectedMarketSkin.Name}`" +
                        $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
                }
            }
        }

        //Sell
        public static async Task SellInventoryItemAsync(SocketCommandContext context, string itemMarketHash)
        {
            //Get skin data
            var rootWeaponSkin = CsgoDataHandler.GetCsgoCosmeticData();
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            //Find user selected item, make sure it is owned by user
            var selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower() == itemMarketHash.ToLower())
                .Where(s => s.OwnerId == context.Message.Author.Id)
                .FirstOrDefault();

            //If searching by direct comparison results in nothing, search by contain
            if (selectedSkinToSell == null)
            {
                selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                .Where(s => s.OwnerId == context.Message.Author.Id)
                .FirstOrDefault();
            }

            //Try to search by whole words if still null
            if (selectedSkinToSell == null)
            {
                selectedSkinToSell = FindSimilarItemsByWords(userSkin.UserSkinEntries, context, itemMarketHash).FirstOrDefault();
            }

            if (selectedSkinToSell == null)
            {
                //Send error if user does not have item
                await context.Channel.SendMessageAsync($"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }
            else
            {
                //Get item price
                var itemData = rootWeaponSkin.ItemsList.Values.Where(s => s.Name == selectedSkinToSell.MarketName).FirstOrDefault();

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
                    UserInteraction.BoldUserName(context) + $", you sold your `{selectedSkinToSell.MarketName}`" +
                    $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
            }

        }

        public static async Task SellAllSelectedInventoryItemAsync(SocketCommandContext context, string itemMarketHash)
        {
            //Get skin data
            var rootSkinData = CsgoDataHandler.GetCsgoCosmeticData();
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            //Find ALL user selected items, make sure it is owned by user
            List<UserSkinEntry> selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                .Where(s => s.OwnerId == context.Message.Author.Id).ToList();

            //Try to search by whole words if still null
            if (selectedSkinToSell.Count == 0)
            {
                selectedSkinToSell = FindSimilarItemsByWords(userSkin.UserSkinEntries, context, itemMarketHash);
            }

            //Get item prices
            long weaponSkinValue = GetItemValue(selectedSkinToSell, rootSkinData);

            //Give user credits
            BankingHandler.AddCredits(context.Message.Author.Id, weaponSkinValue);

            //Remove skin from inventory
            var filterUserSkinNames = new List<string>();
            foreach (var item in selectedSkinToSell)
            {
                //Remove items that were selected to be sold
                userSkin.UserSkinEntries.Remove(item);

                filterUserSkinNames.Add(item.MarketName);
            }

            if (filterUserSkinNames.Count > 0)
            {
                //Set skin storage
                CsgoDataHandler.SetUserSkinStorage(userSkin);

                //join weapon string
                string soldWeaponsString = string.Join("\n", filterUserSkinNames);
                //Cut string off if length is greater than 1000
                if (soldWeaponsString.Length > 1000) soldWeaponsString = soldWeaponsString.Substring(0, 1000) + "...";

                //Send receipt
                await context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(context) + $", you sold your \n`{soldWeaponsString}`" +
                    $" for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
            }
            else
            {
                //Send error if user does not have item
                await context.Channel.SendMessageAsync($"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**, you do not have anything containing `{itemMarketHash}` in your inventory");
            }

        }

        public static async Task SellAllInventoryItemAsync(SocketCommandContext context)
        {
            //Get price data
            var rootSkinData = CsgoDataHandler.GetCsgoCosmeticData();
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            //If player has items in inventory, sell!
            if (userSkin.UserSkinEntries.Any(s => s.OwnerId == context.Message.Author.Id))
            {
                long weaponSkinValue = GetItemValue(userSkin.UserSkinEntries.Where(s => s.OwnerId == context.Message.Author.Id).ToList(), rootSkinData);

                //Give user credits
                BankingHandler.AddCredits(context.Message.Author.Id, weaponSkinValue);

                //Remove user skins from inventory
                List<UserSkinEntry> filteredUserSkinEntries = userSkin.UserSkinEntries.Where(s => s.OwnerId != context.Message.Author.Id).ToList();

                //Write to file
                var newUserSkinStorageRoot = new UserSkinStorage
                {
                    UserSkinEntries = filteredUserSkinEntries
                };

                //Set skin storage
                CsgoDataHandler.SetUserSkinStorage(newUserSkinStorageRoot);

                //Send receipt
                await context.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you sold your inventory for **{BankingHandler.CurrencyFormatter(weaponSkinValue)} Credits**");
            }
            else
            {
                //Send error user does not have any items
                await context.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", your inventory is empty! Go unbox some with `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}open`");
            }
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
                    EventLogger.LogMessage("Error trying to get item value at item | " + item, ConsoleColor.Red);
                }
                
            }

            return weaponSkinValue;
        }

        public static async Task DisplayCsgoItemStatistics(SocketCommandContext context, string filterString)
        {
            //Search by exact, then contain, then whole words
            var skinItem = CsgoDataHandler.CsgoWeaponCosmetic.ItemsList.Values.Where(c => c.Name.ToLower() == filterString.ToLower()).FirstOrDefault();
            if (skinItem == null) skinItem = CsgoDataHandler.CsgoWeaponCosmetic.ItemsList.Values.Where(c => c.Name.ToLower().Contains(filterString.ToLower())).FirstOrDefault();
            if (skinItem == null) skinItem = FindSimilarItemsByWords(CsgoDataHandler.CsgoWeaponCosmetic, filterString).FirstOrDefault();

            //Check if the skin exists
            if (skinItem != null)
            {
                //Get all collections skin / item is in
                string skinCaseCollections = "\u200b";

                //Do not display collection info for knives as they have a massive list of interchangeable cases
                if (skinItem.WeaponType != WeaponType.Knife)
                {
                    if (skinItem.Cases != null) skinCaseCollections = string.Join("\n", skinItem.Cases.Select(i => i.CaseCollection));
                }

                //Get item price
                long weaponSkinPrice = Convert.ToInt64(skinItem.Price.AllTime.Average);


                //Send embed
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
                            .WithName("Item Info")
                            .WithIconUrl("https://i.redd.it/1s0j5e4fhws01.png");
                    })
                    .AddField(skinItem.Name, $"{skinCaseCollections}\nMarket Value: {weaponSkinPrice}")
                    .WithImageUrl("https://steamcommunity.com/economy/image/" + skinItem.IconUrlLarge);

                var embed = embedBuilder.Build();

                await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);

            }
            else
            {
                //Send embed
                var embedBuilder = new EmbedBuilder()
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

                var embed = embedBuilder.Build();

                await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
            }

        }

        //Market
        public static PaginatedMessage GetCsgoMarketInventory(SocketCommandContext context, string filterString)
        {
            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context);

            //Get skin data
            var rootWeaponSkin = CsgoDataHandler.GetCsgoCosmeticData();

            var filteredRootWeaponSkin = new List<string>();
            var filteredRootWeaponSkinPrice = new List<string>();

            //Only show if they specified a filter
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                //filter rootWeaponSkin to those with a price found in rootWeaponSkinPrice
                List<SkinDataItem> filteredItems = rootWeaponSkin.ItemsList.Values.Where(sk => sk.Name.ToLower().Contains(filterString.ToLower())).ToList();

                //If searching by direct result cannot be found, search by anything that contains the input
                if (filteredItems.Count == 0)
                {
                    filteredItems = rootWeaponSkin.ItemsList.Values.Where(s => s.Name.ToLower().Contains(filterString.ToLower())).ToList();
                }
                //If it still cannot be found, search by whole words
                if (filteredItems.Count == 0)
                {
                    filteredItems = FindSimilarItemsByWords(rootWeaponSkin, filterString).ToList();
                }

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

        //Filtering
        private static List<UserSkinEntry> FindSimilarItemsByWords(List<UserSkinEntry> userSkinEntry, SocketCommandContext context, string inputString)
        {
            var userSkinEntries = new List<UserSkinEntry>();

            bool match = false;

            string[] tokens = inputString.ToLower().Split(' ');

            //Filter out items not owned by user
            userSkinEntry = userSkinEntry.Where(s => s.OwnerId == context.Message.Author.Id).ToList();

            //Search through userCosmeticEntry for words that have the specified input string seperated with spaces
            foreach (var item in userSkinEntry)
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!item.MarketName.ToLower().Contains(tokens[i]))
                    {
                        match = false;
                        break;
                    }

                    match = true;
                    
                }

                if (match) userSkinEntries.Add(item);
            }

            return userSkinEntries;
        }

        private static List<SkinDataItem> FindSimilarItemsByWords(CsgoCosmeticData userCosmeticEntry, string inputString)
        {
            var userSkinEntries = new List<SkinDataItem>();

            bool match = false;

            string[] tokens = RemoveSpecialCharacters(inputString).ToLower().Split(' ');

            //Search through userCosmeticEntry for words that have the specified input string separated with spaces
            foreach (var item in userCosmeticEntry.ItemsList.Values)
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!RemoveSpecialCharacters(item.Name).ToLower().Contains(tokens[i]))
                    {
                        match = false;
                        break;
                    }

                    match = true;

                }

                if (match) userSkinEntries.Add(item);
            }

            return userSkinEntries;
        }

        private static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || (str[i] == '.' || str[i] == '_')))
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }
    }
}
