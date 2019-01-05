using Discord.WebSocket;
using DiscordBotsList.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public class DiscordBotsListUpdater
    {
        public static async void UpdateDiscordBotsListInfo(DiscordSocketClient _client)
        {
            while (true)
            {
                try
                {
                    if (_client.CurrentUser != null)
                    {
                        AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(_client.CurrentUser.Id, FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("DiscordBotListToken.txt")));

                        var me = await DblApi.GetMeAsync();

                        // Update stats           guildCount
                        await me.UpdateStatsAsync(_client.Guilds.Count());
                    }
                    
                }
                catch (Exception)
                {
                    EventLogger.LogMessage("Unable to update stats, possible invalid token?", ConsoleColor.Red);
                }

                Thread.Sleep(300000);
            }
        }
    }
}
