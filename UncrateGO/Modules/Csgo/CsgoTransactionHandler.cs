using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoTransactionHandler
    {
        //Buy
        public static async Task BuyItemFromMarketAsync(SocketCommandContext context, string itemMarketHash)
        {
            //Get skin data
            var rootWeaponSkins = CsgoDataHandler.GetRootWeaponSkin();

            SkinDataItem selectedMarketSkin = new SkinDataItem();

            //Get market skin cost
            long weaponSkinValue = 0;



            bool userSpecifiedSkinExistsInMarket = false;

            //Make sure skin exists in market
            foreach (var marketSkin in rootWeaponSkins.ItemsList.Values)
            {
                //If it does exist, get info on it
                if (marketSkin.Name.ToLower() == itemMarketHash.ToLower())
                {
                    weaponSkinValue = Convert.ToInt64(rootWeaponSkins.ItemsList.Values.Where(s => s.Name == marketSkin.Name).FirstOrDefault().Price.AllTime.Average);
                    userSpecifiedSkinExistsInMarket = true;

                    selectedMarketSkin.Classid = marketSkin.Classid;
                    selectedMarketSkin.Name = marketSkin.Name;
                }
            }
            //If searching by direct result cannot be found, search by anything that contains the input
            if (!userSpecifiedSkinExistsInMarket)
            {
                foreach (var marketSkin in rootWeaponSkins.ItemsList.Values)
                {
                    //If it does exist, get info on it
                    if (marketSkin.Name.ToLower().Contains(itemMarketHash.ToLower()))
                    {
                        weaponSkinValue = Convert.ToInt64(rootWeaponSkins.ItemsList.Values.Where(s => s.Name == marketSkin.Name).FirstOrDefault().Price.AllTime.Average);
                        userSpecifiedSkinExistsInMarket = true;

                        selectedMarketSkin.Classid = marketSkin.Classid;
                        selectedMarketSkin.Name = marketSkin.Name;
                    }
                }
            }               

            //Send error if skin does not exist
            if (!userSpecifiedSkinExistsInMarket)
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", `{itemMarketHash}` does not exist in the current skin market");
            }
            //Make sure user has enough credits to buy skin
            else if (BankingHandler.GetUserCredits(context) < weaponSkinValue)
            {
                await context.Message.Channel.SendMessageAsync($"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**, you do not have enough credits to buy `{selectedMarketSkin.Name}` | **{BankingHandler.CreditCurrencyFormatter(weaponSkinValue)}** - **{BankingHandler.CreditCurrencyFormatter(BankingHandler.GetUserCredits(context))}** ");
            }
            else
            {
                //Checks are true, now give user skin and remove credits

                //Remove user credits
                BankingHandler.AddCredits(context, -weaponSkinValue);

                //Add skin to inventory
                CsgoDataHandler.AddItemToUserInventory(context, selectedMarketSkin);

                //Send receipt
                await context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(context) + $", you bought`{selectedMarketSkin.Name}`" +
                    $" for **{BankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
        }

        //Sell
        public static async Task SellInventoryItemAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get skin data
            var rootWeaponSkin = CsgoDataHandler.GetRootWeaponSkin();
            var userSkin = CsgoDataHandler.GetUserSkinStorageRootobject();

            //Find user selected item, make sure it is owned by user
            var selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower() == itemMarketHash.ToLower())
                .Where(s => s.OwnerID == Context.Message.Author.Id)
                .FirstOrDefault();

            //If searching by direct comparison results in nothing, search by contain
            if (selectedSkinToSell == null)
            {
                selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                .Where(s => s.OwnerID == Context.Message.Author.Id)
                .FirstOrDefault();
            }

            if (selectedSkinToSell == null)
            {
                //Send error if user does not have item
                await Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }
            else
            {
                //Get item price
                long weaponSkinValue = Convert.ToInt64(rootWeaponSkin.ItemsList.Values.Where(s => s.Name == selectedSkinToSell.MarketName).FirstOrDefault().Price.AllTime.Average);

                //Give user credits
                BankingHandler.AddCredits(Context, weaponSkinValue);


                //Remove items that were selected to be sold
                userSkin.UserSkinEntries.Remove(selectedSkinToSell);

                //Write to file
                CsgoDataHandler.WriteUserSkinStorageRootobject(userSkin);

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(Context) + $", you sold your `{selectedSkinToSell.MarketName}`" +
                    $" for **{BankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }

        }

        public static async Task SellAllSelectedInventoryItemAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get skin data
            var rootSkinData = CsgoDataHandler.GetRootWeaponSkin();
            var userSkin = CsgoDataHandler.GetUserSkinStorageRootobject();

            //Find ALL user selected items, make sure it is owned by user
            var selectedSkinToSell = userSkin.UserSkinEntries
                .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                .Where(s => s.OwnerID == Context.Message.Author.Id).ToList();

            //Get item prices
            long weaponSkinValue = GetItemValue(selectedSkinToSell, rootSkinData);

            //Give user credits
            BankingHandler.AddCredits(Context, weaponSkinValue);

            //Remove skin from inventory
            List<string> filterUserSkinNames = new List<string>();
            foreach (var item in selectedSkinToSell)
            {
                //Remove items that were selected to be sold
                userSkin.UserSkinEntries.Remove(item);

                filterUserSkinNames.Add(item.MarketName);
            }

            if (filterUserSkinNames.Count > 0)
            {
                //Write to file
                CsgoDataHandler.WriteUserSkinStorageRootobject(userSkin);

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(Context) + $", you sold your \n`{string.Join("\n", filterUserSkinNames)}`" +
                    $" for **{BankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
            else
            {
                //Send error if user does not have item
                await Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have anything containing `{itemMarketHash}` in your inventory");
            }

        }

        public static async Task SellAllInventoryItemAsync(SocketCommandContext context)
        {
            //Get price data
            var rootSkinData = CsgoDataHandler.GetRootWeaponSkin();
            var userSkin = CsgoDataHandler.GetUserSkinStorageRootobject();

            //If player has items in inventory, sell!
            if (userSkin.UserSkinEntries.Where(s => s.OwnerID == context.Message.Author.Id).Count() > 0)
            {
                long weaponSkinValue = GetItemValue(userSkin.UserSkinEntries, rootSkinData);

                //Give user credits
                BankingHandler.AddCredits(context, weaponSkinValue);

                //Remove user skins from inventory
                var filteredUserSkinEntries = userSkin.UserSkinEntries.Where(s => s.OwnerID != context.Message.Author.Id).ToList();

                //Write to file
                var newUserSkinStorageRoot = new UserSkinStorageRootobject
                {
                    UserSkinEntries = filteredUserSkinEntries
                };
                CsgoDataHandler.WriteUserSkinStorageRootobject(newUserSkinStorageRoot);

                //Send receipt
                await context.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you sold your inventory for **{BankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
            else
            {
                //Send error user does not have any items
                await context.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", your inventory is empty! Go unbox some with `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}open`");
            }
        }

        private static long GetItemValue(List<UserSkinEntry> userSkins, RootSkinData rootSkinData)
        {
            long weaponSkinValue = 0;
            foreach (var item in userSkins)
            {
                weaponSkinValue += Convert.ToInt64(rootSkinData.ItemsList.Values.Where(s => s.Name == item.MarketName).FirstOrDefault().Price.AllTime.Average);
            }

            return weaponSkinValue;
        }
    }
}
