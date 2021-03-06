﻿using Discord;
using Discord.Commands;
using UncrateGo.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UncrateGo.Modules
{
    public static class UserHelpHandler
    {
        private static HelpMenuCommands _helpMenuCommands;

        public static async Task DisplayHelpMenu(SocketCommandContext context)
        {
            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel);

            //https://leovoel.github.io/embed-visualizer/
            var embedBuilder = new EmbedBuilder()
                .WithDescription($"Prefix: `{botCommandPrefix}`")
                .WithColor(new Color(253, 184, 20))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Sent by " + context.Message.Author.ToString())
                        .WithIconUrl(context.Message.Author.GetAvatarUrl());
                })
                .WithAuthor(author =>
                {
                    author
                        .WithName("UncrateGO help")
                        .WithIconUrl(context.Client.CurrentUser.GetAvatarUrl());
                })
                .AddField("Currency Commands", "`balance` `moneytransfer`")
                .AddField("Case Commands", "`open` `drop` `select` `inventory` `market` `buy` `sell` `sellall` `view` `statistics`")
                .AddField("Settings Commands", "`prefix` `info` `reset`")
                .AddField("\u200b", $"To check command usage, type `{botCommandPrefix}help [command]` ");

            var embed = embedBuilder.Build();

            await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
        }

        public static async Task DisplayCommandHelpMenu(SocketCommandContext context, string inputCommand)
        {
            string botCommandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel);

            //Get command help list from storage
            var commandHelpDefinitionStorage = GetHelpMenuCommands();

            //Create a boolean to warn user that command does not exist if false
            bool commandHelpDefinitionExists = false;

            //Search commandHelpDefinitionStorage for command definition
            foreach (var commandHelpDefinition in commandHelpDefinitionStorage.CommandHelpEntry)
            {
                if (commandHelpDefinition.CommandName == inputCommand)
                {
                    commandHelpDefinitionExists = true;

                    var embedBuilder = new EmbedBuilder()
                    .WithDescription($"**{commandHelpDefinition.CommandDescription}**")
                    .WithColor(new Color(253, 88, 20))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText("Sent by " + context.Message.Author.ToString())
                            .WithIconUrl(context.Message.Author.GetAvatarUrl());
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("UncrateGO help - " + inputCommand)
                            .WithIconUrl(context.Client.CurrentUser.GetAvatarUrl());
                    });

                    if (!string.IsNullOrEmpty(commandHelpDefinition.CommandRequiredPermissions))
                    {
                        embedBuilder.AddField("Permissions required", $"`{commandHelpDefinition.CommandRequiredPermissions}`");
                    }

                    embedBuilder.AddField("Usage", $"`{commandHelpDefinition.CommandUsage.Replace("(prefix)", botCommandPrefix)}`");

                    if (!string.IsNullOrEmpty(commandHelpDefinition.CommandUsageDefinition))
                    {
                        embedBuilder.AddField("Definitions", commandHelpDefinition.CommandUsageDefinition);
                    }

                    var embed = embedBuilder.Build();

                    await context.Message.Channel.SendMessageAsync(" ", embed: embed).ConfigureAwait(false);
                }
            }

            //Send warning if command definition could not be found
            if (commandHelpDefinitionExists == false)
            {
                string similarItemsString = FindSimilarCommands(commandHelpDefinitionStorage.CommandHelpEntry.Select(i => i.CommandName).ToList(), inputCommand);

                //If no similar matches are found, send nothing
                if (string.IsNullOrEmpty(similarItemsString))
                {
                    await context.Channel.SendMessageAsync($"Command **{inputCommand}** could not be found");
                }
                //If similar matches are found, send suggestions
                else
                {
                    await context.Channel.SendMessageAsync($"Command **{inputCommand}** could not be found. Did you mean: \n {similarItemsString}");
                }

            }
        }

        public static HelpMenuCommands GetHelpMenuCommands()
        {
            //Read from file if unassigned
            if (_helpMenuCommands == null)
            {
                var tempHelpMenuCommands = XmlManager.FromXmlFile<HelpMenuCommands>(FileManager.GetFileLocation("CommandHelpDescription.xml"));

                //Create new help menu commands if null
                if (tempHelpMenuCommands == null)
                {
                    tempHelpMenuCommands = new HelpMenuCommands
                    {
                        CommandHelpEntry = new List<HelpMenuCommandEntry>()
                    };
                }

                _helpMenuCommands = tempHelpMenuCommands;
            }

            return _helpMenuCommands;
        }

        /// <summary>
        /// Try to find similar help items based on input
        /// </summary>
        /// <param name="storedCommands">String list of stored strings</param>
        /// <param name="inputCommand">Input string to check</param>
        /// <param name="fuzzyIndex"></param>
        /// <returns></returns>
        public static string FindSimilarCommands(List<string> storedCommands, string inputCommand, int fuzzyIndex=3)
        {
            //Filter out command names to string
            string similarItemsString = "";

            foreach (var item in storedCommands)
            {
                //If fuzzy search difference is less than 6 or if storedCommand contains inputCommand
                if (FuzzySearch.Compute(item.ToLower(), inputCommand.ToLower()) < fuzzyIndex ||
                    item.ToLower().Contains(inputCommand.ToLower()))
                {
                    //Concat items in list together
                    similarItemsString = string.Concat(similarItemsString, "\n", item);
                }

            }

            return similarItemsString;
        }
    }

    public class HelpMenuCommands
    {
        public List<HelpMenuCommandEntry> CommandHelpEntry { get; set; }
    }
    public class HelpMenuCommandEntry
    {
        public string CommandName { get; set; }
        public string CommandDescription { get; set; }
        public string CommandRequiredPermissions { get; set; }
        public string CommandUsage { get; set; }
        public string CommandUsageDefinition { get; set; }
    }
}


