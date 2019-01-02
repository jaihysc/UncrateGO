using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Models;
using UncrateGo.Modules.Finance.CurrencyManager;
using UncrateGo.Modules.Interaction;
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

            try
            {
                UserSkinEntry selectedMarketSkin = new UserSkinEntry();

                //Get market skin cost
                long weaponSkinValue = Convert.ToInt64(rootWeaponSkins.ItemsList.Values.Where(s => s.Name.ToLower().Contains(itemMarketHash.ToLower())).FirstOrDefault().Price.AllTime.Average);



                bool userSpecifiedSkinExistsInMarket = false;

                //Make sure skin exists in market
                foreach (var marketSkin in rootWeaponSkins.ItemsList.Values)
                {
                    //If it does exist, get info on it
                    if (marketSkin.Name.ToLower().Contains(itemMarketHash.ToLower()))
                    {
                        userSpecifiedSkinExistsInMarket = true;

                        selectedMarketSkin.ClassId = marketSkin.Classid;
                        selectedMarketSkin.OwnerID = context.Message.Author.Id;
                        selectedMarketSkin.UnboxDate = DateTime.UtcNow;
                        selectedMarketSkin.MarketName = marketSkin.Name;
                    }
                }
                //Send error if skin does not exist
                if (userSpecifiedSkinExistsInMarket == false)
                {
                    await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", `{itemMarketHash}` does not exist in the current skin market");
                }
                //Make sure user has enough credits to buy skin
                else if (UserCreditsHandler.GetUserCredits(context) < weaponSkinValue)
                {
                    await context.Message.Channel.SendMessageAsync($"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**, you do not have enough credits to buy`{itemMarketHash}` | **{UserCreditsHandler.GetUserCredits(context)} Credits**");
                }
                else
                {
                    //Checks are true, now give user skin and remove credits

                    //Remove user credits
                    UserCreditsHandler.AddCredits(context, -weaponSkinValue);

                    //Add skin to inventory
                    var userSkins = XmlManager.FromXmlFile<UserSkinStorageRootobject>(FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

                    userSkins.UserSkinEntries.Add(selectedMarketSkin);

                    var filteredUserSkin = new UserSkinStorageRootobject
                    {
                        SkinAmount = 0,
                        UserSkinEntries = userSkins.UserSkinEntries
                    };

                    XmlManager.ToXmlFile(filteredUserSkin, FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

                    //Send receipt
                    await context.Channel.SendMessageAsync(
                        UserInteraction.BoldUserName(context) + $", you bought`{selectedMarketSkin.MarketName}`" +
                        $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        //Sell
        public static async Task SellInventoryItemAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get skin data
            var rootWeaponSkin = CsgoDataHandler.GetRootWeaponSkin();
            var rootUserSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

            try
            {
                //Find user selected item, make sure it is owned by user
                var selectedSkinToSell = rootUserSkin.UserSkinEntries
                    .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                    .Where(s => s.OwnerID == Context.Message.Author.Id)
                    .FirstOrDefault();

                //Get item price
                long weaponSkinValue = Convert.ToInt64(rootWeaponSkin.ItemsList.Values.Where(s => s.Name == selectedSkinToSell.MarketName).FirstOrDefault().Price.AllTime.Average);

                //Give user credits
                UserCreditsHandler.AddCredits(Context, weaponSkinValue);

                //Remove skin from inventory
                var filteredUserSkinEntries = rootUserSkin.UserSkinEntries.Where(s => s.OwnerID == Context.Message.Author.Id).Where(s => s.ClassId != selectedSkinToSell.ClassId).ToList();

                //Write to file
                WriteUserSkinDataToFile(filteredUserSkinEntries);

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(Context) + $", you sold your `{selectedSkinToSell.MarketName}`" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
            catch (Exception)
            {
                //Send error if user does not have item
                await Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }

        }

        public static async Task SellAllSelectedInventoryItemAsync(SocketCommandContext Context, string itemMarketHash)
        {
            //Get skin data
            var rootSkinData = CsgoDataHandler.GetRootWeaponSkin();
            var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

            try
            {
                //Find ALL user selected items, make sure it is owned by user
                var selectedSkinToSell = userSkin.UserSkinEntries
                    .Where(s => s.MarketName.ToLower().Contains(itemMarketHash.ToLower()))
                    .Where(s => s.OwnerID == Context.Message.Author.Id).ToList();

                //Get item prices
                long weaponSkinValue = GetItemValue(selectedSkinToSell, rootSkinData);

                //Give user credits
                UserCreditsHandler.AddCredits(Context, weaponSkinValue);

                //Remove skin from inventory
                List<string> filterUserSkinNames = new List<string>();
                foreach (var item in selectedSkinToSell)
                {
                    //Remove items that were selected to be sold
                    userSkin.UserSkinEntries.Remove(item);

                    filterUserSkinNames.Add(item.MarketName);
                }

                //Write to file
                WriteUserSkinDataToFile(userSkin);

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(Context) + $", you sold your \n`{string.Join("\n", filterUserSkinNames)}`" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
            catch (Exception)
            {
                //Send error if user does not have item
                await Context.Channel.SendMessageAsync($"**{Context.Message.Author.ToString().Substring(0, Context.Message.Author.ToString().Length - 5)}**, you do not have `{itemMarketHash}` in your inventory");
            }

        }

        public static async Task SellAllInventoryItemAsync(SocketCommandContext Context)
        {
            //Get price data
            var rootSkinData = CsgoDataHandler.GetRootWeaponSkin();
            var userSkin = XmlManager.FromXmlFile<UserSkinStorageRootobject>(FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

            try
            {
                long weaponSkinValue = GetItemValue(userSkin.UserSkinEntries, rootSkinData);

                //Give user credits
                UserCreditsHandler.AddCredits(Context, weaponSkinValue);

                //Remove skin from inventory
                var filteredUserSkinEntries = userSkin.UserSkinEntries.Where(s => s.OwnerID == Context.Message.Author.Id).Where(s => s.OwnerID != Context.Message.Author.Id).ToList();

                //Write to file
                WriteUserSkinDataToFile(filteredUserSkinEntries);

                //Send receipt
                await Context.Channel.SendMessageAsync(
                    UserInteraction.BoldUserName(Context) + $", you sold your inventory" +
                    $" for **{UserBankingHandler.CreditCurrencyFormatter(weaponSkinValue)} Credits**");
            }
            catch (Exception)
            {
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

        private static void WriteUserSkinDataToFile(List<UserSkinEntry> skinEntries)
        {
            var filteredUserSkin = new UserSkinStorageRootobject
            {
                SkinAmount = 0,
                UserSkinEntries = skinEntries
            };

            XmlManager.ToXmlFile(filteredUserSkin, FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

        }

        private static void WriteUserSkinDataToFile(UserSkinStorageRootobject skinEntry)
        {
            XmlManager.ToXmlFile(skinEntry, FileAccessManager.GetFileLocation("UserSkinStorage.xml"));

        }
    }
}
