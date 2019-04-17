using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;
using Discord;
using System.Threading.Tasks;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public static class BankingHandler
    {
        private static void CheckIfUserCreditProfileExists(SocketGuildUser user)
        {
            var userStorage = UserDataManager.GetUserStorage();
            //Create txt user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(user.Id, out _))
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
        public static string CurrencyFormatter(long inputCredits)
        {
            //Formats number to use currency numeration
            var numberGroupSeparator = new NumberFormatInfo { NumberGroupSeparator = " " };
            return inputCredits.ToString("N0", numberGroupSeparator);
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
            ulong userId = context.Message.Author.Id;
            long userCredits = UserDataManager.GetUserCredit(userId);
            if (amount <= 0)
            {
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you must send **1 or more** Credits");
            }
            else if (userCredits - amount < 0)
            {
                await context.Message.Author.SendMessageAsync(UserInteraction.BoldUserName(context) + ", you do not have enough money to send || **" + CurrencyFormatter(userCredits) + " Credits**");
            }
            else
            {
                var recipient = context.Guild.GetUser(MentionUtils.ParseUser(targetUser));

                //Check if recipient has a profile
                CheckIfUserCreditProfileExists(recipient);

                //Subtract money from sender
                if (AddCredits(userId, -amount))
                {
                    //If successfully subtracted money, add credits to receiver
                    AddCredits(MentionUtils.ParseUser(targetUser), amount);

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
                        .AddInlineField("Total Amount", $"-{CurrencyFormatter(amount)}")

                        .AddInlineField("Recipient", recipient.ToString().Substring(0, recipient.ToString().Length - 5))
                        .AddInlineField("​", recipient.Id)
                        .AddInlineField("​", CurrencyFormatter(amount))

                        .AddInlineField("​", "​")
                        .AddInlineField("​", "​");

                    var embed = embedBuilder.Build();

                    await context.Message.Author.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                    await recipient.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Adds input amount to user balance
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="addAmount">Amount to add</param>
        /// <returns></returns>
        public static bool AddCredits(ulong userId, long addAmount)
        {
            //Check if user has sufficient credits
            long userCredits = UserDataManager.GetUserCredit(userId);
            if (userCredits >= 0 && userCredits + addAmount >= 0)
            {
                //Calculate new credits
                long userCreditsNew = CreditsCalculator(userCredits, addAmount);

                UserDataManager.SetUserCredit(userId, userCreditsNew);

                return true;
            }

            //False to indicate that user does not have enough credits to be deducted
            return false;
        }

        private static long CreditsCalculator(long baseCredits, long addAmount)
        {
            if (baseCredits + addAmount >= 0)
            {
                return baseCredits + addAmount;
            }

            return 0; //Return 0 if baseCredits + addAmount is below 0
        }
    }
}
