﻿using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Core;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Csgo;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using UncrateGo.Models;
using Discord;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit()]
    [UserStorageCheckerPrecondition]
    public class CommandModule : InteractiveBase<SocketCommandContext>
    {
        [RequireOwner]
        [Command("setInfo")]
        public async Task SetInfoAsync([Remainder]string input)
        {
            await Context.Client.SetGameAsync(input);
        }

        [RequireOwner]
        [Command("flush")]
        public async Task FlushDataAsync()
        {
            MainProgram.FlushAllData();
        }

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
        public async Task SlotBalanceAsync()
        {
            long userCredits = BankingHandler.GetUserCredits(Context);

            await Context.Message.Channel.SendMessageAsync(UserInteraction.BoldUserName(Context) + $", you have **{BankingHandler.CreditCurrencyFormatter(userCredits)} Credits**");
        }

        [Command("moneyTransfer", RunMode = RunMode.Async)]
        public async Task MoneyTransferAsync(string targetUser, long amount)
        {
            await BankingHandler.TransferCredits(Context, targetUser, amount);
        }

        //Cases
        [Command("open", RunMode = RunMode.Async)]
        public async Task OpenCaseAsync()
        {
            //See if user has opened a case before, if not, send a help tip
            if (!CsgoCaseSelectionHandler.GetHasUserSelectedCase(Context)) await ReplyAndDeleteAsync($"Tip: Use `{GuildCommandPrefixManager.GetGuildCommandPrefix(Context)}select` to select different cases to open", timeout: TimeSpan.FromSeconds(30));

            await CsgoUnboxingHandler.OpenCase(Context);
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
                var userStorage = UserDataManager.GetUserStorage();

                userStorage.UserInfo[Context.Message.Author.Id].UserBankingStorage.Credit = 0;

                //Reset inventory
                var userSkinStorage = CsgoDataHandler.GetUserSkinStorage();
                var userSkinStorageNew = userSkinStorage.UserSkinEntries.Where(i => i.OwnerID != Context.Message.Author.Id).ToList();

                CsgoDataHandler.SetUserSkinStorage(new UserSkinStorage { UserSkinEntries = userSkinStorageNew });

                await Context.Message.Channel.SendMessageAsync(Context.Message.Author.Mention + ", your profile has been reset");
            }
            else
            {
                await Context.Message.Channel.SendMessageAsync(Context.Message.Author.Mention + ", profile reset cancelled");
            }
            
        }


        [Command("select", RunMode = RunMode.Async)]
        public async Task SelectOpenCaseAsync(string inputNumber = null)
        {
            if (inputNumber == null)
            {
                var pager = CsgoCaseSelectionHandler.ShowPossibleCases(Context);

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
                await CsgoCaseSelectionHandler.SelectOpenCase(Context, inputNumber.ToString(), null);
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
            var pager = CsgoInventoryHandler.DisplayUserCsgoInventory(Context);

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
            var pager = CsgoInventoryHandler.GetCsgoMarketInventory(Context, filterString);

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
            await CsgoLeaderboardsManager.DisplayUserStatsAsync(Context);
        }

        /// <summary>
        /// Deletes the specified sent message after the specified amount of time, do not await this for the program to keep running
        /// </summary>
        /// <param name="sentMessage"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private async Task DeleteMessage(IUserMessage sentMessage, TimeSpan timeSpan)
        {
            await Task.Delay(timeSpan);

            //This may throw an exception if the message has already been deleted
            try
            {
                await sentMessage.DeleteAsync();
            }
            catch (Exception)
            {
            }
            
        }
    }
}
