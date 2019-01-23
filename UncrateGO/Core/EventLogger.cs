using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace UncrateGo.Core
{
    class EventLogger : ModuleBase<SocketCommandContext>
    {
        
        public static Task LogAsync(LogMessage message)
        {
            //Logs server messages to console
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }
        
        public static void LogMessage(string message, ConsoleColor intensity = ConsoleColor.White)
        {
            Console.ForegroundColor = intensity;
            Console.WriteLine($"{DateTime.Now,-19} [    Info] Logging: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static Task LogUserMessageAsync(SocketMessage msg)
        {
            //Log user message to file
            var chnl = msg.Channel as SocketGuildChannel;

            if (chnl != null)
            {
                Console.Write($"{DateTime.Now,-19} [    Log] {chnl.Guild.Name} ||  {msg.Channel} - {msg.Author}: ");
            }
            else
            {
                Console.Write($"{DateTime.Now,-19} [    Log] Direct Message >| {msg.Channel} - {msg.Author}: ");
            }

            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg.ToString());

            Console.BackgroundColor = ConsoleColor.Black;
            return Task.CompletedTask;
        }

    }
}
