using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Core;
using UncrateGo.Modules.Csgo;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UncrateGo.Modules;
using System.Runtime.InteropServices;
using UncrateGo.Modules.Commands;

namespace UncrateGo
{
    public class MainProgram
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public static void Main(string[] args)
        {
            try
            {
                Stopwatch.Start();
                EventLogger.LogMessage("Hello World! - Beginning startup");

                DisableConsoleQuickEdit.Go(); //Disable console features

                //Runs setup if path file is not present
                SetupManager.CheckIfPathsFileExists();

                CsgoDataUpdater.GenerateSouvenirCollections();

                //Timers
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(57600000);
                        CsgoDataUpdater.UpdateRootWeaponSkin();
                    }

                });
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(60000);
                        CsgoLeaderboardManager.GetStatisticsLeader();

                    }

                });
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(300000);
                        FlushAllData();
                    }
                });

                //Setup
                CsgoDataHandler.GetCsgoCosmeticData();
                UserDataManager.GetUserStorage();
                CsgoDataHandler.GetUserSkinStorage();
                GuildCommandPrefixManager.PopulateGuildCommandPrefix();
                CsgoUnboxingHandler.GetUserSelectedCase();

                //Exception handling
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += ExceptionHandler;

                //Program exit handling
                currentDomain.ProcessExit += CurrentDomain_ProcessExit;

                //Main
                new MainProgram().MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oh no, something went wrong!!! \n" + ex.Source + ex.StackTrace);
                Console.WriteLine("Press ENTER to exit");
                Console.ReadLine();
            }

        }

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        //Main
        private async Task MainAsync()
        {
            var config = new DiscordSocketConfig { MessageCacheSize = 500 };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()

                //BuildsServiceProvider
                .BuildServiceProvider();

            
            //Bot init
            string tokenPath = FileManager.GetFileLocation("BotToken.txt");
            if (!File.Exists(tokenPath))
            {
                EventLogger.LogMessage("No BotToken.txt found, bot will not start", EventLogger.LogLevel.Critical);
                throw new Exception("No bot token");
            }

            try
            {
                //Get token
                string token = File.ReadAllLines(tokenPath).FirstOrDefault();

                //Connect to discord
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

            }
            catch (Exception)
            {
                Console.WriteLine("Unable to initialize! - Could it be because of an invalid token?");
            }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            //Set help text
            //await _client.SetGameAsync("Mention me in server for command prefix");

            //EVENT HANDLERS
            //Log user / console messages
            _client.Log += EventLogger.LogAsync;
            _client.MessageReceived += EventLogger.LogUserMessageAsync;

            //Joining a guild first time, display help text
            _client.JoinedGuild += UserInteraction.SendFirstTimeHelpMenuAsync;
            _client.LeftGuild += GuildCommandPrefixManager.DeleteGuildCommandPrefix;

            //Handles command on message received event
            _client.MessageReceived += HandleCommandAsync;

            //Discord bots list updater
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(900000);
                    DiscordBotsListUpdater.UpdateDiscordBotsListInfo(_client.CurrentUser.Id, _client.Guilds.Count);
                }
            });

            Stopwatch.Stop();
            EventLogger.LogMessage($"Ready! - Took {Stopwatch.ElapsedMilliseconds} milliseconds");

            //All commands before this
            await Task.Delay(-1);
        }

        //Command Handler
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message, if sender is bot
            if (!(messageParam is SocketUserMessage message)) return;

            if (message.Author.IsBot) return;

            //integer to determine when commands start
            int argPos = 0;

            //If message is in a DM, return
            var chnl = messageParam.Channel as SocketGuildChannel;
            if (chnl == null) return;
            
            var context = new SocketCommandContext(_client, message);

            //Ignore commands that are not using the prefix or mentioning the bot
            string commandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context.Channel);

            if (!message.HasStringPrefix(commandPrefix, ref argPos) && !(message.Content == ("<@" + context.Client.CurrentUser.Id.ToString() + ">") || message.Content == ("<@!" + context.Client.CurrentUser.Id.ToString() + ">")) ||
                message.Author.IsBot)
                return;

            //Only process command if user is not in cooldown
            if (Ratelimit.UserRateLimited(context.Message.Author.Id, context)) return;

            //Show prefix help if user mentions bot
            if (message.Content == ("<@" + context.Client.CurrentUser.Id.ToString() + ">") || message.Content == ("<@!" + context.Client.CurrentUser.Id.ToString() + ">"))
            {
                await context.Channel.SendMessageAsync($"Current guild prefix: `{commandPrefix}` | Get help with `{commandPrefix}help`");
                return;
            }

            var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);

            //COMMAND LOGGING
            // Inform the user if the command fails
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand)
                {
                    //Find similar commands
                    var commandHelpDefinitionStorage = UserHelpHandler.GetHelpMenuCommands();

                    string similarItemsString = "";
                    if (message.ToString().Length > commandPrefix.Length)
                    {
                        similarItemsString = UserHelpHandler.FindSimilarCommands(
                            commandHelpDefinitionStorage.CommandHelpEntry.Select(i => i.CommandName).ToList(),
                            message.ToString().Substring(commandPrefix.Length + 1));
                    }

                    //If no similar matches are found, send nothing
                    if (string.IsNullOrEmpty(similarItemsString))
                    {
                        await context.Channel.SendMessageAsync($"Invalid command, use `{commandPrefix}help` for a list of commands");
                    }
                    //If similar matches are found, send suggestions
                    else
                    {
                        await context.Channel.SendMessageAsync($"Invalid command, use `{commandPrefix}help` for a list of commands. Did you mean: \n {similarItemsString}");
                    }
                    
                }
                else if (result.Error == CommandError.BadArgCount)
                {
                    var commandHelpDefStorage = UserHelpHandler.GetHelpMenuCommands();

                    //Search 10 spaces up for the command in commandHelpDescription
                    string userCommand = "[command]";

                    string str = message.Content.ToLower().Substring(commandPrefix.Length);

                    string[] tokens = str.Split(' ');

                    //Extract the command from the user (minus prefix and any invalid arguments)
                    for (int i = 1; i < 10; i++)
                    {
                        bool resultFound = false;
                        
                        userCommand = "";

                        //Combine the strings
                        for (int ii = 0; ii < i; ii++)
                        {
                            //Make sure next string exists before adding it
                            if (i <= tokens.Count())
                            {
                                //Add a space between the strings after the first
                                if (ii > 0)
                                {
                                    userCommand += " ";
                                }

                                userCommand += tokens[ii];
                            }                
                            
                        }

                        //Check if the command exists
                        foreach (var item in commandHelpDefStorage.CommandHelpEntry)
                        {
                            //If so break out
                            if (item.CommandName == userCommand)
                            {
                                resultFound = true;
                                break;
                            }
                        }

                        //Break after finding result
                        if (resultFound) break;
                    }

                    //Default value in case nothing can be found
                    //if (string.IsNullOrEmpty(userCommand)) userCommand = "[command]";
                    //await context.Channel.SendMessageAsync($"Invalid command usage, use `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}help {userCommand}` for correct command usage");

                    //Send message
                    await context.Channel.SendMessageAsync(context.Message.Author.Mention + $", you used `{userCommand}` incorrectly, below is the correct usage");
                    if (!string.IsNullOrEmpty(userCommand)) await UserHelpHandler.DisplayCommandHelpMenu(context, userCommand);
                }
                else
                {
                    EventLogger.LogMessage($"Error - {result.ErrorReason}", EventLogger.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Flushes all data stored to file
        /// </summary>
        public static void FlushAllData()
        {
            EventLogger.LogMessage("Flushing data to file...", EventLogger.LogLevel.Info);

            UserDataManager.FlushUserStorage();
            CsgoDataHandler.FlushUserSkinStorage();
            GuildCommandPrefixManager.FlushGuildCommandDictionary();
            CsgoUnboxingHandler.FlushUserSelectedCase();

            EventLogger.LogMessage("Flushing data to file - DONE!", EventLogger.LogLevel.Info);
        }

        //Program exit handling
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            FlushAllData(); //Flush any remaining data
        }

        /// <summary>
        /// Exception handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Caught: " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
            Console.WriteLine(e.StackTrace);

            FlushAllData();

            //Write a crashlog to file
            FileManager.WriteStringToFile(e.Message + e.StackTrace, false, FileManager.GetFileLocation("crashlog.txt"));
        }

        //https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode
        static class DisableConsoleQuickEdit
        {
            const uint EnableQuickEdit = 0x0040;

            // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
            const int StdInputHandle = -10;

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll")]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll")]
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

            internal static void Go()
            {

                IntPtr consoleHandle = GetStdHandle(StdInputHandle);

                // get current console mode
                if (!GetConsoleMode(consoleHandle, out var consoleMode))
                {
                    // ERROR: Unable to get console mode.
                    return;
                }

                // Clear the quick edit bit in the mode flags
                consoleMode &= ~EnableQuickEdit;

                // set the new mode
                if (!SetConsoleMode(consoleHandle, consoleMode))
                {
                    // ERROR: Unable to set console mode
                }
            }
        }
    }
}
