using ByteKnightConsole.ByteKnightCore.CommandModules;
using ByteKnightConsole.Helpers;
using ByteKnightConsole.MongoDBDriver;
using ByteKnightConsole.MongoDBSchemas;
using ByteKnightConsole.ByteKnightCore;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Reflection;
using ByteKnightConsole.ByteKnightCore;
using ByteKnightConsole.ByteKnightCore.PrefixModules;
using ByteKnightConsole.ByteKnightCore.InteractionHandlers;
//
//                                                                                                                                  ByteKnight - CLI
//                                                                                                              Version: 1.2.0 (Public Release - CLI Version)
//                                                                                                             Author: ByteKnight Development Team (Voidpool)
//                                                                                                                           Release Date: [01/18/2025]
//
// Description:
// ByteKnight is a powerful, multi-purpose Discord bot built on top of Discord.Net and MongoDB, presented here in a streamlined console-based application.
// It offers a robust set of moderation tools (mute, ban, warn), a flexible leveling/XP system, numerous slash commands, automated role assignments, and more.
// Perfect for server admins who want a feature-rich, easily customizable bot—no GUI overhead required.
//
// Current Features:
//  - Verification & Auto Role assignment on join
//  - Role management (e.g., mute system) 
//  - User XP and level tracking, complete with leaderboards
//  - Customizable welcome messages and channels
//  - Full moderation toolkit: warnings, kicks, bans, purge, etc.
//  - Slash commands (roll, coinflip, 8ball, YouTube search, and more)
//  - Prefix commands
//
// Future Features (Subject to change as development continues):
//  - Custom embed creation for server admins
//  - Advanced moderation logging and analytics
//  - Expanded slash commands and interactive event handling
//  - Additional integrations for streaming and social platforms
//
// Notes:
//  - This public release focuses on the console-based core bot features.
//  - CodeForge offers a GUI-based version of ByteKnight with working examples for in-app backend settings (MongoDB, etc.) 
//    available under the "ByteKnight Apprentice" tier and the "ByteKnight Champion (AI)" tier.
//  - Both paid tiers provide more command modules, advanced commands in easy-to-use module formats, and dedicated support 
//    for additions/customizations.
//  - Expect ongoing updates, new commands, and refinements.
//  - Community feedback and contributions are always welcome!
//  - Support/donate at: https://buymeacoffee.com/byteknight
//  - Join the ByteKnight Discord (support, coding help, feature requests): https://discord.gg/trm9qEzcuw
//
namespace ByteKnightConsole
{
    public class ByteKnightEngine
    {
        // Ensure TopGG Vote only runs one task at a time.
        public static readonly SemaphoreSlim _voteCheckSemaphore = new SemaphoreSlim(1, 1);
        // Timer for the TopGG Vote Check
        public static System.Threading.Timer _voteCheckTimer;
        public Dictionary<string, UserLevelData> userLevels = new Dictionary<string, UserLevelData>();
        public Dictionary<ulong, DateTime> lastMessageTimes = new Dictionary<ulong, DateTime>();
        // Allow access to _client between MainProgram
        public static DiscordSocketClient _client;
        DiscordSocketClient client = new DiscordSocketClient();

        // Allow access to _mclient between MainProgram(If needed, this is here as an example)
        private MongoClient _mclient;
        public static DiscordSocketClient DiscordClient
        {
            //Access _client from other classes
            get { return _client; }
            private set { _client = value; }
        }
        public static string DiscordBotToken;
        public static string startupPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string userFile = "UserCFG.ini";
        // Check if the message author has the required permissions throughout all of ByteKnight, This makes it easier to reference across the entire project.
        public static bool HasPermission(IUserMessage message)
        {
            var user = message.Author as IGuildUser;
            return user != null && (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild);
        }
        string MongoDBConnectionURL;
        string MongoDBName;
        public static string contentstr;
        public static string BotNickname;
        public ulong _serverId;
        public bool isBotRunning = false;
        private bool shouldReconnect = true;
        // Define an event/action for log messages
        public event Action<string> LogReceived;
        public event Action<string, string> MessageReception;
        // Define the event/action
        public event Action<string> BotDisconnected;
        public event Func<Task> BotConnected;
        private CancellationTokenSource cancellationTokenSource;
        public const int DEFAULT_XP_COOLDOWN_SECONDS = 60;
        public static int XP_COOLDOWN_SECONDS = DEFAULT_XP_COOLDOWN_SECONDS;
        // MongoDB Connection String, Name, Collection names, etc. (read from UserCFG.ini)
        public static string _connectionString;
        public static string _databaseName;
        public static string _warnings;
        public static string _userLevels;
        public static string _mutes;
        public static string _serverSettings;
        public static ulong rulesChannel;
        public static ulong verificationChannel;
        public static ulong verificationAutoRole;
        public static InteractionService _interactionService;
        public static bool IsMongoDBInitialized { get; set; } = false;
        //Define MongoDB Database
        public static IMongoDatabase _database;
        // Define MongoDB Collections here
        public static IMongoCollection<Warning> _warningsCollection;
        public static IMongoCollection<UserLevelData> _userLevelsCollection;
        public static IMongoCollection<MuteInfo> _muteCollection;
        public static IMongoCollection<ServerSettings> _serverSettingsCollection;
        // Instance access
        public static ByteKnightEngine _instance;
        public static ByteKnightEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ByteKnightEngine();
                }
                return _instance;
            }
        }
        /// <summary>
        /// Main entry point of the application. Initializes and runs the bot.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static void Main(string[] args)
           => new ByteKnightEngine().RunBotAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Constructor for ByteKnightEngine. Initializes configuration settings from the UserCFG.ini file.
        /// </summary>
        public ByteKnightEngine()
        {
         

            string userConfigs = Path.Combine(startupPath, userFile);

            if (!File.Exists(userConfigs))
            {
                Console.WriteLine("UserCFG.ini not found in Application Directory, Creating file...");
                Module1.SaveToDisk("UserCFG.ini", startupPath + @"\UserCFG.ini");
            }

            // Load MongoDB Connection String and DB Name
            _connectionString = UserSettings(userConfigs, "MongoClientLink");
            _databaseName = UserSettings(userConfigs, "MongoDBName");
            // Load Collections contained inside of your MongoDB Name
            _serverSettings = UserSettings(userConfigs, "ServerSettingsName");
            _userLevels = UserSettings(userConfigs, "UserLevelsName");
            _warnings = UserSettings(userConfigs, "WarningsName");
            _mutes = UserSettings(userConfigs, "MuteName");

        }

        /// <summary>
        /// Loads API keys and configuration from the INI file into memory.
        /// </summary>
        public async Task LoadAPI()
        {
            // Load API Keys, and names from the INI file
            string userConfigs = Path.Combine(startupPath, userFile);
            DiscordBotToken = UserSettings(userConfigs, "DiscordBotToken");
            // Parse the string value of ServerId to Ulong to be used throughout the Bot
            string serverIdString = UserSettings(userConfigs, "ServerID");
            if (ulong.TryParse(serverIdString, out ulong serverId))
            {
                _serverId = serverId;
            }
            else
            {
                Console.WriteLine("Error: The server ID in the config file is not a valid ulong value.");
                // Handle the error (set _serverId to 0, or exit, etc.)
            }
            BotNickname = UserSettings(userConfigs, "BotNickname");
            // Parse the string value of RulesChannelID to Ulong to be used throughout the Bot
            string rulesChannelStr = UserSettings(userConfigs, "RulesChannelID");
            if (ulong.TryParse(rulesChannelStr, out ulong rulesChannelId))
            {
                rulesChannel = rulesChannelId;
              
            }
            // Parse the string value of VerificationChannelID to Ulong to be used throughout the Bot
            string verificationChannelStr = UserSettings(userConfigs, "VerificationChannelID");
            if (ulong.TryParse(verificationChannelStr, out ulong verificationChannelId))
            {
                verificationChannel = verificationChannelId;
            }
            // Parse the string value of VerificationAutoRoleID to Ulong to be used throughout the Bot
            string verificationAutoRoleStr = UserSettings(userConfigs, "VerificationAutoRoleID");
            if (ulong.TryParse(verificationAutoRoleStr, out ulong verificationAutoRoleId))
            {
                verificationAutoRole = verificationAutoRoleId;
            }
            Console.WriteLine(@"| API Keys Loaded. Opening connection to API Services | Status: Waiting For Connection...");
            // Check if the API keys are properly loaded
            if (string.IsNullOrEmpty(DiscordBotToken))
            {
                Console.WriteLine("Error: API Error, Settings not configured properly. Are your API Keys correct? Exiting thread.");
                return;
            }


        }
        /// <summary>
        /// Reads a specific setting from the given INI file.
        /// </summary>
        /// <param name="File">Path to the INI file.</param>
        /// <param name="Identifier">The key identifier to look for.</param>
        /// <returns>The value associated with the identifier.</returns>
        public string UserSettings(string File, string Identifier) // User Settings handler
        {
            using var S = new System.IO.StreamReader(File);
            string Result = "";
            while (S.Peek() != -1)
            {
                string Line = S.ReadLine();
                if (Line.ToLower().StartsWith(Identifier.ToLower() + "="))
                {
                    Result = Line.Substring(Identifier.Length + 1);
                }
            }
            return Result;
        }
        /// <summary>
        /// Starts the bot if it is not currently running.
        /// </summary>
        public async Task StartBotAsync()
        {
            if (!isBotRunning)
            {
                isBotRunning = true;
                // Allow reconnection attempts
                shouldReconnect = true;
                // Start the bot                        
                await RunBotAsync();

            }
        }
        /// <summary>
        /// Stops the bot if it is currently running.
        /// </summary>
        public async Task StopBot()
        {

            if (isBotRunning)
            {

                isBotRunning = false;
                // Prevent reconnection, stop the bot
                shouldReconnect = false;
                await DisconnectBot();

            }
        }

        // Flag to check if the bot is in the process of disconnecting
        private static bool isDisconnecting = false;
        private static SemaphoreSlim disconnectSemaphore = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Gracefully disconnects the bot from Discord, logging out and disposing resources.
        /// </summary>
        public async Task DisconnectBot()
        {
            try
            {

                await disconnectSemaphore.WaitAsync();
                // Set flag indicating disconnect is in progress
                isDisconnecting = true;
                if (_client != null && _instance != null)
                {
                    Console.WriteLine("[SYSTEM] Clearing background tasks...");

                    await _client.LogoutAsync();
                    try
                    {
                        // Stop the client
                        await _client.StopAsync();
                        Console.WriteLine("[SYSTEM] Stopping remaining tasks, Waiting...");
                    }
                    catch (Exception logoutException)
                    {
                        Console.WriteLine($"Logout Exception: {logoutException.Message}");
                    }
                    // Dispose of the client
                    _client.Dispose();

                    _client = null;
                    DiscordClient = null;
                    await Task.Delay(2500);
                    Console.WriteLine("[SYSTEM] Refreshed client, Client Ready...");

                }
            }
            finally
            {
                isDisconnecting = false;
                disconnectSemaphore.Release();
            }
        }
        /// <summary>
        /// Callback invoked when the Discord client successfully connects. Initializes MongoDB and registers slash commands.
        /// </summary>
        private async Task OnClientConnected()
        {
            Console.WriteLine("[SYSTEM]  ByteKnight connected to Discord...");

            // Initialize MongoDB and check if successful
            IsMongoDBInitialized = await Init.InitializeMongoDBAsync();
            if (!IsMongoDBInitialized)
            {
                Console.WriteLine("MongoDB Connection failed. Please check connection URL and database name.");
                return;
            }
            var slashCommandService = new SlashCommandService(_client);
            await slashCommandService.RegisterSlashCommandsAsync();
            Console.WriteLine("Slash commands registered.");
        }
        
        /// <summary>
        /// Initializes and runs the Discord bot, setting up event handlers and starting the client.
        /// </summary>
        public async Task RunBotAsync()
        {
            Console.Title = "ByteKnight Discord Bot";
            string userConfigs = Path.Combine(startupPath, userFile);
          
            if (!File.Exists(userConfigs))
            {
                ExtractResourceToFile("ByteKnightConsole.UserCFG.ini", userConfigs);
            }
            // Utilize cancellation token
            cancellationTokenSource = new CancellationTokenSource();
            // Ensure MongoDB is initialized before creating CommandsCore
            IsMongoDBInitialized = await Init.InitializeMongoDBAsync();
            if (!IsMongoDBInitialized)
            {
                Console.WriteLine("Failed to initialize MongoDB. Exiting...");
                return;
            }
            // Inject Command Handler
            var interactionHandler = new CommandsCore(_client, _database, this);
            // Inject Prefix Command Handler
            var prefixHandler = new PrefixCore(_client, _database, this);
            try
            {
                // Define your Gateway intents, messagecachesize, etc.
                var socketConfig = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences | GatewayIntents.MessageContent,
                    MessageCacheSize = 300
                };
                // Load user settings
                await LoadAPI();
                _client = new DiscordSocketClient(socketConfig);
                _client.Log += Log;
                _client.MessageReceived += async (message) =>
                {
                    if (message is not IUserMessage userMessage || message.Author.IsBot)
                        return;

                    // XP progression
                    await HandleMessageAsync(message);

                    // Prefix commands
                    if (userMessage.Content.StartsWith("!")) // Or your preferred prefix, this can also be set via a dashboard or ini/ENV file
                    {
                        await prefixHandler.HandlePrefixInteraction(userMessage);
                    }
                };
                _client.InteractionCreated += interactionHandler.HandleInteraction;
                _client.UserJoined += UserJoined;
                _client.Connected += OnClientConnected;
                _client.InteractionCreated += async (interaction) =>
                {
                    if (interaction is SocketMessageComponent component)
                    {
                        if (component.Data.CustomId.StartsWith("initiate_verify_button_") ||
                                component.Data.CustomId.StartsWith("final_verify_button_") ||
                                component.Data.CustomId.StartsWith("support_button_"))
                        {
                            await ButtonInteraction.HandleButtonInteraction(component); // Handle verification buttons
                        }
                        else if (component.Data.CustomId.StartsWith("toggle_vote_reminder"))
                        {
                            await ButtonInteraction.HandleButtonInteraction(component); // Handle reminder interactions
                        }

                        else if (component.Data.CustomId.StartsWith("toggle_steal_reminder"))
                        {
                            await ButtonInteraction.HandleButtonInteraction(component); // Handle reminder interactions
                        }
                    }
                };
                    _client.Disconnected += async (exception) =>
                {
                    Console.WriteLine($"[SYSTEM]  ByteKnight disconnected: {exception?.Message}");

                    if (shouldReconnect)
                    {
                        for (int i = 1; i <= 5; i++)  // Retry max 5 times
                        {
                            try
                            {
                                Console.WriteLine($"[SYSTEM] Attempting reconnect #{i}...");
                                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(5, 30)));  // Exponential backoff
                                await StartBotAsync();
                                return;  // Exit on successful reconnect
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SYSTEM] Reconnect attempt #{i} failed: {ex.Message}");
                            }
                        }
                        Console.WriteLine($"[SYSTEM] Failed to reconnect after 5 attempts. Stopping reconnections.");
                    }
                };
                await _client.LoginAsync(TokenType.Bot, DiscordBotToken);
                Console.WriteLine("[SYSTEM] Attempting Login...]");
                await _client.StartAsync();
                Console.WriteLine("[SYSTEM] Logging into Discord Services...");
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Bot stopping due to cancellation request.");
                DisconnectBot().GetAwaiter().GetResult();
    }
}
        /// <summary>
        /// Logs messages from the Discord client to the console and a file.
        /// </summary>
        /// <param name="arg">The log message received from Discord.</param>
        /// <returns>A completed task.</returns>
        private async Task<Task> Log(LogMessage arg)
        {
            string logText = $"{DateTime.Now} [{arg.Severity}] {arg.Source}: {arg.Exception?.ToString() ?? arg.Message}";
            // This can be used to SetOut Console to a file btw :P
            Console.WriteLine(logText);
            string filePath = Path.Combine(startupPath, "Bot_logs.txt");
            try
            {
                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        await sw.WriteLineAsync(logText);
                    }
                }
                else
                {
                    // Append the log text to the existing file
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        await sw.WriteLineAsync(logText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Processes received messages, updates user XP and leveling information accordingly. Can be further extended to handle more cases.
        /// </summary>
        /// <param name="arg">The socket message received.</param>
        public async Task HandleMessageAsync(SocketMessage arg)
        {

            var message = arg as SocketUserMessage;
            if (message == null || message.Author == null || message.Author.IsBot)
            {
                // Either the message is null, the author is null, or the author is a bot, so we ignore it
                return;
            }
            var guildUser = message.Author as SocketGuildUser;
            // Ensure the author is a guild user
            if (guildUser == null) return;


            string userfile = @"\UserCFG.ini";
            string botNickname = UserSettings(startupPath + userfile, "BotNickname");
            var now = DateTime.UtcNow;
            // Load server settings from MongoDB
            var serverSettingsCollection = _database.GetCollection<ServerSettings>(_serverSettings);
            var serverSettings = await serverSettingsCollection
                .Find(Builders<ServerSettings>.Filter.Eq(s => s.ServerId, guildUser.Guild.Id))
                .FirstOrDefaultAsync();
            // If XP system is disabled, return early and do not track XP (Use this with your dashboard, or set it via a command)
            if (serverSettings == null)
            {
                return; // XP system can be disabled, so no XP tracking will happen. Add another check for !serverSettings.XPSystemEnabled with serverSettings == null
            }
            // If no server-specific settings, fall back to defaults
            int xpCooldownSeconds = serverSettings?.XPCooldown ?? 60;
            int xpAmount = serverSettings?.XPAmount ?? 10;

            // Cooldown check
            if (lastMessageTimes.TryGetValue(message.Author.Id, out var lastMessageTime))
            {
                if ((now - lastMessageTime).TotalSeconds < xpCooldownSeconds)
                {
                    return; // Do not award XP if within cooldown period
                }
            }

            // Update last message time for the cooldown check
            lastMessageTimes[message.Author.Id] = now;

            // Load or create user level data
            var userLevel = await _userLevelsCollection.FindOneAndUpdateAsync(
                Builders<UserLevelData>.Filter.Where(u => u.ID == message.Author.Id && u.ServerId == guildUser.Guild.Id),
                Builders<UserLevelData>.Update.Inc(u => u.MessageCount, 1),
                new FindOneAndUpdateOptions<UserLevelData, UserLevelData> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
            );
            // Check if level increased
            int oldLevel = userLevel.Level;
            // Update XP in the database
            userLevel = await _userLevelsCollection.FindOneAndUpdateAsync(
                Builders<UserLevelData>.Filter.Where(u => u.ID == message.Author.Id && u.ServerId == guildUser.Guild.Id),
                Builders<UserLevelData>.Update.Inc(u => u.XP, xpAmount),
                new FindOneAndUpdateOptions<UserLevelData, UserLevelData> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
            );


            ulong levelchanid = serverSettings.LevelChannelId;
            var levelChannel = guildUser.Guild.GetTextChannel(levelchanid) as IMessageChannel;
            int newLevel = userLevel.Level;
            // Send a message to the channel the user leveled up in
            if (newLevel > oldLevel)
            {
                await levelChannel.SendMessageAsync($"Congratulations, {message.Author.Mention}! You've reached level {newLevel}!");
                // Update user roles based on new level
                await XPSystem.UpdateUserRoles(guildUser, newLevel, oldLevel);
                Console.WriteLine($"[XP] User '{guildUser.Username}' has leveled up to {newLevel}'.");
            }

        }
        /// <summary>
        /// Extracts an embedded resource from the assembly and writes it to a file.
        /// </summary>
        /// <param name="resourceName">Name of the embedded resource.</param>
        /// <param name="filePath">Destination file path.</param>
        static void ExtractResourceToFile(string resourceName, string filePath)
        {
            try
            {
                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        Console.WriteLine($"Error: Resource '{resourceName}' not found.");
                        return;
                    }
                    using (FileStream fileStream = File.Create(filePath))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                    Console.WriteLine($"Config File Not found, creating file: '{resourceName}' extracted to '{filePath}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting resource to file: {ex.Message}");
            }
        }
        /// <summary>
        /// Handles new user joining the guild by sending welcome and verification messages.
        /// </summary>
        /// <param name="user">The guild user who joined.</param>
        private async Task UserJoined(SocketGuildUser user)
        {
            var serverId = user.Guild.Id;
            string userConfigs = Path.Combine(startupPath, userFile);
            var serverSettings = await MongoDBDriver.Helpers.GetServerSettings(serverId);

            if (serverSettings == null)
            {
                Console.WriteLine("Server settings not found.");
                return;
            }

            var channels = user.Guild.Channels;

            // Parse RulesChannelId
            ulong rulesChannelId = serverSettings?.RulesChannelId ??
                (ulong.TryParse(UserSettings(userConfigs, "RulesChannelID"), out var parsedRulesChannelId) ? parsedRulesChannelId : 0);

            // Parse WelcomeChannelId
            ulong welcomeChannelId = serverSettings?.WelcomeChannelId ??
                (ulong.TryParse(UserSettings(userConfigs, "WelcomeChannelID"), out var parsedWelcomeChannelId) ? parsedWelcomeChannelId : 0);

            // Find the channels
            var rulesChannel = channels.OfType<ITextChannel>().FirstOrDefault(c => c.Id == rulesChannelId);
            var verifyWelcomeChannel = channels.OfType<ITextChannel>().FirstOrDefault(c => c.Id == welcomeChannelId);

            if (rulesChannel == null || verifyWelcomeChannel == null)
            {
                Console.WriteLine("Rules or Welcome channel not found.");
                return;
            }

            string[] welcomeMessages = new string[]
            {
        $"HEYO! Welcome to the server {user.Mention}! Be sure to read the Rules in the {rulesChannel.Mention}!",
        $"Greetings {user.Mention}! Enjoy your stay and don't forget to check out the Rules in {rulesChannel.Mention}.",
        $"Welcome to the server {user.Mention}! Enjoy your stay and don't forget to check out the Rules in {rulesChannel.Mention}.",
        $"Welcome, {user.Mention}! We're thrilled to have you in the server! Check out the Rules in {rulesChannel.Mention}.",
        $"Hey there, {user.Mention}! Feel free to explore and have fun! Don't forget to familiarize yourself with the Rules in {rulesChannel.Mention}.",
        $"Greetings, {user.Mention}! Your presence makes our server even better! Take a moment to review the Rules in {rulesChannel.Mention}.",
        $"Hello, {user.Mention}! Get ready for an awesome experience! Don't skip the Rules in {rulesChannel.Mention}."
            };

            int randomIndex = new Random().Next(0, welcomeMessages.Length);
            string selectedWelcomeMessage = welcomeMessages[randomIndex];

            var welcomeEmbed = new EmbedBuilder
            {
                Title = $"?? Welcome, {user.GlobalName}!",
                Color = Color.DarkRed,
                Description = selectedWelcomeMessage,
                Footer = new EmbedFooterBuilder
                {
                    Text = "\u200B\n?|ByteKnight Discord Bot|?",
                    IconUrl = "https://i.imgur.com/SejD45x.png"
                },
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/mat.png"
            };

            var verifyButton = new ButtonBuilder
            {
                Label = "? Begin Verification",
                CustomId = $"initiate_verify_button_{user.Id}",
                Style = ButtonStyle.Success
            };

            var supportButton = new ButtonBuilder
            {
                Label = "? Support",
                CustomId = $"support_button_{user.Id}",
                Style = ButtonStyle.Primary
            };

            var component = new ComponentBuilder()
                .WithButton(verifyButton)
                .WithButton(supportButton)
                .Build();

            var welcomeMessage = await verifyWelcomeChannel.SendMessageAsync(embed: welcomeEmbed.Build(), components: component);
            var pingMessage = await verifyWelcomeChannel.SendMessageAsync($"{user.Mention}, Please verify your account using the button above.");

            var userContext = new VerificationContexts
            {
                ServerId = serverId,
                UserId = user.Id,
                WelcomeMessageId = welcomeMessage.Id,
                PingMessageId = pingMessage.Id
            };

            // Update the user context in MongoDB
            await UserContextStore.AddOrUpdateAsync(userContext);
            // Log user information
            LogUserInformation(user);
        }
        /// <summary>
        /// Logs detailed information about a user when they join a server.
        /// </summary>
        /// <param name="user">The guild user whose information is logged.</param>
        private void LogUserInformation(SocketGuildUser user)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"User Joined: {user.Username}#{user.Discriminator}");
            Console.WriteLine($"User ID: {user.Id}");
            Console.WriteLine($"Join Time: {DateTime.UtcNow}");
            Console.WriteLine($"Guild: {user.Guild.Name}");
            Console.WriteLine($"Account Creation Date: {user.CreatedAt}");
            Console.WriteLine($"Joined Server: {user.JoinedAt?.ToString() ?? "Unknown"}");
            Console.WriteLine("==================================================");
        }
        /// <summary>
        /// Retrieves the top users by XP for a given server.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        /// <param name="serverId">The server's unique identifier.</param>
        /// <param name="count">The number of top users to retrieve.</param>
        /// <returns>A list of top users sorted by XP.</returns>
        public static async Task<List<UserLevelData>> GetTopUsers(IMongoDatabase database, ulong serverId, int count)
        {
            if (database == null)
            {
                Console.WriteLine("Database is null.");
                return new List<UserLevelData>();
            }

            var collection = database.GetCollection<UserLevelData>(_userLevels);
            if (collection == null)
            {
                Console.WriteLine("Collection is null.");
                return new List<UserLevelData>();
            }

            var filter = Builders<UserLevelData>.Filter.Eq("ServerId", serverId);
            var sort = Builders<UserLevelData>.Sort.Descending("XP");

            try
            {
                var topUsers = await collection.Find(filter)
                                               .Sort(sort)
                                               .Limit(count)
                                               .ToListAsync();

                if (topUsers == null || !topUsers.Any())
                {
                    Console.WriteLine("No top users found.");
                }

                return topUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching top users: {ex.Message}");
                return new List<UserLevelData>();
            }
        }
        /// <summary>
        /// Retrieves the top users along with their Discord guild user information.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        /// <param name="serverId">The server's unique identifier.</param>
        /// <param name="count">The number of top users to retrieve.</param>
        /// <returns>A list of key-value pairs pairing SocketGuildUser with their level data.</returns>
        public static async Task<List<KeyValuePair<SocketGuildUser, UserLevelData>>> GetTopUsersWithNamesAsync(IMongoDatabase database, ulong serverId, int count)
        {
            var topUsers = await GetTopUsers(database, serverId, count);

            if (topUsers == null || !topUsers.Any())
            {
                Console.WriteLine("Top users list is null or empty.");
                return new List<KeyValuePair<SocketGuildUser, UserLevelData>>();
            }

            var usersWithNames = new List<KeyValuePair<SocketGuildUser, UserLevelData>>();

            var guild = _client.GetGuild(serverId);
            if (guild == null)
            {
                Console.WriteLine("Guild is null.");
                return new List<KeyValuePair<SocketGuildUser, UserLevelData>>();
            }

            var guildUsersCollection = await guild.GetUsersAsync().FlattenAsync();
            if (guildUsersCollection == null || !guildUsersCollection.Any())
            {
                Console.WriteLine("Guild users collection is null or empty.");
            }

            foreach (var user in topUsers)
            {
                var guildUser = guildUsersCollection.FirstOrDefault(u => u.Id == user.ID) as SocketGuildUser;
                if (guildUser != null)
                {
                    usersWithNames.Add(new KeyValuePair<SocketGuildUser, UserLevelData>(guildUser, user));
                }
                else
                {
                    Console.WriteLine($"User with ID {user.ID} not found in guild.");
                }
            }

            return usersWithNames;
        }
    }
}