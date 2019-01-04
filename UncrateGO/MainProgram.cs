using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using UncrateGo.Core;
using UncrateGo.Models;
using UncrateGo.Modules.Csgo;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UncrateGo.Modules;

namespace UncrateGo
{
    public class MainProgram
    {
        private static Stopwatch stopwatch = new Stopwatch();

        public static void Main(string[] args)
        {
            //Injection
            stopwatch.Start();
            EventLogger.LogMessage("Hello World! - Beginning startup");


            //Runs setup if path file is not present
            SetupManager.CheckIfPathsFileExists();
            CsgoDataHandler.GenerateSouvenirCollections();


            //Setup
            CsgoDataHandler.GetRootWeaponSkin();

            //Main
            new MainProgram().MainAsync().GetAwaiter().GetResult();


        }

        public DiscordSocketClient _client;
        public CommandService _commands;
        public IServiceProvider _services;

        //Main
        public async Task MainAsync()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };

            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()

                //BuildsServiceProvider
                .BuildServiceProvider();

            //Bot init
            try
            {
                //Get token
                string token = File.ReadAllLines(FileAccessManager.GetFileLocation("BotToken.txt")).FirstOrDefault();

                //Connect to discord
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

            }
            catch (Exception)
            {
                throw new Exception("Unable to initialize! - Could it be because of an invalid token?");
            }

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            stopwatch.Stop();
            EventLogger.LogMessage($"Ready! - Took {stopwatch.ElapsedMilliseconds} milliseconds");

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



            //All commands before this
            await Task.Delay(-1);

        }

        //Command Handler
        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message, if sender is bot
            if (!(messageParam is SocketUserMessage message)) return;

            if (message.Author.IsBot) return;

            //integer to determine when commands start
            int argPos = 0;

            //If message is in a DM, return
            var chnl = messageParam.Channel as SocketGuildChannel;
            if (chnl == null) return;

            //Show prefix help if user mentions bot
            var context = new SocketCommandContext(_client, message);
            if (message.Content == ("<@" + context.Client.CurrentUser.Id.ToString() + ">"))
            {
                await context.Channel.SendMessageAsync($"Current guild prefix: `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}` | Get help with `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}help`");
                return;
            }

            //Ignore commands that are not using the prefix
            string commandPrefix = GuildCommandPrefixManager.GetGuildCommandPrefix(context);

            if (!(message.HasStringPrefix(commandPrefix, ref argPos) ||
                message.Author.IsBot))
                return;

            var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);


            //COMMAND LOGGING
            // Inform the user if the command fails
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand)
                {
                    //Find similar commands
                    var commandHelpDefinitionStorage = XmlManager.FromXmlFile<HelpMenuCommands>(FileAccessManager.GetFileLocation(@"CommandHelpDescription.xml"));
                    string similarItemsString = UserHelpHandler.FindSimilarCommands(
                        commandHelpDefinitionStorage.CommandHelpEntry.Select(i => i.CommandName).ToList(), 
                        message.ToString().Substring(GuildCommandPrefixManager.GetGuildCommandPrefix(context).Length + 1));

                    //If no similar matches are found, send nothing
                    if (string.IsNullOrEmpty(similarItemsString))
                    {
                        await context.Channel.SendMessageAsync($"Invalid command, use `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}help` for a list of commands");
                    }
                    //If similar matches are found, send suggestions
                    else
                    {
                        await context.Channel.SendMessageAsync($"Invalid command, use `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}help` for a list of commands. Did you mean: \n {similarItemsString}");
                    }
                    
                }
                else if (result.Error == CommandError.BadArgCount)
                {
                    await context.Channel.SendMessageAsync($"Invalid command usage, use `{GuildCommandPrefixManager.GetGuildCommandPrefix(context)}help <command>` for correct command usage");
                }
            }
        }
    }
}
