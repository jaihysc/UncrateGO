using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Finance.CurrencyManager;
using UncrateGo.Modules.Interaction;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit(1, 3, Measure.Seconds)]
    [UserStorageCheckerPrecondition]
    public class FinanceCommandModule : InteractiveBase
    {
        [Command("balance", RunMode = RunMode.Async)]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            long userCredits = UserCreditsHandler.GetUserCredits(Context);

            await Context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + $", you have **{UserBankingHandler.CreditCurrencyFormatter(userCredits)} Credits**");
        }

        [Command("moneyTransfer", RunMode = RunMode.Async)]
        [Alias("mt")]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {
            await UserCreditsHandler.TransferCredits(Context, targetUser, amount);
        }
    }
}
