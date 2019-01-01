using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Interaction
{
    public class GuildJoinInteraction
    {
        public static async Task SendFirstTimeHelpMenuAsync(SocketGuild socketGuild)
        {
            //Get first text channel
            var chnl = socketGuild.TextChannels.FirstOrDefault();

            //Send initial join message
            await chnl.SendMessageAsync("**Thank you for adding me! :white_check_mark:**\n`-`My default prefix here is `~`\n`-`View list of commands with .`d help`\n`-`You can change my command prefix with `.d elevated prefix`");
        }
    }
}
