using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Csgo;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit(1, 2, Measure.Seconds)]
    [UserStorageCheckerPrecondition]
    public class CommandModule : InteractiveBase<SocketCommandContext>
    {
        //Help
        [Command("help")]
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
        [Command("prefix")]
        public async Task ChangeGuildCommandPrefixAsync([Remainder]string input)
        {
            //Find guild id
            var chnl = Context.Channel as SocketGuildChannel;

            //Make sure invoker is owner of guild
            if (chnl.Guild.OwnerId == Context.Message.Author.Id)
            {
                GuildCommandPrefixManager.ChangeGuildCommandPrefix(Context, input);
                await Context.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + $", server prefix has successfully been changed to `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context)}`");
            }
            //Otherwise send error
            else
            {
                await Context.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + ", only the server owner may invoke this command");
            }
        }

        [Command("info")]
        public async Task InfoAsync()
        {
            await Context.Channel.SendMessageAsync("Invite: https://discordapp.com/oauth2/authorize?client_id=523282498265022479&permissions=337984&scope=bot \nBy <@285266023475838976> | Framework: Discord.NET V1.0.2 | Github: github.com/jaihysc/Discord-UncrateGO");
        }

        //Finance
        [Command("balance", RunMode = RunMode.Async)]
        [Alias("bal")]
        public async Task SlotBalanceAsync()
        {
            long userCredits = BankingHandler.GetUserCredits(Context);

            await Context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + $", you have **{BankingHandler.CreditCurrencyFormatter(userCredits)} Credits**");
        }

        [Command("moneyTransfer", RunMode = RunMode.Async)]
        [Alias("mt")]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {
            await BankingHandler.TransferCredits(Context, targetUser, amount);
        }

        //Cases
        [Command("open", RunMode = RunMode.Async)]
        [Alias("o")]
        public async Task OpenCaseAsync()
        {
            //See if user has opened a case before, if not, send a help tip
            if (!CsgoCaseSelectionHandler.GetHasUserSelectedCase(Context)) await ReplyAndDeleteAsync($"Tip: Use `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context)}select` to select different cases to open", timeout: TimeSpan.FromSeconds(30));

            await CsgoUnboxingHandler.OpenCase(Context);
        }


        [Command("select", RunMode = RunMode.Async)]
        public async Task SelectOpenCaseAsync()
        {
            var pager = CsgoCaseSelectionHandler.ShowPossibleCases(Context);

            //Send paginated message
            Discord.IUserMessage sentMessage = await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
            });

            //Get user response
            var response = await NextMessageAsync();

            await CsgoCaseSelectionHandler.SelectOpenCase(Context, response.ToString(), sentMessage);
        }


        [Command("drop", RunMode = RunMode.Async)]
        [Alias("d")]
        public async Task OpenDropAsync()
        {
            await CsgoUnboxingHandler.OpenDrop(Context);
        }



        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv", "i")]
        public async Task DisplayInventoryAsync()
        {
            //Get paginated message
            var pager = CsgoInventoryHandler.DisplayUserCsgoInventory(Context);

            //Send paginated message
            await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Jump = true,
                Trash = true
            });
        }


        [Command("sell", RunMode = RunMode.Async)]
        [Alias("s")]
        public async Task SellInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            if (inventoryMarketHash == "*")
            {
                await CsgoTransactionHandler.SellAllInventoryItemAsync(Context);
            }
            else
            {
                await CsgoTransactionHandler.SellInventoryItemAsync(Context, inventoryMarketHash);
            }           
        }


        [Command("sellall", RunMode = RunMode.Async)]
        [Alias("sa")]
        public async Task SellAllSelectedInventoryItemAsync([Remainder]string inventoryMarketHash = null)
        {
            //If user does not specify filter, sell all
            if (inventoryMarketHash == null)
            {
                await CsgoTransactionHandler.SellAllInventoryItemAsync(Context);
            }
            else
            {
                await CsgoTransactionHandler.SellAllSelectedInventoryItemAsync(Context, inventoryMarketHash);
            }           
        }


        [Command("buy", RunMode = RunMode.Async)]
        [Alias("b")]
        public async Task BuyInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            await CsgoTransactionHandler.BuyItemFromMarketAsync(Context, inventoryMarketHash);
        }


        [Command("market", RunMode = RunMode.Async)]
        [Alias("m")]
        public async Task ShowItemMarketAsync([Remainder]string filterString = null)
        {
            var pager = CsgoInventoryHandler.GetCsgoMarketInventory(Context, filterString);

            //Send paginated message
            await PagedReplyAsync(pager, new ReactionList
            {
                Jump = true,
                Forward = true,
                Backward = true,
                Trash = true
            });
        }


        [Command("view", RunMode = RunMode.Async)]
        public async Task ShowItemInfoAsync([Remainder]string filterString)
        {
            await CsgoInventoryHandler.DisplayCsgoItemStatistics(Context, filterString);
        }

        [Command("statistics", RunMode = RunMode.Async)]
        public async Task ShowUserStatisticsAsync([Remainder]string filterString = null)
        {
            await CsgoDataHandler.DisplayUserStatsAsync(Context);
        }

    }
}
