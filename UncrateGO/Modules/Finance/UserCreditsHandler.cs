using Discord;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Interaction;
using UncrateGo.Modules.UserActions;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Finance.CurrencyManager
{
    public class UserCreditsHandler
    {
        /// <summary>
        /// Transfers credits from sender to target receiver
        /// </summary>
        /// <param name="context">Sender, typically the one who initiated the command</param>
        /// <param name="targetUser">A @mention of the receiver</param>
        /// <param name="amount">Amount to send to the receiver</param>
        /// <returns></returns>
        public static async Task TransferCredits(SocketCommandContext context, string targetUser, long amount)
        {
            if (amount <= 0)
            {
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you must send **1 or more** Credits**");
            }
            else if (GetUserCredits(context) - amount < 0)
            {
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you do not have enough money to send || **" + UserBankingHandler.CreditCurrencyFormatter(GetUserCredits(context)) + " Credits**");
            }
            else
            {
                var recipient = context.Guild.GetUser(MentionUtils.ParseUser(targetUser));

                //Check if recipient has a profile
                UserBankingHandler.CheckIfUserCreditProfileExists(recipient);

                //Subtract money from sender
                AddCredits(context, -amount);

                //AddCredits credits to receiver
                AddCredits(context, MentionUtils.ParseUser(targetUser), amount);

                //Send receipts to both parties
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("Transaction Receipt")
                    .WithDescription("​")
                    .WithColor(new Color(68, 199, 40))
                    .WithFooter(footer =>
                    {
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Duck Banking Inc.")
                            .WithIconUrl("https://freeiconshop.com/wp-content/uploads/edd/bank-flat.png");
                    })
                    .AddInlineField("Sender", context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5))
                    .AddInlineField("Id", context.Message.Author.Id)
                    .AddInlineField("Total Amount", $"-{UserBankingHandler.CreditCurrencyFormatter(amount)}")

                    .AddInlineField("Recipient", recipient.ToString().Substring(0, recipient.ToString().Length - 5))
                    .AddInlineField("​", recipient.Id)
                    .AddInlineField("​", UserBankingHandler.CreditCurrencyFormatter(amount))

                    .AddInlineField("​", "​")
                    .AddInlineField("​", "​");

                var embed = embedBuilder.Build();

                await context.Message.Author.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                await recipient.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //READ
        /// <summary>
        /// Returns the credits the specified user has
        /// </summary>
        /// <param name="context">This is the user, typically the sender</param>
        /// <returns></returns>
        public static long GetUserCredits(SocketCommandContext context)
        {
            var userStorage = UserDataManager.GetUserStorage();

            return userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit;
        }
        /// <summary>
        /// Returns the credits the specified user has
        /// </summary>
        /// <param name="userID">ID of the user to get</param>
        /// <returns></returns>
        public static long GetUserCredits(ulong userID)
        {
            var userStorage = UserDataManager.GetUserStorage();

            return userStorage.UserInfo[userID].UserBankingStorage.Credit;
        }

        //READ + WRITE
        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="context">This is the user, typically the sender</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(SocketCommandContext context, long setAmount)
        {
            var userStorage = UserDataManager.GetUserStorage();

            userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit = setAmount;

            //Write new credits amount 
            UserDataManager.WriteUserStorage(userStorage);
        }
        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="userId">Target user ID</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(ulong userId, long setAmount)
        {
            var userStorage = UserDataManager.GetUserStorage();

            userStorage.UserInfo[userId].UserBankingStorage.Credit = setAmount;

            //Write new credits amount 
            UserDataManager.WriteUserStorage(userStorage);
        }
        /// <summary>
        /// Sets input amount to user balance
        /// </summary>
        /// <param name="context">Used to determine channel to send messages to if necessary</param>
        /// <param name="guildID">Guild ID where the target user is in</param>
        /// <param name="userID">Target user ID</param>
        /// <param name="setAmount">Amount to set credits balance to</param>
        public static void SetCredits(SocketCommandContext context, ulong guildID, ulong userID, long setAmount)
        {
            //Get user credits to list
            var guild = context.Client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            //Get user credit storage
            var userStorage = UserDataManager.GetUserStorage();

            userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit = setAmount;

            //Write new credits amount 
            UserDataManager.WriteUserStorage(userStorage);
        }

        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="context">This is the user, typically the sender</param>
        /// <param name="addAmount">Amount to add</param>
        /// <param name="deductTaxes">Whether or not deduct taxes from the add amount, tax rate is set in FinanceConfigValues</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext context, long addAmount)
        {
            //Get user credit storage
            var userStorage = UserDataManager.GetUserStorage();

            //Check if user has sufficient credits
            if (GetUserCredits(context) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;
                userCreditsNew = userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit + addAmount;


                userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit = userCreditsNew;

                //Write new credits amount 
                UserDataManager.WriteUserStorage(userStorage);

                return true;
            }
            else
            {
                //False to indicate that user does not have enough credits to be deducted
                return false;
            }
        }
        /// <summary>
        /// Adds input amount to user balance, Note: deductTaxes is not supported with this overload, use one with SocketCommandContext for that functionality
        /// </summary>
        /// <param name="userID">Target user's discord ID</param>
        /// <param name="addAmount">Amount to add</param>
        /// <returns></returns>
        public static bool AddCredits(ulong userID, long addAmount)
        {
            //Get user credits
            var userStorage = UserDataManager.GetUserStorage();

            //Check if user has sufficient credits
            if (GetUserCredits(userID) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;

                userStorage.UserInfo[userID].UserBankingStorage.Credit = userCreditsNew;

                //Write new credits amount 
                UserDataManager.WriteUserStorage(userStorage);

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="Context">Used to determine channel to send messages to if necessary</param>
        /// <param name="guildID">Guild ID where the target user is in</param>
        /// <param name="userID">Target user ID</param>
        /// <param name="addAmount">Amount to add</param>
        /// <param name="deductTaxes">Whether or not deduct taxes from the add amount, tax rate is set in FinaceConfigValues</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext Context, ulong userID, long addAmount)
        {
            //Get user credits
            var userStorage = UserDataManager.GetUserStorage();

            //Check if user has sufficient credits
            if (GetUserCredits(Context) + addAmount > 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;

                userCreditsNew = userStorage.UserInfo[userID].UserBankingStorage.Credit + addAmount;

                userStorage.UserInfo[userID].UserBankingStorage.Credit = userCreditsNew;

                //Write new credits amount 
                UserDataManager.WriteUserStorage(userStorage);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
