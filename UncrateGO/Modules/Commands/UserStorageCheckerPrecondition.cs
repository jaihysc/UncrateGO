using System;
using System.Threading.Tasks;
using Discord.Commands;
using UncrateGo.Core;

namespace UncrateGo.Modules.Commands
{
    class UserStorageCheckerPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var userStorage = UserDataManager.GetUserStorage();

            //Create xml user credit entry if user does not exist
            if (!userStorage.UserInfo.TryGetValue(context.Message.Author.Id, out _))
            {
                //Create user profile
                UserDataManager.CreateNewUserEntry(context as SocketCommandContext);
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
