using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;

namespace UncrateGo.Modules.Commands
{
    /// <summary> Sets how often a user is allowed to use this command
    /// or any command in this module. </summary>
    /// <remarks>This is backed by an in-memory collection
    /// and will not persist with restarts.</remarks>
    public static class Ratelimit
    {
        private static readonly uint InvokeLimit;
        private static readonly TimeSpan InvokeLimitPeriod;
        //Ulong is userId, commandTimeout is class storing timeout info
        private static readonly Dictionary<ulong, CommandTimeout> InvokeTracker = new Dictionary<ulong, CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        static Ratelimit()
        {
            //Ratelimit Config
            uint times = 3;
            double period = 10;
            Measure measure = Measure.Seconds;

            //-Config
            InvokeLimit = times;

            switch (measure)
            {
                case Measure.Days:
                    InvokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    InvokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    InvokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                case Measure.Seconds:
                    InvokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;
            }
        }

        /// <summary>
        /// Gets if specified user is in cooldown
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="context">To send warning, pass in context</param>
        /// <returns>True if in cooldown</returns>
        public static bool UserRateLimited(ulong userId, SocketCommandContext context = null)
        {
            var now = DateTime.UtcNow;
            var key = userId;

            CommandTimeout timeout;
            if (InvokeTracker.TryGetValue(key, out var t))
            {
                //Keep the commandtimeout if it has not expired
                if (now - t.FirstInvoke < InvokeLimitPeriod)
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
            if (timeout.TimesInvoked <= InvokeLimit)
            {
                InvokeTracker[key] = timeout;

                return false;
            }
            else
            {
                //Send an error if this is the first time user tried to use the command again while in cooldown                
                if (timeout.ReceivedError == false)
                {
                    //Only send this message once
                    InvokeTracker[key].ReceivedError = true;

                    //Stores the warning message to delete later on
                    if (context != null)
                    {
                        Task.Run(async () =>
                        {
                            RestUserMessage msg = await context.Channel.SendMessageAsync(context.Message.Author.Mention + " Please slow down. **(You are in cooldown)**");
                            await Task.Delay(5000);
                            await msg.DeleteAsync();
                        });
                    }

                }

                return true;
            }
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
