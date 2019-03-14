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
    public static class RatelimitPrecondtion
    {
        private static readonly uint _invokeLimit;
        private static readonly TimeSpan _invokeLimitPeriod;
        //Ulong is userId, commandTimeout is class storing timeout info
        private static readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        /// <param name="flags">Flags to set behavior of the ratelimit.</param>
        static RatelimitPrecondtion()
        {
            //Ratelimit Config
            uint times = 2;
            double period = 8;
            Measure measure = Measure.Seconds;

            //-Config
            _invokeLimit = times;

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

        /// <summary>
        /// Gets if specified user is in cooldown
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="context"></param>
        /// <returns>True if in cooldown</returns>
        public static async Task<bool> UserRateLimited(ulong userID, SocketCommandContext context = null)
        {
            var now = DateTime.UtcNow;
            var key = userID;

            CommandTimeout timeout;
            if (_invokeTracker.TryGetValue(key, out var t))
            {
                //Kep the commandtimeout if it has not expired
                if (now - t.FirstInvoke < _invokeLimitPeriod)
                {
                    timeout = t;
                }
                //Reset counter if user has passed timeout
                else
                {
                    timeout = new CommandTimeout(now);
                }
            }
            //If commandtimeout for user does not exist, create one
            else
            {
                timeout = new CommandTimeout(now);
            }

            //Increment invoke amount
            timeout.TimesInvoked++;

            //Timeout messages
            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[key] = timeout;

                return false;
            }
            else
            {
                //Send an error if this is the first time user tried to use the command again while in cooldown                
                if (timeout.ReceivedError == false)
                {
                    //Only send this message once
                    _invokeTracker[key].ReceivedError = true;

                    //Stores the warning message to delete later on
                    if (context != null) SendWarningMessageAsync(context);

                }

                return true;
            }
        }

        private static async Task SendWarningMessageAsync(ICommandContext context)
        {
            var d = await context.Channel.SendMessageAsync(context.Message.Author.Mention + " Please slow down. **(You are in cooldown)**");
            await Task.Delay(5000);
            await d.DeleteAsync();
        }
    }

    /// <summary> Sets the scale of the period parameter. </summary>
    public enum Measure
    {
        Days,
        Hours,
        Minutes,
        Seconds
    }

    internal sealed class CommandTimeout
    {
        public uint TimesInvoked { get; set; }
        public DateTime FirstInvoke { get; }
        public bool ReceivedError { get; set; }

        public CommandTimeout(DateTime timeStarted)
        {
            FirstInvoke = timeStarted;
            ReceivedError = false;
        }
    }
}
