using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Csgo;
using System;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit(1, 4, Measure.Seconds)]
    [UserStorageCheckerPrecondition]
    public class CaseCommandModule : InteractiveBase<SocketCommandContext>
    {

        [Command("open", RunMode = RunMode.Async)]
        [Alias("o")]
        public async Task OpenCaseAsync()
        {
            //See if user has opened a case before, if not, send a help tip
            if (!CsgoCaseSelectionHandler.GetHasUserSelectedCase(Context)) await ReplyAndDeleteAsync($"Tip: Use `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context)} cs case` to select different cases to open", timeout: TimeSpan.FromSeconds(60));

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
                Backward = true
            });
        }


        [Command("info", RunMode = RunMode.Async)]
        public async Task ShowItemInfoAsync([Remainder]string filterString)
        {
            await CsgoInventoryHandler.DisplayCsgoItemStatistics(Context, filterString);
        }
    }
}
