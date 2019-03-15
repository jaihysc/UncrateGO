using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;
using Discord;
using System.Threading.Tasks;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public class BankingHandler
    {
        public static void CheckIfUserCreditProfileExists(SocketGuildUser user)
        {
            var userStorage = UserDataManager.GetUserStorage();
            //Create txt user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(user.Id, out var i))
            {
                //Create user profile
                UserDataManager.CreateNewUserEntry(user);
            }
        }

        /// <summary>
        /// Formats the currency with spaces, as well as a decimal place
        /// </summary>
        /// <param name="inputCredits"></param>
        /// <returns></returns>
        public static string CreditCurrencyFormatter(long inputCredits)
        {
            //Formats number to use currency numeration
            var numberGroupSeperator = new NumberFormatInfo { NumberGroupSeparator = " " };
            return inputCredits.ToString("N0", numberGroupSeperator);
        }

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
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you must send **1 or more** Credits");
            }
            else if (GetUserCredits(context) - amount < 0)
            {
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you do not have enough money to send || **" + BankingHandler.CreditCurrencyFormatter(GetUserCredits(context)) + " Credits**");
            }
            else
            {
                var recipient = context.Guild.GetUser(MentionUtils.ParseUser(targetUser));

                //Check if recipient has a profile
                BankingHandler.CheckIfUserCreditProfileExists(recipient);

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
                            .WithName("UncrateGO Banking")
                            .WithIconUrl("https://freeiconshop.com/wp-content/uploads/edd/bank-flat.png");
                    })
                    .AddInlineField("Sender", context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5))
                    .AddInlineField("Id", context.Message.Author.Id)
                    .AddInlineField("Total Amount", $"-{BankingHandler.CreditCurrencyFormatter(amount)}")

                    .AddInlineField("Recipient", recipient.ToString().Substring(0, recipient.ToString().Length - 5))
                    .AddInlineField("​", recipient.Id)
                    .AddInlineField("​", BankingHandler.CreditCurrencyFormatter(amount))

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

        //READ + WRITE
        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="context">This is the user, typically the sender</param>
        /// <param name="addAmount">Amount to add</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext context, long addAmount)
        {
            //Get user credit storage
            var userStorage = UserDataManager.GetUserStorage();

            //Check if user has sufficient credits
            if (GetUserCredits(context) + addAmount >= 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;
                userCreditsNew = userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit + addAmount;


                userStorage.UserInfo[context.Message.Author.Id].UserBankingStorage.Credit = userCreditsNew;

                //Set new credits amount 
                UserDataManager.SetUserStorage(userStorage);

                return true;
            }
            else
            {
                //False to indicate that user does not have enough credits to be deducted
                return false;
            }
        }

        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="context">Used to determine channel to send messages to if necessary</param>
        /// <param name="guildID">Guild ID where the target user is in</param>
        /// <param name="userID">Target user ID</param>
        /// <param name="addAmount">Amount to add</param>
        /// <returns></returns>
        public static bool AddCredits(SocketCommandContext context, ulong userID, long addAmount)
        {
            //Get user credits
            var userStorage = UserDataManager.GetUserStorage();

            //Check if user has sufficient credits
            if (GetUserCredits(context) + addAmount >= 0)
            {
                //Calculate new credits
                long userCreditsNew = 0;

                userCreditsNew = userStorage.UserInfo[userID].UserBankingStorage.Credit + addAmount;

                userStorage.UserInfo[userID].UserBankingStorage.Credit = userCreditsNew;

                //Set new credits amount 
                UserDataManager.SetUserStorage(userStorage);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
