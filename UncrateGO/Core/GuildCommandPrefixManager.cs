using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UncrateGo.Core
{
    public class GuildCommandPrefixManager
    {
        private static readonly string DefaultCommandPrefix = "~";
        private static CommandPrefix GuildPrefixDictionary = JsonConvert.DeserializeObject<CommandPrefix>(FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("GuildCommandPrefix.json")));

        /// <summary>
        /// Returns the command prefix for the current guild message is sent in
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetGuildCommandPrefix(SocketCommandContext context)
        {
            //Find guild id
            var chnl = context.Channel as SocketGuildChannel;
            var guildId = chnl.Guild.Id;

            //Just in case the file is null
            if (GuildPrefixDictionary == null || GuildPrefixDictionary.GuildPrefixes == null)
            {
                GuildPrefixDictionary = new CommandPrefix { GuildPrefixes = new Dictionary<ulong, string>() };
                GuildPrefixDictionary.GuildPrefixes.Add(guildId, DefaultCommandPrefix);

                //Write new dictionary to file
                string newJson = JsonConvert.SerializeObject(GuildPrefixDictionary);
                FileAccessManager.WriteStringToFile(newJson, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
            }

            //Look for guild prefix, in event guild does not have one, use default
            if (!GuildPrefixDictionary.GuildPrefixes.TryGetValue(guildId, out var i))
            {
                GuildPrefixDictionary.GuildPrefixes.Add(guildId, DefaultCommandPrefix);

                //Write new dictionary to file
                string newJson = JsonConvert.SerializeObject(GuildPrefixDictionary);
                FileAccessManager.WriteStringToFile(newJson, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
            }

            return GuildPrefixDictionary.GuildPrefixes[guildId];
        }

        /// <summary>
        /// Changes the bot invoke prefix of the message invoke guild to the specified new prefix
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newPrefix"></param>
        public static void ChangeGuildCommandPrefix(SocketCommandContext context, string newPrefix)
        {
            //Find guild id
            var chnl = context.Channel as SocketGuildChannel;
            var guildId = chnl.Guild.Id;

            //Change prefix
            GuildPrefixDictionary.GuildPrefixes[guildId] = newPrefix;

            //Write new dictionary to file
            string newJson = JsonConvert.SerializeObject(GuildPrefixDictionary);
            FileAccessManager.WriteStringToFile(newJson, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
        }
    }
}
