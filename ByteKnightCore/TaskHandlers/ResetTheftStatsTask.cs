using ByteKnightConsole.MongoDBSchemas;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore
{
    /// <summary>
    /// Handles resetting monthly theft-related statistics for a specific server. 
    /// This class ensures that user theft stats (e.g., theft count, total stolen values) are reset on the first day of every month.
    /// </summary>
    public static class ResetMonthlyThefts
    {
        /// <summary>
        /// Resets the theft-related statistics for all users in the specified server on the 1st of each month. 
        /// If the server's settings or data do not exist, default values are initialized in the database.
        /// </summary>
        /// <param name="serverId">The ID of the server for which theft stats should be reset.</param>
        /// <returns>An asynchronous task that continuously monitors and resets theft stats as required.</returns>
        public static async Task Reset(ulong serverId)
        {
            while (true)
            {
                try
                {
                    var now = DateTime.Now;
                    // Fetch the LastTheftReset date for the specific ServerId from the database
                    var serverSettingsCollection = ByteKnightEngine._database.GetCollection<ServerSettings>(ByteKnightEngine._serverSettings);
                    var filter = Builders<ServerSettings>.Filter.Eq(s => s.ServerId, serverId);
                    var serverSettings = await serverSettingsCollection.Find(filter).FirstOrDefaultAsync();
                    if (serverSettings == null)
                    {
                        Console.WriteLine("ServerSettings entry not found. Please configure bot.");
                        serverSettings = new ServerSettings
                        {
                            Id = ObjectId.GenerateNewId(),
                            ServerId = serverId,
                            SetupNotificationSent = false,
                            WelcomeChannelId = 0,
                            RulesChannelId = 0,
                            WarnPingChannelId = 0, // Default value
                            TicketPingChannelId = 0,
                            LogPingChannelId = 0,
                            LevelChannelId = 0,
                            QuarantineChannelID = 0,
                            SecurityPingChannelID = 0,
                            SpamCount = 10,
                            SpamTimeFrame = 10,
                            StarboardChannelId = 0,
                            StarboardReactionThreshhold = 5,
                            AiHomeChannel = 0,
                            AutoRoleId = 0,
                            StreamerRole = 0,
                            WelcomeMessage = "",
                            YoutubeURL = "",
                            TwitchURL = "",
                            SteamURL = "",
                            FacebookURL = "",
                            InviteURL = "",
                            Prefix = "",
                            XPCooldown = 60,
                            XPAmount = 10, // Default XP amount
                            XPSystemEnabled = true,
                            VoiceXPAmount = 1,
                            VoiceXPEnabled = true,
                            RolePings = new Dictionary<string, ulong>(), // Correctly initialize as Dictionary<string, ulong>
                            LastTheftReset = DateTime.MinValue
                        };
                        await serverSettingsCollection.InsertOneAsync(serverSettings);
                    }
                    // Check if today is the 1st day of the month
                    if (now.Day == 1 && serverSettings.LastTheftReset.Date != now.Date)
                    {
                        Console.WriteLine($"[{now}] Resetting theft stats...");

                        var collection = ByteKnightEngine._database.GetCollection<UserLevelData>(ByteKnightEngine._userLevels);


                        // Reset fields to 0 only for users in the specific server
                        var userFilter = Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId);
                        var update = Builders<UserLevelData>.Update
                            .Set(u => u.StealCount, 0)
                            .Set(u => u.StealTotal, 0)
                            .Set(u => u.StolenFromCount, 0)
                            .Set(u => u.StolenTotal, 0);

                        await collection.UpdateManyAsync(userFilter, update);

                        // Update the LastTheftReset date in the database
                        var serverSettingsUpdate = Builders<ServerSettings>.Update.Set(s => s.LastTheftReset, now.Date);
                        await serverSettingsCollection.UpdateOneAsync(filter, serverSettingsUpdate);
                        Console.WriteLine($"[{now}] Theft stats reset successfully.");
                    }

                    // Wait until the next day to check again
                    var nextDay = DateTime.Now.AddDays(1).Date;
                    var delay = nextDay - DateTime.Now;
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting theft stats: {ex.Message}");
                }
            }
        }
    }
}
