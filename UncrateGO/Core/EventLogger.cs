using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace UncrateGo.Core
{
    internal static class EventLogger
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
        
        public static void LogMessage(string message, LogLevel logLevel = LogLevel.Debug)
        {
            string severity = "";

            switch (logLevel)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    severity = "Debug";
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    severity = "Info";
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    severity = "WARNING";
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    severity = "ERROR";
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    severity = "CRITICAL";
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [    {severity}] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public enum LogLevel { Debug, Info, Warning, Error, Critical }

        public static Task LogUserMessageAsync(SocketMessage msg)
        {
            //Log user message to file
            if (msg.Channel is SocketGuildChannel chnl)
            {
                Console.Write($"{DateTime.Now,-19} [    Log] {chnl.Guild.Name} | {msg.Channel} - {msg.Author}: ");
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
