using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Core;
using UncrateGo.Models;
using Microsoft.Extensions.DependencyInjection;

namespace UncrateGo.Modules.Commands.Preconditions
{
    //Credit to https://github.com/Joe4evr/Discord.Addons/tree/master/src/Discord.Addons.Preconditions for the precondition

    /// <summary> Sets how often a user is allowed to use this command
    /// or any command in this module. </summary>
    /// <remarks>This is backed by an in-memory collection
    /// and will not persist with restarts.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly bool _noLimitInDMs;
        private readonly bool _noLimitForAdmins;
        private readonly bool _applyPerGuild;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<(ulong, ulong?), CommandTimeout> _invokeTracker = new Dictionary<(ulong, ulong?), CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        /// <param name="flags">Flags to set behavior of the ratelimit.</param>
        public RatelimitAttribute(
            uint times,
            double period,
            Measure measure,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;

            //TODO: C# 8 candidate switch expression
            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                case Measure.Seconds:
                    _invokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;
            }
        }

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="flags">Flags to set bahavior of the ratelimit.</param>
        public RatelimitAttribute(
            uint times,
            TimeSpan period,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;

            _invokeLimitPeriod = period;
        }

        /// <summary>
        /// Handels actual user ratelimit
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="_services"></param>
        /// <returns></returns>
        public async override Task<PreconditionResult> CheckPermissions(
            ICommandContext context,
            CommandInfo command,
            IServiceProvider _services)
        {
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var key = _applyPerGuild ? (context.User.Id, context.Guild?.Id) : (context.User.Id, null);

            var timeout = (_invokeTracker.TryGetValue(key, out var t)
                && ((now - t.FirstInvoke) < _invokeLimitPeriod))
                    ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            var client = _services.GetRequiredService<DiscordSocketClient>();

            // Get the ID of the bot's owner
            var appInfo = await client.GetApplicationInfoAsync().ConfigureAwait(false);
            var ownerId = appInfo.Owner.Id;

            //Allow bypass for whitelisted users
            // Get whitelisted users
            List<ulong> whitelistedUsers = new List<ulong>
            {
                ownerId
            };

            FileAccessManager.ReadFromFileToList(FileAccessManager.GetFileLocation("UserWhitelist.txt")).ForEach(u => whitelistedUsers.Add(ulong.Parse(u)));

            //Test if user is whitelisted
            bool userIsWhiteListed = false;
            foreach (var user in whitelistedUsers)
            {
                if (context.Message.Author.Id == user)
                {
                    userIsWhiteListed = true;
                }
            }

            //Timeout messages
            if (timeout.TimesInvoked <= _invokeLimit || userIsWhiteListed == true)
            {
                _invokeTracker[key] = timeout;
                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                //Send an error if this is the first time user tried to use the command again while in cooldown                
                if (timeout.ReceivedError == false)
                {
                    //Only send this message once
                    _invokeTracker[key].ReceivedError = true;

                    await context.Channel.SendMessageAsync(context.Message.Author.Mention + " Chill, calm down. Take a drink, have a walk, come back **(You are in cooldown)**");
                }
                
              
                return await Task.FromResult(PreconditionResult.FromError("User is in cooldown"));
            }


        }
    }

    /// <summary> Sets the scale of the period parameter. </summary>
    public enum Measure
    {
        /// <summary> Period is measured in days. </summary>
        Days,

        /// <summary> Period is measured in hours. </summary>
        Hours,

        /// <summary> Period is measured in minutes. </summary>
        Minutes,

        /// <summary> Period is measured in minutes. </summary>
        Seconds
    }

    /// <summary> Used to set behavior of the ratelimit </summary>
    [Flags]
    public enum RatelimitFlags
    {
        /// <summary> Set none of the flags. </summary>
        None = 0,

        /// <summary> Set whether or not there is no limit to the command in DMs. </summary>
        NoLimitInDMs = 1 << 0,

        /// <summary> Set whether or not there is no limit to the command for guild admins. </summary>
        NoLimitForAdmins = 1 << 1,

        /// <summary> Set whether or not to apply a limit per guild. </summary>
        ApplyPerGuild = 1 << 2
    }
}
