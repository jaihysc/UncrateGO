using Discord;
using Discord.Commands;
using DuckBot.Core;
using DuckBot.Models;
using DuckBot.Modules.Interaction;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using DuckBot_ClassLibrary.Modules;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Modules.Finance.CurrencyManager
{
    public class UserDebtHandler
    {
        /// <summary>
        /// Allows the user to borrow credits
        /// </summary>
        /// <param name="context">Invoke data for the user</param>
        /// <param name="borrowAmount">Amount to borrow for the user</param>
        /// <returns></returns>
        public static async Task BorrowCredits(SocketCommandContext context, long borrowAmount)
        {
            if (GetUserCreditsDebt(context) + borrowAmount > long.Parse(SettingsManager.RetrieveFromConfigFile("maxBorrow")))
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you have exceeded your credit limit of **{UserBankingHandler.CreditCurrencyFormatter(long.Parse(SettingsManager.RetrieveFromConfigFile("maxBorrow")))} Credits**");
            }
            else if (borrowAmount <= 0)
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you have to borrow **1 or more** Credits");
            }
            else
            {
                //Add to debt counter
                AddDebt(context, borrowAmount);
                //Add credits to user
                UserCreditsHandler.AddCredits(context, borrowAmount);

                //Send receipt
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you borrowed **{UserBankingHandler.CreditCurrencyFormatter(borrowAmount)} Credits**");
            }
        }

        /// <summary>
        /// Allows the user to pay back their credits borrowed
        /// </summary>
        /// <param name="context">Invoke data for the user</param>
        /// <param name="returnAmount">Amount to return for the user</param>
        /// <returns></returns>
        public static async Task ReturnCredits(SocketCommandContext context, long returnAmount)
        {
            if (returnAmount > GetUserCreditsDebt(context))
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you do not owe **{UserBankingHandler.CreditCurrencyFormatter(returnAmount)} Credits** || **{UserBankingHandler.CreditCurrencyFormatter(GetUserCreditsDebt(context))} Credits**");
            }
            else if (returnAmount <= 0)
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you have to pay back **1 or more** Credits");
            }
            else if (returnAmount > UserCreditsHandler.GetUserCredits(context))
            {
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you do not have enough credits to pay back || **{UserCreditsHandler.GetUserCredits(context)}** Credits");
            }
            else
            {
                //Subtract from debt counter
                AddDebt(context, -returnAmount);
                //Subtract credits to user
                UserCreditsHandler.AddCredits(context, -returnAmount);

                //Send receipt
                await context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(context) + $", you paid back **{UserBankingHandler.CreditCurrencyFormatter(returnAmount)} Credits**");
            }
        }

        //READ
        /// <summary>
        /// Retrieves the user debt from storage
        /// </summary>
        /// <param name="context">invoke data from the user</param>
        /// <returns></returns>
        public static long GetUserCreditsDebt(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            return userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.CreditDebt;
        }
        /// <summary>
        /// Retrieves the user debt from storage
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public static long GetUserCreditsDebt(ulong userId)
        {
            var userStorage = UserDataManager.GetUserStorage();

            return userStorage.UserInfo[userId].UserBankingStorage.CreditDebt;
        }

        //READ + WRITE
        /// <summary>
        /// Sets the user debt
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <param name="setAmount">Amount to set debt to</param>
        public static void SetDebt(SocketCommandContext context, long setAmount)
        {
            //Get user debt to list
            var userStorage = UserDataManager.GetUserStorage();

            //Set debt
            userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.CreditDebt = setAmount;

            //Write new debt amount 
            UserDataManager.WriteUserStorage(userStorage);
        }

        /// <summary>
        /// Sets the user debt
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <param name="addAmount">Amount of debt to add</param>
        public static void AddDebt(SocketCommandContext context, long addAmount)
        {
            //Get user debt to list
            var userStorage = UserDataManager.GetUserStorage();

            //Calculate new debt balance
            userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.CreditDebt = userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.CreditDebt + addAmount;

            //Write new debt amount 
            UserDataManager.WriteUserStorage(userStorage);
        }
    }
}
