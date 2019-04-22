using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Csgo;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace UncrateGo.Modules.Commands
{
    [UserStorageCheckerPrecondition]
    public class CommandModule : InteractiveBase<SocketCommandContext>
    {
        [RequireOwner]
        [Group("mana")]
        class OwnerModule : InteractiveBase<SocketCommandContext>
        {
            [RequireOwner]
            [Command("setInfo", RunMode = RunMode.Async)]
            public async Task SetInfoAsync([Remainder]string input = null)
            {
                await Context.Client.SetGameAsync(input);
            }

            [RequireOwner]
            [Command("reload", RunMode = RunMode.Async)]
            public async Task ReloadDataAsync()
            {
                EventLogger.LogMessage("Reloading data from file...", EventLogger.LogLevel.Info);

                CsgoDataHandler.GetCsgoCosmeticData();
                UserDataManager.GetUserStorage();
                CsgoDataHandler.GetUserSkinStorage();
                GuildCommandPrefixManager.PopulateGuildCommandPrefix();

                CsgoDataUpdater.UpdateRootWeaponSkin();
                EventLogger.LogMessage("Reloading data from file...DONE!", EventLogger.LogLevel.Info);
            }

            [RequireOwner]
            [Command("flush", RunMode = RunMode.Async)]
            public async Task FlushDataAsync()
            {
                MainProgram.FlushAllData(null);
            }
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task HelpAsync([Remainder]string inputCommand = null)
        {
            if (!string.IsNullOrEmpty(inputCommand))
            {
                await UserHelpHandler.DisplayCommandHelpMenu(Context, inputCommand);
            }
            else
            {
                await UserHelpHandler.DisplayHelpMenu(Context);
            }
        }

        //Settings
        [Command("prefix", RunMode = RunMode.Async)]
        public async Task ChangeGuildCommandPrefixAsync([Remainder]string input)
        {
            string userName = UserInteraction.BoldUserName(Context.Message.Author.ToString());

            //Make sure invoker is owner of guild
            if (Context.Channel is SocketGuildChannel chnl && chnl.Guild.OwnerId == Context.Message.Author.Id)
            {
                GuildCommandPrefixManager.ChangeGuildCommandPrefix(Context, input);
                await Context.Channel.SendMessageAsync(userName + $", server prefix has successfully been changed to `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context.Channel)}`");
            }
            //Otherwise send error
            else
            {
                await Context.Channel.SendMessageAsync(userName + ", only the server owner may invoke this command");
            }
        }

        [Command("info", RunMode = RunMode.Async)]
        public async Task InfoAsync()
        {
            await Context.Channel.SendMessageAsync("Invite: https://discordapp.com/oauth2/authorize?client_id=523282498265022479&permissions=337984&scope=bot \nBy <@285266023475838976> | Framework: Discord.NET V1.0.2 | Github: github.com/jaihysc/Discord-UncrateGO");
        }

        //Finance
        [Command("balance", RunMode = RunMode.Async)]
        public async Task BalanceAsync()
        {
            string userName = UserInteraction.BoldUserName(Context.Message.Author.ToString());

            long userCredits = UserDataManager.GetUserCredit(Context.Message.Author.Id);

            await Context.Message.Channel.SendMessageAsync(userName + $", you have **{BankingHandler.CurrencyFormatter(userCredits)} Credits**");
        }

        [Command("moneyTransfer", RunMode = RunMode.Async)]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {
            await BankingHandler.TransferCredits(Context, targetUser, amount);
        }
    }
}
