using ByteKnightConsole;
using ByteKnightConsole.Helpers;
using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace ByteKnightConsole.Helpers
{
    public class XPSystem
    {
        private static ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> lastVoiceXpTimes = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();
        private static readonly Dictionary<int, string> LevelRoles = new Dictionary<int, string>
{
    { 10, "Level 10" },
    { 20, "Level 20" },
    { 30, "Level 30" },
    { 40, "Level 40" },
    { 50, "Level 50" },
    { 60, "Level 60" },
    { 70, "Level 70" },
    { 80, "Level 80" },
    { 90, "Level 90" },
    { 100, "Level 100" },
    // Add more levels as needed
};
        /// <summary>
        /// Starts monitoring voice chat activity across all servers to grant XP to users periodically.
        /// Runs continuously, checking for active users every minute.
        /// </summary>
        public static async Task MonitorVoiceChatXp()
        {
            Console.WriteLine("[XP] Starting voice chat XP monitoring.");
            while (true)
            {
                var tasks = ByteKnightEngine._client.Guilds.Select(guild => ProcessVoiceChannels(guild)).ToArray();
                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromMinutes(1)); // Check every 1 minute
            }
        }
        /// <summary>
        /// Processes all voice channels in a specific guild to grant XP to eligible users.
        /// Skips the AFK channel and bot users.
        /// </summary>
        /// <param name="guild">The guild to process voice channels for.</param>
        private static async Task ProcessVoiceChannels(SocketGuild guild)
        {
            var inactiveChannelId = guild.AFKChannel?.Id;

            foreach (var voiceChannel in guild.VoiceChannels)
            {
                // Skip the AFK channel
                if (inactiveChannelId.HasValue && voiceChannel.Id == inactiveChannelId.Value)
                {
                    continue;
                }

                // Grant XP to all eligible users in the voice channel
                var tasks = voiceChannel.Users
                    .Where(user => !user.IsBot) // Skip bots
                    .Select(user => GrantVoiceChatXp(user, inactiveChannelId))
                    .ToArray();

                await Task.WhenAll(tasks); // Process all users concurrently
            }
        }
        /// <summary>
        /// Grants XP to a specific user in a voice channel, ensuring they meet all eligibility criteria
        /// (e.g., not a bot, not in an inactive channel, and outside the cooldown period).
        /// Updates the user's XP in the database and checks for level-up events.
        /// </summary>
        /// <param name="user">The user to grant XP to.</param>
        /// <param name="inactiveChannelId">The ID of the guild's inactive (AFK) channel.</param>
        private static async Task GrantVoiceChatXp(SocketGuildUser user, ulong? inactiveChannelId)
        {

            var voiceChannel = user.VoiceChannel;
            var serverId = user.Guild.Id;
            var userId = user.Id;
            var isAdmin = (user as IGuildUser)?.GuildPermissions.Administrator ?? false;
            // Load server settings from MongoDB
            var serverSettings = await ByteKnightConsole.MongoDBDriver.Helpers.GetServerSettings(serverId);
            if (voiceChannel == null)
            {
                return;
            }
            if (user.IsBot || user.VoiceChannel == null)
            {
                return; // Skip bot users and users not in a voice channel
            }

            // Skip users in inactive channels
            if (inactiveChannelId.HasValue && user.VoiceChannel.Id == inactiveChannelId.Value)
            {
                return;
            }
            var now = DateTime.UtcNow;


            // Check if voice XP is enabled
            if (!serverSettings.VoiceXPEnabled)
            {
                // Console.WriteLine($"Voice XP not enabled for server: {user.Guild.Name}");
                return;
            }

            // Fetch all users currently in the voice channel (exclude bots)
            var connectedUsers = voiceChannel.Users
                .Where(u => u.VoiceChannel != null && u.VoiceChannel.Id == voiceChannel.Id && !u.IsBot)
                .ToList(); // Exclude bots and ensure they're in the voice channel

            if (connectedUsers.Count <= 1 && !isAdmin)
            {
                return;
            }

            // Get or create the dictionary for the server
            var userLastXpTimes = lastVoiceXpTimes.GetOrAdd(serverId, new ConcurrentDictionary<ulong, DateTime>());

            // Check cooldown
            if (userLastXpTimes.TryGetValue(userId, out var lastGrantTime))
            {
                if ((now - lastGrantTime).TotalSeconds < ByteKnightEngine.XP_COOLDOWN_SECONDS)
                {
                    var cooldownremainder = (now - lastGrantTime).TotalSeconds;

                    return; // Skip if within cooldown period
                }
            }



            // Update last XP grant time
            userLastXpTimes[userId] = now;

            // Load or create user level data
            var userLevel = await ByteKnightEngine._userLevelsCollection.FindOneAndUpdateAsync(
                Builders<UserLevelData>.Filter.Where(u => u.ID == userId && u.ServerId == serverId),
                Builders<UserLevelData>.Update.Inc(u => u.XP, 0), // Initial increment with 0 to load the user level data
                new FindOneAndUpdateOptions<UserLevelData, UserLevelData> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
            );
            int oldLevel = userLevel.Level;
            // Determine the amount of XP to grant based on server settings
            var xpGranted = serverSettings.VoiceXPAmount;

            // Apply double XP if active
            if (IsDoubleXpActive(userLevel))
            {
                xpGranted *= 2;
                // Console.WriteLine($"[XP] User '{user.Username}' is receiving double XP.");
            }

            // Update the user's XP in the database
            userLevel = await ByteKnightEngine._userLevelsCollection.FindOneAndUpdateAsync(
                Builders<UserLevelData>.Filter.Where(u => u.ID == userId && u.ServerId == serverId),
                Builders<UserLevelData>.Update.Inc(u => u.XP, xpGranted),
                new FindOneAndUpdateOptions<UserLevelData, UserLevelData> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
            );

            // Check if the user leveled up

            int newLevel = userLevel.Level;

            ulong levelchanid = serverSettings.LevelChannelId;
            if (newLevel > oldLevel)
            {
                var levelChannel = user.Guild.GetTextChannel(levelchanid) as IMessageChannel;
                if (levelChannel != null)
                {
                    await levelChannel.SendMessageAsync($"Congratulations, {user.Mention}! You've reached level {newLevel}!");
                    await UpdateUserRoles(user, newLevel, oldLevel);
                    Console.WriteLine($"[XP] User '{user.Username}' has leveled up to {newLevel}'.");
                }
            }
            else
            {
                Console.WriteLine($"[XP] Granted {xpGranted} XP to user '{user.Username}'. Current XP: {userLevel.XP}");
            }
        }
        /// <summary>
        /// Ensures that all level roles exist in the guild and are correctly ordered.
        /// Creates missing roles and reorders them based on their level values.
        /// </summary>
        /// <param name="guild">The guild to ensure roles for.</param>
        public static async Task EnsureAllLevelRolesExist(SocketGuild guild)
        {


            var sortedLevelRoles = LevelRoles.OrderByDescending(kv => kv.Key).ToList(); // Descending order

            // Get all roles and find the lowest position
            var allRoles = guild.Roles.OrderByDescending(r => r.Position).ToList();
            int lowestPosition = allRoles.LastOrDefault()?.Position ?? 0;

            foreach (var levelRole in sortedLevelRoles)
            {
                await EnsureRoleExists(guild, levelRole.Value, lowestPosition);
                lowestPosition++; // Increase position to move roles upwards in descending order
            }

            await ReorderRoles(guild);
        }

        /// <summary>
        /// Ensures a specific role exists in the guild and sets its position if necessary.
        /// Creates the role if it doesn't exist or updates its position if it differs from the desired position.
        /// </summary>
        /// <param name="guild">The guild to create or update the role in.</param>
        /// <param name="roleName">The name of the role to ensure.</param>
        /// <param name="position">The desired position of the role.</param>
        private static async Task EnsureRoleExists(SocketGuild guild, string roleName, int position)
        {
            var role = guild.Roles.FirstOrDefault(r => r.Name == roleName);

            if (role == null)
            {
                // Create the role if it doesn't exist
                await RateLimits.RetryOnRateLimit(async () =>
                {
                    var restRole = await guild.CreateRoleAsync(roleName, isMentionable: false);
                    role = guild.GetRole(restRole.Id);
                    // Immediately set the position if the role was just created
                    await role.ModifyAsync(properties => properties.Position = position);
                });
            }
            else
            {
                // Only modify the position if the desired position is different
                if (role.Position != position)
                {
                    await RateLimits.RetryOnRateLimit(async () => await role.ModifyAsync(properties => properties.Position = position));
                }
            }
        }
        /// <summary>
        /// Reorders all level roles in the guild to ensure they are positioned correctly based on their level values.
        /// Higher-level roles are positioned above lower-level roles.
        /// </summary>
        /// <param name="guild">The guild to reorder roles for.</param>
        private static async Task ReorderRoles(SocketGuild guild)
        {
            // Fetch all roles from the guild, sorted by position descending (higher roles first)
            var allRoles = guild.Roles.OrderByDescending(r => r.Position).ToList();

            // Filter out the level roles and sort them by level
            var levelRoles = allRoles
                .Where(r => LevelRoles.Values.Contains(r.Name))
                .OrderByDescending(r => int.Parse(r.Name.Replace("Level ", ""))) // Extract and sort by numeric value
                .ToList();

            // Get the starting position based on the lowest position in the existing roles
            int lowestPosition = allRoles.LastOrDefault()?.Position ?? 0;

            // Create a list of reorder properties for level roles
            var positions = new List<ReorderRoleProperties>();

            // Start positioning level roles from the lowest position upwards
            for (int i = 0; i < levelRoles.Count; i++)
            {
                var targetPosition = lowestPosition + i + 1; // Ensure that we increment positions
                if (levelRoles[i].Position != targetPosition)
                {
                    positions.Add(new ReorderRoleProperties(levelRoles[i].Id, targetPosition));
                }
            }

            // Reorder the roles if there are any position changes
            if (positions.Any())
            {
                await RateLimits.RetryOnRateLimit(async () => await guild.ReorderRolesAsync(positions));
            }
        }

        /// <summary>
        /// Updates a user's roles in a guild based on their new level. Removes roles for levels
        /// lower or higher than the user's current level and ensures the correct role for the new level is added.
        /// </summary>
        /// <param name="user">The user to update roles for.</param>
        /// <param name="newLevel">The user's new level.</param>
        /// <param name="oldLevel">The user's previous level.</param>
        public static async Task UpdateUserRoles(SocketGuildUser user, int newLevel, int oldLevel)
        {
            var guild = user.Guild;

            // Ensure all level roles exist and are ordered correctly
            await EnsureAllLevelRolesExist(guild);

            // Determine the highest role the user should have for their new level
            var newLevelRole = LevelRoles
                .Where(lr => lr.Key <= newLevel)
                .OrderByDescending(lr => lr.Key)
                .FirstOrDefault();

            var rolesToAdd = new List<SocketRole>();
            var rolesToRemove = new List<SocketRole>();

            // Cache guild roles for efficiency
            var guildRoles = guild.Roles.ToList();

            // Go through all the level roles
            foreach (var levelRole in LevelRoles)
            {
                var role = guildRoles.FirstOrDefault(r => r.Name == levelRole.Value);

                if (role != null && user.Roles.Contains(role))
                {
                    // Remove roles for both lower and higher levels than the current level
                    if (levelRole.Key != newLevelRole.Key) // Keep only the role for the user's current level
                    {
                        rolesToRemove.Add(role);
                    }
                }
            }

            // Add the correct role for the new level if the user doesn't have it already
            if (newLevelRole.Value != null)
            {
                var role = guildRoles.FirstOrDefault(r => r.Name == newLevelRole.Value);
                if (role != null && !user.Roles.Contains(role))
                {
                    rolesToAdd.Add(role);
                }
            }

            try
            {
                Console.WriteLine($"Processing role updates for user {user.Username} (ID: {user.Id})");

                // Remove the old roles
                if (rolesToRemove.Any())
                {
                    Console.WriteLine($"Removing roles: {string.Join(", ", rolesToRemove.Select(r => r.Name))}");
                    await RateLimits.RetryOnRateLimit(async () => await user.RemoveRolesAsync(rolesToRemove));
                }

                // Add the new role
                if (rolesToAdd.Any())
                {
                    Console.WriteLine($"Adding roles: {string.Join(", ", rolesToAdd.Select(r => r.Name))}");
                    await RateLimits.RetryOnRateLimit(async () => await user.AddRolesAsync(rolesToAdd));
                }

                Console.WriteLine($"Role updates completed for user {user.Username} (ID: {user.Id})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating roles for user {user.Username} (ID: {user.Id}): {ex.Message}");
            }
        }
        /// <summary>
        /// Checks whether a user has an active double XP bonus.
        /// </summary>
        /// <param name="userLevel">The user's level data to check.</param>
        /// <returns>True if the user has an active double XP bonus; otherwise, false.</returns>
        private static bool IsDoubleXpActive(UserLevelData userLevel)
        {
            return userLevel.DoubleXpExpiry.HasValue && userLevel.DoubleXpExpiry.Value > DateTime.UtcNow;
        }
        /// <summary>
        /// Removes expired double XP bonuses from all users in the database.
        /// Clears the `DoubleXpExpiry` field for users whose bonus has expired.
        /// </summary>
        public static async Task RemoveExpiredDoubleXpAsync()
        {
            var filter = Builders<UserLevelData>.Filter.Lt(u => u.DoubleXpExpiry, DateTime.UtcNow);
            var update = Builders<UserLevelData>.Update.Unset(u => u.DoubleXpExpiry);

            await ByteKnightEngine._userLevelsCollection.UpdateManyAsync(filter, update);
        }
        /// <summary>
        /// Periodically removes expired double XP bonuses from the database.
        /// Runs continuously, checking for expired bonuses every hour.
        /// </summary>
        public static async Task PeriodicDoubleXpCleanup()
        {
            while (true)
            {
                await RemoveExpiredDoubleXpAsync();
                await Task.Delay(TimeSpan.FromHours(1)); // Check every hour
            }
        }
    }
}
