using DiscordBotsList.Api;
using System;
using UncrateGo.Core;

namespace UncrateGo.Modules
{
    public static class DiscordBotsListUpdater
    {
        public static async void UpdateDiscordBotsListInfo(ulong userId, int guildsCount)
        {
            try
            {
                EventLogger.LogMessage("Updating discord bots list bot info...", EventLogger.LogLevel.Info);

                AuthDiscordBotListApi dblApi = new AuthDiscordBotListApi(userId, FileManager.ReadFromFile(FileManager.GetFileLocation("DiscordBotListToken.txt")));

                var me = await dblApi.GetMeAsync();

                // Update stats guildCount
                await me.UpdateStatsAsync(guildsCount);

                EventLogger.LogMessage("Updating discord bots list bot info... Done", EventLogger.LogLevel.Info);
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to update stats, possible invalid token?", EventLogger.LogLevel.Error);
            }
        }
    }
}
