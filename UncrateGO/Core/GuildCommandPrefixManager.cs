using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace UncrateGo.Core
{
    public static class GuildCommandPrefixManager
    {
        private static readonly string DefaultCommandPrefix = "~";
        private static CommandPrefix _guildPrefixDictionary = new CommandPrefix();

        /// <summary>
        /// Returns the command prefix for the current guild message is sent in
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetGuildCommandPrefix(SocketCommandContext context)
        {
            //Find guild id
            if (context.Channel is SocketGuildChannel chnl)
            {
                var guildId = chnl.Guild.Id;

                //Just in case the file is null, it will use default
                if (_guildPrefixDictionary == null || _guildPrefixDictionary.GuildPrefixes == null)
                {
                    _guildPrefixDictionary = JsonConvert.DeserializeObject<CommandPrefix>(FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("GuildCommandPrefix.json")));

                    if (_guildPrefixDictionary == null || _guildPrefixDictionary.GuildPrefixes == null)
                    {
                        _guildPrefixDictionary = new CommandPrefix { GuildPrefixes = new Dictionary<ulong, string>() };
                        _guildPrefixDictionary.GuildPrefixes.Add(guildId, DefaultCommandPrefix);

                        //Create dictionary to file
                        string newJson = JsonConvert.SerializeObject(_guildPrefixDictionary);
                        FileAccessManager.WriteStringToFile(newJson, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
                    }              
                }

                //Look for guild prefix, in event guild does not have one
                if (!_guildPrefixDictionary.GuildPrefixes.TryGetValue(guildId, out _))
                {
                    EventLogger.LogMessage($"Failed to retrieve prefix for guild {guildId}, defaulting to {DefaultCommandPrefix}", EventLogger.LogLevel.Warning);

                    _guildPrefixDictionary.GuildPrefixes.Add(guildId, DefaultCommandPrefix);
                }

                return _guildPrefixDictionary.GuildPrefixes[guildId];
            }

            return "~"; //Return the default prefix if it cannot get the guild prefix
        }

        public static void PopulateGuildCommandPrefix()
        {
            _guildPrefixDictionary = JsonConvert.DeserializeObject<CommandPrefix>(FileAccessManager.ReadFromFile(FileAccessManager.GetFileLocation("GuildCommandPrefix.json")));

            if (_guildPrefixDictionary == null || _guildPrefixDictionary.GuildPrefixes == null)
            {
                EventLogger.LogMessage("GuildCommandPrefix.json not found, creating one", EventLogger.LogLevel.Info);

                _guildPrefixDictionary = new CommandPrefix { GuildPrefixes = new Dictionary<ulong, string>() };

                //Create dictionary to file
                string newJson = JsonConvert.SerializeObject(_guildPrefixDictionary);
                FileAccessManager.WriteStringToFile(newJson, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
            }
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
            if (chnl != null)
            {
                var guildId = chnl.Guild.Id;

                //Change prefix
                _guildPrefixDictionary.GuildPrefixes[guildId] = newPrefix;
            }
        }

        /// <summary>
        /// Removes the prefix for the specified guild
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static Task DeleteGuildCommandPrefix(SocketGuild arg)
        {
            //Remove guild command on leave
            _guildPrefixDictionary.GuildPrefixes.Remove(arg.Id);
            return Task.CompletedTask;
        }

        public static void FlushGuildCommandDictionary()
        {
            try
            {
                //Write new dictionary to file
                var tempGuildPrefixDictionary = _guildPrefixDictionary;
                string json = JsonConvert.SerializeObject(tempGuildPrefixDictionary);
                FileAccessManager.WriteStringToFile(json, true, FileAccessManager.GetFileLocation("GuildCommandPrefix.json"));
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to flush guild command dictionary", EventLogger.LogLevel.Error);
            }

        }
    }

    internal sealed class CommandPrefix
    {
        public Dictionary<ulong, string> GuildPrefixes { get; set; }
    }
}
