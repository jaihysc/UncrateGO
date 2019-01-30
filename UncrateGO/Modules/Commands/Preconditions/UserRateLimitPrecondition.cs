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
        private static readonly uint _invokeLimit;
        private static readonly TimeSpan _invokeLimitPeriod;
        //Ulong is userId, commandTimeout is class storing timeout info
        private static readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        /// <param name="flags">Flags to set behavior of the ratelimit.</param>
        static RatelimitAttribute()
        {
            //Ratelimit Config
            uint times = 2;
            double period = 9;
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
        /// Handels actual user ratelimit
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="_services"></param>
        /// <returns></returns>
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            bool UserRatelimited = await UserRateLimit(context.Message.Author.Id, context as SocketCommandContext);

            //Timeout messages
            if (!UserRatelimited)
            {
                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return await Task.FromResult(PreconditionResult.FromError("User is in cooldown"));
            }
        }

        public static async Task<bool> UserRateLimit(ulong userID, SocketCommandContext context = null)
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

            //Set the invoke limit
            timeout.InvokeLimit = _invokeLimit;
            timeout.InvokeLimitPeriod = _invokeLimitPeriod;

            //Timeout messages
            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[key] = timeout;

                //Set limit reached to true after reaching invoke limit
                if (timeout.TimesInvoked >= _invokeLimit) _invokeTracker[key].InvokeLimitReached = true;

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
            var d = await context.Channel.SendMessageAsync(context.Message.Author.Mention + " Chill, calm down. Take a drink, have a walk, come back **(You are in cooldown)**");
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
        public TimeSpan InvokeLimitPeriod { get; set; }
        public long InvokeLimit { get; set; }
        public bool InvokeLimitReached { get; set; }
        public bool ReceivedError { get; set; }

        public CommandTimeout(DateTime timeStarted)
        {
            FirstInvoke = timeStarted;
            ReceivedError = false;
        }
    }
}
