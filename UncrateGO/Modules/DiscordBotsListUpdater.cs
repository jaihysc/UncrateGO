using Discord.WebSocket;
using DiscordBotsList.Api;
using System;
using System.Linq;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public static class DiscordBotsListUpdater
    {
        public static async void UpdateDiscordBotsListInfo(object state)
        {
            try
            {
                var client = (DiscordSocketClient)state;

                if (client.CurrentUser != null)
                {
                    AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(client.CurrentUser.Id, FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("DiscordBotListToken.txt")));

                    var me = await DblApi.GetMeAsync();

                    // Update stats guildCount
                    await me.UpdateStatsAsync(client.Guilds.Count());
                }
                
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to update stats, possible invalid token?", EventLogger.LogLevel.Error);
            }
        }
    }
}
