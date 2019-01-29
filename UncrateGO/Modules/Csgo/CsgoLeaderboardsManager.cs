using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncrateGo.Core;
using UncrateGo.Models;

namespace UncrateGo.Modules.Csgo
{
    public class CsgoLeaderboardsManager
    {
        private static List<string> LeaderboardsLeaders = new List<string>();

        /// <summary>
        /// Increments the user stat tracker
        /// </summary>
        /// <param name="itemListType"></param>
        public static void IncrementStatTracker(SocketCommandContext context, ItemListType itemListType, ItemCategory itemCategory)
        {
            var userCaseStats = GetUserCsgoStatsStorage(context);

            //If using default category
            if (itemCategory == ItemCategory.Default)
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

                //Increment knife or covert counter
                if (itemListType.Rarity == Rarity.Covert && itemListType.BlackListWeaponType == WeaponType.Knife) userCaseStats.Covert++;
                else if (itemListType.Rarity == Rarity.Covert && itemListType.WeaponType == WeaponType.Knife) userCaseStats.Special++;
            }
            //If not
            else
            {
                switch (itemCategory)
                {
                    case ItemCategory.Special:
                        userCaseStats.Special++;
                        break;
                    case ItemCategory.Sticker:
                        userCaseStats.Stickers++;
                        break;
                    case ItemCategory.Other:
                        userCaseStats.Other++;
                        break;
                }    

            }

            //Set stats back to master list
            SetUserCsgoStatsStorage(userCaseStats, context);
        }

        public static void IncrementCaseStatTracker(SocketCommandContext context, CaseCategory caseCategory)
        {
            var userCaseStats = GetUserCsgoStatsStorage(context);

            switch (caseCategory)
            {
                case CaseCategory.Case:
                    userCaseStats.CasesOpened++;
                    break;
                case CaseCategory.Drop:
                    userCaseStats.DropsOpened++;
                    break;
                case CaseCategory.Souvenir:
                    userCaseStats.SouvenirsOpened++;
                    break;
                case CaseCategory.Sticker:
                    userCaseStats.StickersOpened++;
                    break;
            }

            SetUserCsgoStatsStorage(userCaseStats, context);
        }

        private static UserCsgoStatsStorage GetUserCsgoStatsStorage(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Get case stats
            var userCaseStats = userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage;

            if (userCaseStats == null) userCaseStats = new UserCsgoStatsStorage();

            return userCaseStats;
        }

        private static void SetUserCsgoStatsStorage(UserCsgoStatsStorage input, SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage = input;

            UserDataManager.SetUserStorage(userStorage);
        }

        /// <summary>
        /// Displays the current user statistics
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task DisplayUserStatsAsync(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Get case stats
            var userCaseStats = userStorage.UserInfo[context.Message.Author.Id].UserCsgoStatsStorage;

            string[] statFields = { "**Item Drops**", "**Cases Opened**", "**Souvenirs Opened**", "**Sticker Capsules Opened**", "Consumer Grade", "Industrial Grade", "MilSpec Grade", "Restricted", "Classified", "Covert", "Special", "Stickers", "Other" };

            //Add stats to string list
            List<string> statFieldVal = new List<string>();
            List<string> statFieldLeadVal = LeaderboardsLeaders;

            if (userCaseStats != null)
            {
                statFieldVal.Add(userCaseStats.DropsOpened.ToString());
                statFieldVal.Add(userCaseStats.CasesOpened.ToString());
                statFieldVal.Add(userCaseStats.SouvenirsOpened.ToString());
                statFieldVal.Add(userCaseStats.StickersOpened.ToString());

                statFieldVal.Add(userCaseStats.ConsumerGrade.ToString());
                statFieldVal.Add(userCaseStats.IndustrialGrade.ToString());
                statFieldVal.Add(userCaseStats.MilSpecGrade.ToString());
                statFieldVal.Add(userCaseStats.Restricted.ToString());
                statFieldVal.Add(userCaseStats.Classified.ToString());
                statFieldVal.Add(userCaseStats.Covert.ToString());
                statFieldVal.Add(userCaseStats.Special.ToString());
                statFieldVal.Add(userCaseStats.Stickers.ToString());
                statFieldVal.Add(userCaseStats.Other.ToString());
            }
            else
            {
                for (int i = 0; i < statFields.Count(); i++)
                {
                    statFieldVal.Add("0");
                }
                
            }

            //If there are no leaders, leader stats list will print N/A
            if (statFieldLeadVal == null || statFieldLeadVal.Count() == 0)
            {
                statFieldLeadVal.Add("N/A");
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
                .AddField("\u200b", string.Join("\n", statFieldVal), true)
                .AddField("Top", string.Join("\n", statFieldLeadVal), true);

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static async Task GetStatisticsLeaderTimer()
        {
            while (true)
            {
                await Task.Delay(30000);
                GetStatisticsLeader();
            }
        }

        /// <summary>
        /// Finds the leaderboard leaders, returns a list of strings ready to be displayed in embed
        /// </summary>
        private static void GetStatisticsLeader()
        {
            //!!! Possibly rewrite this later to be more effective with a dictionary instead

            var userData = UserDataManager.GetUserStorage();

            //Variables to store the leaders and their values
            LeaderboardData casesOpened = new LeaderboardData();
            LeaderboardData souvenirsOpened = new LeaderboardData();
            LeaderboardData dropsOpened = new LeaderboardData();
            LeaderboardData sticksOpened = new LeaderboardData();

            //Find the leaders
            foreach (var item in userData.UserInfo.Values)
            {
                //Cases opened
                FindEntryLeader(item, item.UserCsgoStatsStorage.CasesOpened, casesOpened);
                //Souvenirs opened
                FindEntryLeader(item, item.UserCsgoStatsStorage.SouvenirsOpened, souvenirsOpened);
                //Drops opened
                FindEntryLeader(item, item.UserCsgoStatsStorage.DropsOpened, dropsOpened);
                //StickersOpened
                FindEntryLeader(item, item.UserCsgoStatsStorage.StickersOpened, sticksOpened);
            }

            //Generate the string to return
            List<string> returnString = new List<string>();
            returnString.Add(casesOpened.Value + " <@" + casesOpened.UserID + ">");
            returnString.Add(souvenirsOpened.Value + " <@" + souvenirsOpened.UserID + ">");
            returnString.Add(dropsOpened.Value + " <@" + dropsOpened.UserID + ">");
            returnString.Add(sticksOpened.Value + " <@" + sticksOpened.UserID + ">");

            LeaderboardsLeaders = returnString;
        }

        /// <summary>
        /// Helper method to find the leaders for each category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="comparisonInputNew"></param>
        /// <param name="comparisonInputOriginal"></param>
        /// <returns></returns>
        private static LeaderboardData FindEntryLeader(UserInfo user, long comparisonInputNew, LeaderboardData comparisonInputOriginal)
        {
            if (comparisonInputNew > comparisonInputOriginal.Value)
            {
                comparisonInputOriginal.Value = comparisonInputNew;
                comparisonInputOriginal.UserID = user.UserId;
            }

            return comparisonInputOriginal;
        }

        private class LeaderboardData
        {
            public long Value { get; set; }
            public ulong UserID { get; set; }
        }

        public enum CaseCategory { Case, Drop, Souvenir, Sticker};

        public enum ItemCategory { Default, Special, Sticker, Other};
    }
}
