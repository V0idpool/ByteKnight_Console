using Discord.WebSocket;
using MongoDB.Driver;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    /// <summary>
    /// Represents the core handler for managing Discord commands in the ByteKnight-CLI Framework.
    /// This class initializes all individual command handlers, processes incoming interactions,
    /// and routes slash commands to their respective handlers.
    /// </summary>
    public class CommandsCore
    {
        private readonly DiscordSocketClient _client;
        private readonly IMongoDatabase _database;
        private readonly ByteKnightEngine _botInstance;
        // Individual command handlers
        private readonly Level _levelHandler;
        private readonly Setup _setupHandler;
        private readonly Warn _warnHandler;
        private readonly WarnInfo _warnInfoHandler;
        private readonly WarnClear _warnClearHandler;
        private readonly GiveXP _giveXPHandler;
        private readonly GiveAllXP _giveAllXPHandler;
        private readonly RemoveXP _removeXPHandler;
        private readonly Roll _rollHandler;
        private readonly EightBall _8BallHandler;
        private readonly Leaderboard _leaderboardHandler;
        private readonly Rank _rankHandler;
        private readonly Duel _duelHandler;
        private readonly Help _helpHandler;
        private readonly Mute _muteHandler;
        private readonly UnMute _unmuteHandler;
        private readonly Say _sayHandler;
        private readonly Kick _kickHandler;
        private readonly SoftBan _softBanHandler;
        private readonly Ban _banHandler;
        private readonly Youtube _ytHandler;
        private readonly PM _pmHandler;
        private readonly Streaming _liveHandler;
        private readonly Purge _purgeHandler;
        private readonly Coinflip _coinflipHandler;
        private readonly GoogleIt _googleitHandler;
        private readonly Slap _slapHandler;
        private readonly Steal _stealHandler;
        private readonly UpdateEmbed _updateEmbedHandler;
        private readonly Verify _verifyHandler;
        private readonly Reminders _remindersHandler;
        private readonly Vote _voteHandler;
        private readonly TheftStats _theftStatsHandler;
        private readonly TheftLeaderboard _theftLeaderboardHandler;
        private readonly CustomReminders _customReminderHandler;


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandsCore"/> class.
        /// Sets up the necessary references to the Discord client, MongoDB database, and ByteKnightEngine,
        /// and initializes individual command handlers for managing bot commands.
        /// </summary>
        /// <param name="client">The Discord socket client instance.</param>
        /// <param name="database">The MongoDB database instance.</param>
        /// <param name="botInstance">The ByteKnight engine instance.</param>
        public CommandsCore(DiscordSocketClient client, IMongoDatabase database, ByteKnightEngine botInstance)
        {
            // Ensure proper instances are used and shared across project
            _client = client;
            _database = database;
            _botInstance = botInstance;
            // Initialize command handlers
            _levelHandler = new Level(botInstance);
            _setupHandler = new Setup(botInstance);
            _warnHandler = new Warn(botInstance);
            _warnInfoHandler = new WarnInfo(botInstance);
            _warnClearHandler = new WarnClear(botInstance);
            _giveXPHandler = new GiveXP(botInstance);
            _giveAllXPHandler = new GiveAllXP(botInstance);
            _removeXPHandler = new RemoveXP(botInstance);
            _rollHandler = new Roll(botInstance);
            _8BallHandler = new EightBall(botInstance);
            _leaderboardHandler = new Leaderboard(botInstance);
            _rankHandler = new Rank(botInstance);
            _duelHandler = new Duel(botInstance);
            _helpHandler = new Help(botInstance);
            _muteHandler = new Mute(botInstance);
            _unmuteHandler = new UnMute(botInstance);
            _sayHandler = new Say(botInstance);
            _kickHandler = new Kick(botInstance);
            _softBanHandler = new SoftBan(botInstance);
            _banHandler = new Ban(botInstance);
            _ytHandler = new Youtube(botInstance);
            _pmHandler = new PM(botInstance);
            _liveHandler = new Streaming(botInstance);
            _purgeHandler = new Purge(botInstance);
            _coinflipHandler = new Coinflip(botInstance);
            _googleitHandler = new GoogleIt(botInstance);
            _slapHandler = new Slap(botInstance);
            _stealHandler = new Steal(botInstance);
            _updateEmbedHandler = new UpdateEmbed(botInstance);
            _verifyHandler = new Verify(botInstance);
            _remindersHandler = new Reminders(botInstance);
            _voteHandler = new Vote(botInstance);
            _theftStatsHandler = new TheftStats(botInstance);
            _theftLeaderboardHandler = new TheftLeaderboard(botInstance);
            _customReminderHandler = new CustomReminders(botInstance);
        }
        /// <summary>
        /// Handles incoming interactions from Discord, such as slash commands.
        /// Routes each slash command to its corresponding command handler based on the command name.
        /// </summary>
        /// <param name="interaction">The interaction received from Discord.</param>
        /// <remarks>
        /// Logs unexpected errors in command handling.
        /// </remarks>
        public async Task HandleInteraction(SocketInteraction interaction)
        {
            if (interaction is SocketSlashCommand slashCommand)
            {
                var verifiedServerId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                switch (slashCommand.Data.Name)
                {
                    case "level":
                        await _levelHandler.CommandHandler(slashCommand);
                        break;

                    case "setup":
                        await _setupHandler.CommandHandler(slashCommand);
                        break;

                    case "warn":
                        await _warnHandler.CommandHandler(slashCommand);
                        break;

                    case "warninfo":
                        await _warnInfoHandler.CommandHandler(slashCommand);
                        break;
                        
                    case "warnclear":
                        await _warnClearHandler.CommandHandler(slashCommand);
                        break;

                    case "giveallxp":
                        await _giveAllXPHandler.CommandHandler(slashCommand);
                        break;

                    case "givexp":
                        await _giveXPHandler.CommandHandler(slashCommand);
                        break;

                    case "removexp":
                        await _removeXPHandler.CommandHandler(slashCommand);
                        break;

                    case "roll":
                        await _rollHandler.CommandHandler(slashCommand);
                        break;

                    case "8ball":
                        await _8BallHandler.CommandHandler(slashCommand);
                        break;

                    case "leaderboard":
                        await _leaderboardHandler.CommandHandler(slashCommand);
                        break;

                    case "rank":
                        await _rankHandler.CommandHandler(slashCommand);
                        break;

                    case "duel":
                        await _duelHandler.CommandHandler(slashCommand);
                        break;

                    case "help":
                        await _helpHandler.CommandHandler(slashCommand);
                        break;

                    case "mute":
                        await _muteHandler.CommandHandler(slashCommand);
                        break;

                    case "unmute":
                        await _unmuteHandler.CommandHandler(slashCommand);
                        break;

                    case "say":
                        await _sayHandler.CommandHandler(slashCommand);
                        break;

                    case "kick":
                        await _kickHandler.CommandHandler(slashCommand);
                        break;

                    case "softban":
                        await _softBanHandler.CommandHandler(slashCommand);
                        break;

                    case "ban":
                        await _banHandler.CommandHandler(slashCommand);
                        break;

                    case "yt":
                        await _ytHandler.CommandHandler(slashCommand);
                        break;

                    case "pm":
                        await _pmHandler.CommandHandler(slashCommand);
                        break;

                    case "live":
                        await _liveHandler.CommandHandler(slashCommand);
                        break;

                    case "purge":
                        await _purgeHandler.CommandHandler(slashCommand);
                        break;

                    case "coinflip":
                        await _coinflipHandler.CommandHandler(slashCommand);
                        break;
                        
                    case "googleit":
                        await _googleitHandler.CommandHandler(slashCommand);
                        break;
                
                    case "slap":
                        await _slapHandler.CommandHandler(slashCommand);
                        break;
                                    
                    case "steal":
                        await _stealHandler.CommandHandler(slashCommand);
                        break;
                              
                    case "theftstats":
                        await _theftStatsHandler.CommandHandler(slashCommand);
                        break;
                        
                    case "theftleaderboard":
                        await _theftLeaderboardHandler.CommandHandler(slashCommand);
                        break;
             
                    case "updatemsg":
                        await _updateEmbedHandler.CommandHandler(slashCommand);
                        break;

                    case "verify":
                        await _verifyHandler.CommandHandler(slashCommand);
                        break;
                        
                    case "reminders":
                        await _remindersHandler.CommandHandler(slashCommand);
                        break;
                                 
                    case "remindme":
                        await _customReminderHandler.CommandHandler(slashCommand);
                        break;
                             
                    case "vote":
                        await _voteHandler.CommandHandler(slashCommand);
                        break;


                        // Other commands...
                }

            }
            else
            {
                Console.WriteLine($"Unexpected error in Command Handling.");
            }
        }
    }
}
