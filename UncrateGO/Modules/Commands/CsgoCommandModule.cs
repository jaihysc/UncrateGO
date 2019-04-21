using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Csgo;

namespace UncrateGo.Modules.Commands
{
    [UserStorageCheckerPrecondition]
    public class CsgoCommandModule : InteractiveBase<SocketCommandContext>
    {
        [Command("open", RunMode = RunMode.Async)]
        public async Task OpenCaseAsync(int caseSelection = -1)
        {
            //Allow for case selection within the open command
            if (caseSelection != -1)
            {
                await CsgoCaseSelectionHandler.SelectOpenCase(Context, caseSelection.ToString(), null);
                await CsgoUnboxingHandler.OpenCase(Context);
            }
            else
            {
                //See if user has opened a case before, if not, send a help tip
                if (!CsgoCaseSelectionHandler.GetHasUserSelectedCase(Context)) await ReplyAndDeleteAsync($"Tip: Use `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context.Channel)}select` to select different cases to open", timeout: TimeSpan.FromSeconds(30));

                await CsgoUnboxingHandler.OpenCase(Context);
            }
        }

        [Command("reset", RunMode = RunMode.Async)]
        public async Task ResetUserAsync()
        {
            await Context.Message.Channel.SendMessageAsync(Context.Message.Author.Mention + ", are you sure you want to reset your profile?\nType **Y** to confirm");

            var userInput = await NextMessageAsync();

            //Check if user entered y
            if (userInput.Content.ToLower() == "y")
            {
                //Reset user's credits
                UserDataManager.SetUserCredit(Context.Message.Author.Id, 0);

                //Reset inventory
                var userSkinStorage = CsgoDataHandler.GetUserSkinStorage();
                List<UserSkinEntry> userSkinStorageNew = userSkinStorage.UserSkinEntries.Where(i => i.OwnerId != Context.Message.Author.Id).ToList();

                CsgoDataHandler.SetUserSkinStorage(new UserSkinStorage { UserSkinEntries = userSkinStorageNew });

                await Context.Message.Channel.SendMessageAsync(Context.Message.Author.Mention + ", your profile has been reset");
            }
            else
            {
                await Context.Message.Channel.SendMessageAsync(Context.Message.Author.Mention + ", profile reset cancelled");
            }
        }

        [Command("select", RunMode = RunMode.Async)]
        public async Task SelectOpenCaseAsync([Remainder]string input = null)
        {
            //Show list if input is null or provided input is not a number
            if (input == null || !int.TryParse(input, out _))
            {
                PaginatedMessage pager;
                if (input == null)
                {
                    pager = CsgoCaseSelectionHandler.ShowPossibleCases(Context.Channel);
                }
                else //Add filter if input is a string and not a number string
                {
                    pager = CsgoCaseSelectionHandler.ShowPossibleCases(Context.Channel, input);
                }


                //Send paginated message
                IUserMessage sentMessage = await PagedReplyAsync(pager, new ReactionList
                {
                    Forward = true,
                    Backward = true,
                });

                //Auto delete message after 1 minute
                DeleteMessage(sentMessage, TimeSpan.FromMinutes(1));

                //Get user response
                var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));

                if (response != null) await CsgoCaseSelectionHandler.SelectOpenCase(Context, response.ToString(), sentMessage);
            }
            else
            {
                await CsgoCaseSelectionHandler.SelectOpenCase(Context, input, null);
            }
        }

        [Command("drop", RunMode = RunMode.Async)]
        public async Task OpenDropAsync()
        {
            await CsgoUnboxingHandler.OpenDrop(Context);
        }

        [Command("inventory", RunMode = RunMode.Async)]
        public async Task DisplayInventoryAsync()
        {
            //Get paginated message
            var pager = CsgoInventoryManager.DisplayUserCsgoInventory(Context);

            //Send paginated message
            var sentMessage = await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Jump = true,
                Trash = true
            });

            //Auto delete message after 5 minutes
            DeleteMessage(sentMessage, TimeSpan.FromMinutes(5));
        }

        [Command("sell", RunMode = RunMode.Async)]
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
        public async Task BuyInventoryItemAsync([Remainder]string inventoryMarketHash)
        {
            await CsgoTransactionHandler.BuyItemFromMarketAsync(Context, inventoryMarketHash);
        }

        [Command("market", RunMode = RunMode.Async)]
        public async Task ShowItemMarketAsync([Remainder]string filterString = null)
        {
            var pager = CsgoTransactionHandler.GetCsgoMarketInventory(Context, filterString);

            //Send paginated message
            var sentMessage = await PagedReplyAsync(pager, new ReactionList
            {
                Jump = true,
                Forward = true,
                Backward = true,
                Trash = true
            });

            //Auto delete message after 5 minutes
            DeleteMessage(sentMessage, TimeSpan.FromMinutes(5));
        }

        [Command("view", RunMode = RunMode.Async)]
        public async Task ShowItemInfoAsync([Remainder]string filterString)
        {
            await CsgoTransactionHandler.DisplayCsgoItemStatistics(Context, filterString);
        }

        [Command("statistics", RunMode = RunMode.Async)]
        public async Task ShowUserStatisticsAsync([Remainder]string filterString = null)
        {
            await CsgoLeaderboardManager.DisplayUserStatsAsync(Context);
        }

        /// <summary>
        /// Deletes the specified sent message after the specified amount of time, do not await this for the program to keep running
        /// </summary>
        /// <param name="sentMessage"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private static void DeleteMessage(IUserMessage sentMessage, TimeSpan timeSpan)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeSpan);

                //This may throw an exception if the message has already been deleted
                try
                {
                    await sentMessage.DeleteAsync();
                }
                catch (Exception)
                {
                    EventLogger.LogMessage("Failed to delete sent message, perhaps the message was already deleted", EventLogger.LogLevel.Warning);
                }
            });
        }
    }
}
