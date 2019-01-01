using Discord.Commands;
using Discord.WebSocket;
using DuckBot_ClassLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.Preconditions
{
    //Blacklist precondition
    public class BlacklistedUsersPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            try
            {
                //
                // If this command was executed by predefined users, return a failure
                List<ulong> blacklistedUsers = new List<ulong>();

                CoreMethod.ReadFromFileToList(CoreMethod.GetFileLocation("UserBlacklist.txt")).ForEach(u => blacklistedUsers.Add(ulong.Parse(u)));

                //Test if user is blacklisted
                bool userIsBlackListed = false;
                foreach (var user in blacklistedUsers)
                {
                    if (context.Message.Author.Id == user)
                    {
                        userIsBlackListed = true;
                    }
                }

                if (userIsBlackListed == false)
                {
                    return PreconditionResult.FromSuccess();
                }
                else
                {
                    await context.Channel.SendMessageAsync(context.Message.Author.Mention + " You have been blocked from using this command");
                    return PreconditionResult.FromError("You have been blocked from using this command.");
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
