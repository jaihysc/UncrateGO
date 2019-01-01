using Discord.Addons.Interactive;
using Discord.Commands;
using UncrateGo.Modules.Commands.Preconditions;
using UncrateGo.Modules.Csgo;
using UncrateGo.Modules.Finance;
using UncrateGo.Modules.Finance.CurrencyManager;
using UncrateGo.Modules.UserActions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UncrateGo.Modules.Commands
{
    [Ratelimit(1, 10, Measure.Seconds)]
    [UserStorageCheckerPrecondition]
    public class HelpCommandModule : InteractiveBase
    {
        [Command("help")]
        public async Task HelpAsync([Remainder]string inputCommand = null)
        {
            if (!string.IsNullOrEmpty(inputCommand))
            {
                await UserHelpHandler.DisplayCommandHelpMenu(Context, inputCommand);
            }
            else
            {
                await UserHelpHandler.DisplayHelpMenu(Context);
            }
        }
    }
}