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
        
        public static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now,-19} [    Info] Logging: {message}");
        }

        public static Task LogUserMessageAsync(SocketMessage msg)
        {
            //Log user message to file
            var chnl = msg.Channel as SocketGuildChannel;
            var cc = Console.BackgroundColor;

            try
            {
                Console.Write($"{DateTime.Now,-19} [    Log] {chnl.Guild.Name} ||  {msg.Channel} - {msg.Author}: ");
            }
            catch (Exception)
            {
                Console.Write($"{DateTime.Now,-19} [    Log] Direct Message >| {msg.Channel} - {msg.Author}: ");
            }

            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(msg.ToString());

            Console.BackgroundColor = cc;
            return Task.CompletedTask;
        }

    }
}
