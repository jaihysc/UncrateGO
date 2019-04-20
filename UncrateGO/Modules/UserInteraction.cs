using System.Collections.Generic;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public static class UserInteraction
    {
        /// <summary>
        /// Returns a bold user name of specified user with numbers at the end
        /// </summary>
        /// <returns>ABC#1234 => ABC</returns>
        public static string BoldUserName(string userString)
        {
            if (userString == null) return "";

            return $"**{userString.Substring(0, userString.Length - 5)}**";
        }

        public static string UserName(string userString)
        {
            if (userString == null) return "";

            return $"{userString.Substring(0, userString.Length - 5)}";
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
                EventLogger.LogMessage("Unable to send initial greeting message", EventLogger.LogLevel.Error);
            }

        }
    }

}
