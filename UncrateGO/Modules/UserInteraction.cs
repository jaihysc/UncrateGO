using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //Get first text channel
            var chnl = socketGuild.TextChannels.FirstOrDefault();

            //Send initial join message
            await chnl.SendMessageAsync("**Thank you for adding me! :white_check_mark:**\n`-`My default prefix here is `~`\n`-`View list of commands with `~help`\n`-`You can change my command prefix with `~prefix`");
        }
    }

}
