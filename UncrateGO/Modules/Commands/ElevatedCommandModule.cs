using Discord;
using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Core;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Finance;
using UncrateGo.Modules.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit(1, 4, Measure.Seconds)]
    [UserStorageCheckerPrecondition]
    public class ElevatedCommandModule : ModuleBase<SocketCommandContext>
    {
        [Group("settings")]
        public class Elevated : ModuleBase<SocketCommandContext>
        {
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
        }
    }
}
