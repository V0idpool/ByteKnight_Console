using ByteKnightConsole.Helpers;
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
    public static class UpdateUserRoles
    {
        /// <summary>
        /// Periodically checks and updates user roles in all guilds the bot is a member of.
        /// Recalculates user levels based on their XP and ensures their roles are aligned with their level.
        /// Runs continuously, performing checks every 30 minutes.
        /// </summary>
        public static async Task PeriodicRoleCheck()
        {
            while (true)
            {
                try
                {
                    foreach (var guild in ByteKnightEngine.DiscordClient.Guilds) // Loop through all guilds the bot is in
                    {
                        Console.WriteLine($"Checking roles for guild: {guild.Name} (ID: {guild.Id})");

                        foreach (var user in guild.Users) // Loop through all users in the guild
                        {
                            // Fetch user level data from MongoDB
                            var userLevel = await ByteKnightEngine._userLevelsCollection.Find(
                                Builders<UserLevelData>.Filter.Where(u => u.ID == user.Id && u.ServerId == guild.Id)
                            ).FirstOrDefaultAsync();

                            if (userLevel != null)
                            {
                                int currentLevel = userLevel.Level;
                                int oldLevel = currentLevel; // Start with the current level

                                // Recalculate the level from XP (in case it changed outside of your existing logic)
                                int recalculatedLevel = userLevel.CalculateLevel();

                                if (recalculatedLevel != currentLevel)
                                {
                                    Console.WriteLine($"[PeriodicCheck] Updating roles for {user.Username}: Old Level = {currentLevel}, New Level = {recalculatedLevel}");
                                    await XPSystem.UpdateUserRoles(user, recalculatedLevel, oldLevel);

                                    // Optionally log level changes
                                    Console.WriteLine($"Guild: {guild} | {user} | Recalculated Level: {recalculatedLevel}");
                                }
                            }
                        }
                    }

                    Console.WriteLine("Periodic role check completed for all guilds.");

                    // Wait 30 minutes before the next check
                    await Task.Delay(TimeSpan.FromMinutes(30));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in periodic role check: {ex.Message}");
                }
            }
        }
    }
}
