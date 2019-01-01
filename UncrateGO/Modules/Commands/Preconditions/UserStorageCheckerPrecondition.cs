using Discord.Commands;
using UncrateGo.Models;
using UncrateGo.Modules.UserActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Commands.Preconditions
{
    class UserStorageCheckerPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Create xml user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(context.Message.Author.Id, out var i))
            {
                //Create user profile
                UserDataManager.CreateNewUserXmlEntry(context as SocketCommandContext);
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
