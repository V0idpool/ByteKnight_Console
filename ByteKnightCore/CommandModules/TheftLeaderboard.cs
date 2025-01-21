using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using System.Text;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class TheftLeaderboard
    {
        private readonly ByteKnightEngine _botInstance;

        public TheftLeaderboard(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                await slashCommand.DeferAsync();

                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                int leaderboardSize = 10;
                // Query the top users based on StealTotal
                var topThieves = await GetTopThievesWithNamesAsync(ByteKnightEngine._database, serverId, leaderboardSize);

                if (topThieves == null || !topThieves.Any())
                {
                    await slashCommand.RespondAsync("No top thieves found.");
                    return;
                }

                var embed = new EmbedBuilder
                {
                    Title = "💎 Thieves of the Month 💎",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    },
                    Timestamp = DateTime.UtcNow,
                    ThumbnailUrl = "https://voidbot.lol/img/coin.png", // Replace with a heist-themed image
                };

                int rank = 1;
                var mentions = new StringBuilder();

                foreach (var user in topThieves)
                {
                    var netTheft = user.Value.StealTotal - user.Value.StolenTotal;
                    mentions.AppendLine($"**#{rank}**. {user.Key}\nNet XP Theft: {netTheft} XP");
                    rank++;
                }


                embed.Description = mentions.ToString() + $"\n*This resets on the 1st of each month";
                await slashCommand.RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing theft leaderboard: {ex.Message}");
                //await slashCommand.FollowupAsync("An error occurred while processing the theft leaderboard.");
            }
        }
        public static async Task<List<KeyValuePair<string, UserLevelData>>> GetTopThievesWithNamesAsync(IMongoDatabase database, ulong serverId, int limit)
        {
            var collection = database.GetCollection<UserLevelData>(ByteKnightEngine._userLevels);

            // Query to filter users with theft activity
            var filter = Builders<UserLevelData>.Filter.And(
                Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId),
                Builders<UserLevelData>.Filter.Or(
                    Builders<UserLevelData>.Filter.Gt(u => u.StealTotal, 0),
                    Builders<UserLevelData>.Filter.Gt(u => u.StolenTotal, 0)
                )
            );

            // Fetch matching users
            var users = await collection.Find(filter).ToListAsync();

            // Compute NetTheft in memory and filter for positive NetTheft
            var sortedUsers = users
                .Select(u => new
                {
                    UserData = u,
                    NetTheft = u.StealTotal - u.StolenTotal
                })
                .Where(u => u.NetTheft > 0)
                .OrderByDescending(u => u.NetTheft)
                .Take(limit)
                .ToList();

            // Use stored usernames instead of relying on guild cache
            var result = sortedUsers
                .Select(u => new KeyValuePair<string, UserLevelData>(
                    $"<@{u.UserData.ID}>", // Mention using user ID
                    u.UserData))
                .ToList();

            return result;
        }
    }
}
