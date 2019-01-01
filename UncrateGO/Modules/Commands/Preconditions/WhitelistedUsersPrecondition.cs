using Discord.Commands;
using Discord.WebSocket;
using DuckBot_ClassLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.Preconditions
{
    //Whitelist precondition
    public class WhitelistedUsersPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            // Get the client via Depedency Injection
            var client = _services.GetRequiredService<DiscordSocketClient>();

            // Get the ID of the bot's owner
            var appInfo = await client.GetApplicationInfoAsync().ConfigureAwait(false);
            var ownerId = appInfo.Owner.Id;

            //
            // If this command was NOT executed by predefined users, return a failure
            List<ulong> whitelistedUsers = new List<ulong>();

            whitelistedUsers.Add(ownerId);

            CoreMethod.ReadFromFileToList("UserWhitelist.txt").ForEach(u => whitelistedUsers.Add(ulong.Parse(u)));

            //Test if user is whitelisted
            bool userIsWhiteListed = false;
            foreach (var user in whitelistedUsers)
            {
                if (context.Message.Author.Id == user)
                {
                    userIsWhiteListed = true;
                }
            }

            if (userIsWhiteListed == false)
            {
                await context.Channel.SendMessageAsync(context.Message.Author.Mention + " You must be whitelisted by the great duck commander to use this command");
                return PreconditionResult.FromError("You must be whitelisted by the great duck commander to use this command.");
            }
            else
            {
                return PreconditionResult.FromSuccess();
            }

        }
    }
}
