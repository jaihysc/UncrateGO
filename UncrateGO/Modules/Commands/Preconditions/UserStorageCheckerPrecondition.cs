using Discord.Commands;
using System;
using System.Threading.Tasks;
using UncrateGo.Core;

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
                UserDataManager.CreateNewUserEntry(context as SocketCommandContext);
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
