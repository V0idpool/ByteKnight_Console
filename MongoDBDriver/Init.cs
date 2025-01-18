using ByteKnightConsole.ByteKnightCore.CommandModules;
using ByteKnightConsole.MongoDBSchemas;
using MongoDB.Driver;
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
//  - Auto Role assignment on join
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
namespace ByteKnightConsole.MongoDBDriver
{
    /// <summary>
    /// Provides initialization routines for the bot, including connecting to MongoDB.
    /// </summary>
    public static class Init
    {
        /// <summary>
        /// Initializes the connection to MongoDB and prepares required collections.
        /// </summary>
        /// <returns>True if initialization succeeded; otherwise, false.</returns>
        public static async Task<bool> InitializeMongoDBAsync()
        {
            if (string.IsNullOrWhiteSpace(ByteKnightEngine._connectionString) || string.IsNullOrWhiteSpace(ByteKnightEngine._databaseName))
            {
                Console.WriteLine("MongoDB connection string or database name is missing. Please provide them before running the bot.");
                return false;
            }

            try
            {
                // Initialize MongoDB client and database
                var client = new MongoClient(ByteKnightEngine._connectionString);
                ByteKnightEngine._database = client.GetDatabase(ByteKnightEngine._databaseName);
                // Get collections
                ByteKnightEngine._warningsCollection = ByteKnightEngine._database.GetCollection<Warning>(ByteKnightEngine._warnings);
                ByteKnightEngine._userLevelsCollection = ByteKnightEngine._database.GetCollection<UserLevelData>(ByteKnightEngine._userLevels);
                ByteKnightEngine._muteCollection = ByteKnightEngine._database.GetCollection<MuteInfo>(ByteKnightEngine._mutes);
                ByteKnightEngine._serverSettingsCollection = ByteKnightEngine._database.GetCollection<ServerSettings>(ByteKnightEngine._serverSettings);
                // Call method to cleanup warnings older than 30 days
                await Warn.InitializeWarningCleanup();
                Console.WriteLine("MongoDB client initialized successfully.");
                // Mark MongoDB as initialized
                ByteKnightEngine.IsMongoDBInitialized = true;
                return true;
            }
            catch (MongoConfigurationException ex)
            {
                Console.WriteLine($"Error initializing MongoDB connection: {ex.Message}");
                return false;
            }
        }
    }
}
