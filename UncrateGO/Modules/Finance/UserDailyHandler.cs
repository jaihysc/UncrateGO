using Discord.Commands;
using System;
using System.Threading.Tasks;
using UncrateGo.Modules.UserActions;
using UncrateGo.Modules.Finance.CurrencyManager;
using UncrateGo.Core;
using UncrateGo.Modules.Interaction;

namespace UncrateGo.Modules.Finance
{
    public class UserDailyHandler : ModuleBase<SocketCommandContext>
    {
        //Daily
        public static async Task GiveDailyCreditsAsync(SocketCommandContext context)
        {
            //Get user storage
            var userStorage = UserDataManager.GetUserStorage();

            //If 24 hours has passed
            if (userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime.AddHours(24) < DateTime.UtcNow)
            {
                //Add credits
                UserCreditsHandler.AddCredits(context, long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount")));

                //Write last use date
                userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime = DateTime.UtcNow;


                //Write new credits and last redeem date to file
                userStorage = UserDataManager.GetUserStorage();
                userStorage.UserInfo[context.Message.Author.Id].UserDailyLastUseStorage.DateTime = DateTime.UtcNow;
                UserDataManager.WriteUserStorage(userStorage);


                //Send channel message confirmation
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you have redeemed your daily **" + UserBankingHandler.CreditCurrencyFormatter(long.Parse(SettingsManager.RetrieveFromConfigFile("dailyAmount"))) + " Credits!**");

            }
            else
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + ", it has not yet been 24 hours since you last redeemed");
            }

        }
    }
}
