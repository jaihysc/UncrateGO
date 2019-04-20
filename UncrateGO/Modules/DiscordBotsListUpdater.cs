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
                    AuthDiscordBotListApi dblApi = new AuthDiscordBotListApi(client.CurrentUser.Id, FileManager.ReadFromFile(FileManager.GetFileLocation("DiscordBotListToken.txt")));

                    var me = await dblApi.GetMeAsync();

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
