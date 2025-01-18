using ByteKnightConsole.MongoDBSchemas;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ByteKnightConsole.MongoDBDriver
{
    /// <summary>
    /// Provides helper methods for user levels and server settings interactions with MongoDB.
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Saves a single user's level data to MongoDB.
        /// </summary>
        /// <param name="userLevel">The user level data to save.</param>
        public static async Task SaveUserLevel(UserLevelData userLevel)
        {
            var filter = Builders<UserLevelData>.Filter.Eq(u => u.ID, userLevel.ID) &
                         Builders<UserLevelData>.Filter.Eq(u => u.ServerId, userLevel.ServerId);
            var options = new ReplaceOptions { IsUpsert = true };
            await ByteKnightEngine._userLevelsCollection.ReplaceOneAsync(filter, userLevel, options);
        }

        /// <summary>
        /// Loads a single user's level data from MongoDB.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="serverId">The server's ID.</param>
        /// <returns>The user's level data, or null if not found.</returns>
        public static async Task<UserLevelData> LoadUserLevel(ulong userId, ulong serverId)
        {
            var filter = Builders<UserLevelData>.Filter.Eq(u => u.ID, userId) &
                         Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId);
            return await ByteKnightEngine._userLevelsCollection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Loads all user level data for a specific server from MongoDB.
        /// </summary>
        /// <param name="serverId">The server's ID.</param>
        /// <returns>A dictionary mapping user IDs to their level data.</returns>
        public static async Task<Dictionary<string, UserLevelData>> LoadUserLevels(ulong serverId)
        {
            var filter = Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId);
            var userLevelsList = await ByteKnightEngine._userLevelsCollection.Find(filter).ToListAsync();
            return userLevelsList.ToDictionary(u => u.ID.ToString(), u => u);
        }

        /// <summary>
        /// Retrieves server settings for a given guild from MongoDB, or creates new defaults if none exist.
        /// </summary>
        /// <param name="guildId">The guild's ID.</param>
        /// <returns>The server settings.</returns>
        public static async Task<ServerSettings> GetServerSettings(ulong guildId)
        {
            var filter = Builders<ServerSettings>.Filter.Eq(s => s.ServerId, guildId);
            var serverSettings = await ByteKnightEngine._serverSettingsCollection.Find(filter).FirstOrDefaultAsync();

            if (serverSettings == null)
            {
                serverSettings = new ServerSettings
                {
                    Id = ObjectId.GenerateNewId(),
                    ServerId = guildId,
                    WelcomeChannelId = 0,
                    RulesChannelId = 0,
                    WarnPingChannelId = 0,
                    AutoRoleId = 0,
                    StreamerRole = 0,
                    InviteURL = "",
                    Prefix = "",
                    XPCooldown = 60,
                    XPAmount = 10,
                    XPSystemEnabled = true,
                };

                await ByteKnightEngine._serverSettingsCollection.InsertOneAsync(serverSettings);
            }

            return serverSettings;
        }
        /// <summary>
        /// Saves the server settings to MongoDB.
        /// </summary>
        /// <param name="serverSettings">The server settings to save.</param>
        public static async Task SaveServerSettings(ServerSettings serverSettings)
        {
            var filter = Builders<ServerSettings>.Filter.Eq(s => s.ServerId, serverSettings.ServerId);

            if (serverSettings.Id == ObjectId.Empty)
            {
                serverSettings.Id = ObjectId.GenerateNewId();
            }

            Console.WriteLine($"Saving ServerSettings: Id={serverSettings.Id}, ServerId={serverSettings.ServerId}");

            var update = Builders<ServerSettings>.Update
                .Set(s => s.WelcomeChannelId, serverSettings.WelcomeChannelId)
                .Set(s => s.RulesChannelId, serverSettings.RulesChannelId)
                .Set(s => s.WarnPingChannelId, serverSettings.WarnPingChannelId)
                .Set(s => s.AutoRoleId, serverSettings.AutoRoleId)
                .Set(s => s.StreamerRole, serverSettings.StreamerRole)
                .Set(s => s.InviteURL, serverSettings.InviteURL)
                .Set(s => s.Prefix, serverSettings.Prefix)
                .Set(s => s.XPSystemEnabled, serverSettings.XPSystemEnabled)
                .Set(s => s.XPCooldown, serverSettings.XPCooldown)
                .Set(s => s.XPAmount, serverSettings.XPAmount);

            var options = new UpdateOptions { IsUpsert = true };

            await ByteKnightEngine._serverSettingsCollection.UpdateOneAsync(filter, update, options);
        }
    }
}
