﻿using System;
using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public class UserInteraction
    {
        /// <summary>
        /// Returns a bolded user name of specified user
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string BoldUserName(SocketCommandContext context)
        {
            return $"**{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}**";
        }

        public static string UserName(SocketCommandContext context)
        {
            return $"{context.Message.Author.ToString().Substring(0, context.Message.Author.ToString().Length - 5)}";
        }

        public static async Task SendFirstTimeHelpMenuAsync(SocketGuild socketGuild)
        {
            try
            {
                //Get first text channel
                List<SocketTextChannel> chnlList = socketGuild.TextChannels.ToList();

                SocketTextChannel messageChannel = null;
                foreach (var channel in chnlList) //Iterate through the channels to find one where the bot has permissions to message
                {
                    foreach (var permissionOverwrite in channel.PermissionOverwrites)
                    {
                        if (permissionOverwrite.Permissions.SendMessages == PermValue.Allow)
                        {
                            messageChannel = channel;
                        }
                    }
                    //Additionally also if the channel does not have permission overrides try to send in it
                    if (channel.PermissionOverwrites.Count <= 0) messageChannel = channel;
                    if (messageChannel != null) break;
                }

                if (messageChannel != null) await messageChannel.SendMessageAsync("**Thank you for adding me! :white_check_mark:**\n`-`My default prefix here is `~`\n`-`View list of commands with `~help`\n`-`You can change my command prefix with `~prefix`\n\n`-`For help with issues or problems: discordapp.com/invite/VNDS9sW");

            }
            catch
            {
                EventLogger.LogMessage("Unable to send initial help messages", ConsoleColor.Red);
            }

        }
    }

}
