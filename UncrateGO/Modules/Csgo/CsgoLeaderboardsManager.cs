using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using UncrateGo.Core;

namespace UncrateGo.Modules.Csgo
{
    public static class CsgoLeaderboardManager
    {
        public enum CaseCategory
        {
            Case,
            Drop,
            Souvenir,
            Sticker
        }

        public enum ItemCategory
        {
            Default,
            Special,
            Sticker,
            Other
        }

        private static List<string> _leaderboardLeaders = new List<string>();

        /// <summary>
        ///     Increments the user stat tracker
        /// </summary>
        /// <param name="context"></param>
        /// <param name="itemListType"></param>
        /// <param name="itemCategory"></param>
        public static void IncrementStatTracker(SocketCommandContext context, ItemListType itemListType,
            ItemCategory itemCategory)
        {
            var userCaseStats = UserDataManager.GetUserCsgoStatsStorage(context.Message.Author.Id);

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
                if (itemListType.Rarity == Rarity.Covert && itemListType.BlackListWeaponType == WeaponType.Knife)
                    userCaseStats.Covert++;
                else if (itemListType.Rarity == Rarity.Covert && itemListType.WeaponType == WeaponType.Knife)
                    userCaseStats.Special++;
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
            UserDataManager.SetUserCsgoStatsStorage(context.Message.Author.Id, userCaseStats);
        }

        public static void IncrementCaseStatTracker(SocketCommandContext context, CaseCategory caseCategory)
        {
            var userCaseStats = UserDataManager.GetUserCsgoStatsStorage(context.Message.Author.Id);

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

            UserDataManager.SetUserCsgoStatsStorage(context.Message.Author.Id, userCaseStats);
        }

        /// <summary>
        ///     Displays the current user statistics
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task DisplayUserStatsAsync(SocketCommandContext context)
        {
            //Get case stats
            var userCaseStats = UserDataManager.GetUserCsgoStatsStorage(context.Message.Author.Id);

            string[] statFields =
            {
                "**Item Drops**", "**Cases Opened**", "**Souvenirs Opened**", "**Sticker Capsules Opened**",
                "Consumer Grade", "Industrial Grade", "MilSpec Grade", "Restricted", "Classified", "Covert", "Special",
                "Stickers", "Other"
            };

            //Add stats to string list
            var statFieldVal = new List<string>();
            List<string> statFieldLeadVal = _leaderboardLeaders;

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
                for (var i = 0; i < statFields.Count(); i++) statFieldVal.Add("0");
            }

            //If there are no leaders, leader stats list will print N/A
            if (statFieldLeadVal == null || !statFieldLeadVal.Any()) statFieldLeadVal = new List<string> {"N/A"};

            //Send embed
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(255, 127, 80))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + context.Message.Author.ToString())
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


        /// <summary>
        ///     Finds the leaderboard leaders, returns a list of strings ready to be displayed in embed
        /// </summary>
        public static void GetStatisticsLeader(object state)
        {
            try
            {
                //Todo Possibly rewrite this later to be more effective with a dictionary instead

                var userData = UserDataManager.GetUserStorage();

                //Variables to store the leaders and their values
                var casesOpened = new LeaderboardData();
                var souvenirsOpened = new LeaderboardData();
                var dropsOpened = new LeaderboardData();
                var sticksOpened = new LeaderboardData();

                //Find the leaders
                foreach (var user in userData.UserInfo.Values)
                    if (user.UserCsgoStatsStorage != null)
                    {
                        //Cases opened
                        FindEntryLeader(user, user.UserCsgoStatsStorage.CasesOpened, casesOpened);
                        //Souvenirs opened
                        FindEntryLeader(user, user.UserCsgoStatsStorage.SouvenirsOpened, souvenirsOpened);
                        //Drops opened
                        FindEntryLeader(user, user.UserCsgoStatsStorage.DropsOpened, dropsOpened);
                        //StickersOpened
                        FindEntryLeader(user, user.UserCsgoStatsStorage.StickersOpened, sticksOpened);
                    }

                //Generate the string to return
                var returnString = new List<string>
                {
                    casesOpened.Value + " <@" + casesOpened.UserId + ">",
                    souvenirsOpened.Value + " <@" + souvenirsOpened.UserId + ">",
                    dropsOpened.Value + " <@" + dropsOpened.UserId + ">",
                    sticksOpened.Value + " <@" + sticksOpened.UserId + ">"
                };

                _leaderboardLeaders = returnString;
            }
            catch
            {
                EventLogger.LogMessage("Unable to update statistics", ConsoleColor.Red);
            }
        }

        /// <summary>
        ///     Helper method to find the leaders for each category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="comparisonInputNew"></param>
        /// <param name="comparisonInputOriginal"></param>
        /// <returns></returns>
        private static void FindEntryLeader(UserInfo user, long comparisonInputNew,
            LeaderboardData comparisonInputOriginal)
        {
            if (comparisonInputNew > comparisonInputOriginal.Value)
            {
                comparisonInputOriginal.Value = comparisonInputNew;
                comparisonInputOriginal.UserId = user.UserId;
            }
        }

        private class LeaderboardData
        {
            public long Value { get; set; }
            public ulong UserId { get; set; }
        }
    }
}