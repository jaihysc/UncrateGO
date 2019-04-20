using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using System.Collections.Generic;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoInventoryManager
    {
        private static List<string> _embedFieldsMaster = new List<string>();
        private static List<string> _embedPriceFieldsMaster = new List<string>();

        public static PaginatedMessage DisplayUserCsgoInventory(SocketCommandContext context)
        {
            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel);

            //Reset fields
            _embedFieldsMaster = new List<string>();
            _embedPriceFieldsMaster = new List<string>();

            //Get user skins from json
            var userSkin = CsgoDataHandler.GetUserSkinStorage();

            var foundUserSkins = new List<UserSkinEntry>();

            //Filter userSkinEntries xml file down to skins belonging to sender
            foreach (var userSkinEntry in userSkin.UserSkinEntries)
            {
                //Filter skin search to those owned by user
                if (userSkinEntry.OwnerId == context.Message.Author.Id)
                {
                    foundUserSkins.Add(new UserSkinEntry { OwnerId = context.Message.Author.Id, ClassId = userSkinEntry.ClassId, UnboxDate = userSkinEntry.UnboxDate, MarketName = userSkinEntry.MarketName });
                }
            }

            //Generate fields
            AddSkinFieldEntry(foundUserSkins);

            //Configure paginated message
            var paginationConfig = new PaginationConfig
            {
                AuthorName = context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5) + " Inventory",
                AuthorUrl = context.Message.Author.GetAvatarUrl(),

                Description = $"Sell items: `{botCommandPrefix}sell [name]` \n Sell all items matching filter: `{botCommandPrefix}sellall [name]`",

                DefaultFieldHeader = "You do not have any items",
                DefaultFieldDescription = $"Go unbox some with `{botCommandPrefix}open` or `{botCommandPrefix}drop`",

                Field1Header = "Name",
                Field2Header = "Market Value",

                Color = new Color(0, 153, 0)
            };

            var paginationManager = new PaginationManager();

            //Generate paginated message
            var pager = paginationManager.GeneratePaginatedMessage(_embedFieldsMaster, _embedPriceFieldsMaster, paginationConfig);

            return pager;
        }

        private static void AddSkinFieldEntry(List<UserSkinEntry> foundUserSkins)
        {
            var rootWeaponSkin = CsgoDataHandler.GetCsgoCosmeticData();

            //Find every item belonging to sender
            foreach (var item in foundUserSkins)
            {
                SkinDataItem skinDataItem = new SkinDataItem();

                //Try to index with the more efficient dictionary first
                if (item.MarketName == null || !rootWeaponSkin.ItemsList.TryGetValue(item.MarketName, out skinDataItem)) //TODO add better handling of unicode
                {
                    //Try to index with the more efficient dictionary, then resort to the more tedious process via class ids

                    bool foundItem = false;
                    //Find skin entry info
                    foreach (var storageSkinEntry in rootWeaponSkin.ItemsList.Values)
                    {
                        if (storageSkinEntry.Classid == item.ClassId)
                        {
                            skinDataItem = storageSkinEntry;
                            foundItem = true;
                        }
                    }

                    //Send warning if item could not be found
                    if (!foundItem) EventLogger.LogMessage($"An item with the id {item.ClassId} could not be found", EventLogger.LogLevel.Warning);
                }

                if (skinDataItem == null)
                {
                    skinDataItem = new SkinDataItem();
                }

                //Add skin entry
                if (string.IsNullOrWhiteSpace(skinDataItem.Name)) continue; //Skip if name is not defined
                try
                {
                    //Add rarity emotes
                    Emote emote = Emote.Parse(GetEmoteBySkinRarity(skinDataItem.Rarity, skinDataItem.WeaponType));

                    //Add skin entry to list
                    _embedFieldsMaster.Add(emote + " " + skinDataItem.Name); //Looks like this: [EMOTE Abcdefghijklmn]

                    //Filter and Add skin price entry to list
                    if (skinDataItem.Price?.AllTime != null) //If skinDataItem.price isn't null and AllTime isn't null with the ?
                    {
                        double price = skinDataItem.Price.AllTime.Average;
                        _embedPriceFieldsMaster.Add(emote + " " + price);
                    }
                    else
                    {
                        _embedPriceFieldsMaster.Add(emote + " " + "N/A"); //Use N/A if price cannot be found
                    }
                        
                }
                catch
                {
                    EventLogger.LogMessage("Unable to add emotes" , EventLogger.LogLevel.Error);
                }
            }

        }

        public static string GetEmoteBySkinRarity(Rarity rarity, WeaponType? weaponType)
        {
            string skinQualityEmote = "<:white:522875796319240193>";

            //Assign quality colors
            if (rarity == Rarity.ConsumerGrade) skinQualityEmote = "<:white:522875796319240193>"; //white
            else if (rarity == Rarity.IndustrialGrade) skinQualityEmote = "<:lightblue:522878230848602131>"; //light blue
            else if (rarity == Rarity.MilSpecGrade) skinQualityEmote = "<:darkerblue:522878230550544387>"; //darker blue
            else if (rarity == Rarity.Restricted) skinQualityEmote = "<:purple:522878233482625034>"; //purple
            else if (rarity == Rarity.Classified) skinQualityEmote = "<:pink:522878230856990807>"; //pink
            else if (rarity == Rarity.Covert && weaponType != WeaponType.Knife) skinQualityEmote = "<:red:522878230533767199>"; //red
            else if (rarity == Rarity.Covert && weaponType == WeaponType.Knife) skinQualityEmote = "<:gold:522878230634692619>"; //gold
            else if (rarity == Rarity.Contraband) skinQualityEmote = "<:yellowgold:522878230923968513>"; //rare gold

            return skinQualityEmote;
        }
    }
}
