using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Level
    {
        private readonly ByteKnightEngine _botInstance;

        public Level(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            await slashCommand.DeferAsync();

            var userId = slashCommand.User.Id;
            var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
            string username = slashCommand.User.GlobalName ?? slashCommand.User.Username;

            // Load user level information from MongoDB
            var userLevel = await MongoDBDriver.Helpers.LoadUserLevel(userId, serverId);
            if (userLevel == null)
            {
                await slashCommand.FollowupAsync("No Rank information for user.", ephemeral: true);
                return;
            }
            // Fetch the list, 1000 is the amount of people to count in the rank list (rank 1-1000)
            var topUsers = await ByteKnightEngine.GetTopUsers(ByteKnightEngine._database, serverId, 1000);
            // Rank is 1-based index
            int rank = topUsers.FindIndex(u => u.ID == userId) + 1;

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $" {username}'s Stats ",
                    IconUrl = slashCommand.User.GetAvatarUrl() ?? slashCommand.User.GetDefaultAvatarUrl(),
                },
                Color = Color.Gold,
                Footer = new EmbedFooterBuilder
                {
                    Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                    IconUrl = "https://i.imgur.com/SejD45x.png"
                },
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/userstats.png",
                Fields = new List<EmbedFieldBuilder>
        {
            new EmbedFieldBuilder
            {
                Name = "Level",
                Value = userLevel.Level.ToString(),
                IsInline = true,
            },
            new EmbedFieldBuilder
            {
                Name = "XP",
                Value = userLevel.XP.ToString(),
                IsInline = true,
            },
            new EmbedFieldBuilder
            {
                Name = "Messages Sent",
                Value = userLevel.MessageCount.ToString(),
                IsInline = false,
            },
            new EmbedFieldBuilder
            {
                Name = "Rank",
                Value = rank > 0 ? $"#{rank}" : "Unranked",
                IsInline = true,
            },
        },
            };
            await slashCommand.FollowupAsync(embed: embed.Build());
            Console.WriteLine("Level Response sent");
        }
    }
}
