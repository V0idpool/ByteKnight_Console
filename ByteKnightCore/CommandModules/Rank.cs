using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Rank
    {
        private readonly ByteKnightEngine _botInstance;

        public Rank(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                await slashCommand.DeferAsync();

                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                var targetUser = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user")?.Value as IUser;

                if (targetUser == null)
                {
                    await slashCommand.FollowupAsync("Invalid command format. Please use `/rank --user @usermention`.", ephemeral: true);
                    return;
                }

                string username = targetUser.GlobalName ?? targetUser.Username;

                // Load user level information
                var userLevelInfo = await MongoDBDriver.Helpers.LoadUserLevel(targetUser.Id, serverId);
                if (userLevelInfo == null)
                {
                    await slashCommand.FollowupAsync("No Rank information for user.", ephemeral: true);
                    return;
                }

                // Retrieve user settings from MongoDB
                var userSettings = await ByteKnightEngine._userLevelsCollection.Find(Builders<UserLevelData>.Filter.And(
                    Builders<UserLevelData>.Filter.Eq("ID", targetUser.Id.ToString()),
                    Builders<UserLevelData>.Filter.Eq("ServerId", serverId.ToString())
                )).FirstOrDefaultAsync();

                // Get all users and find the rank of the target user (rank 1-1000)
                var allUsers = await ByteKnightEngine.GetTopUsers(ByteKnightEngine._database, serverId, 1000);
                // Find user’s rank, +1 because index starts at 0
                int rank = allUsers.FindIndex(u => u.ID == targetUser.Id) + 1;

                if (rank == 0)
                {
                    await slashCommand.FollowupAsync($"{username} is not ranked.", ephemeral: true);
                    return;
                }

                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $" {username}'s Stats ",
                        IconUrl = targetUser.GetAvatarUrl() ?? targetUser.GetDefaultAvatarUrl(),
                    },
                    Color = Color.Gold,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    },
                    Timestamp = DateTime.UtcNow,
                    ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/userstats.png",
                    Fields = new List<EmbedFieldBuilder>
        {
            new EmbedFieldBuilder
            {
                Name = "Level",
                Value = userLevelInfo.Level.ToString(),
                IsInline = true,
            },
            new EmbedFieldBuilder
            {
                Name = "XP",
                Value = userLevelInfo.XP.ToString(),
                IsInline = true,
            },
            new EmbedFieldBuilder
            {
                Name = "Messages Sent",
                Value = userLevelInfo.MessageCount.ToString(),
                IsInline = false,
            },
        },
                };

                await slashCommand.FollowupAsync(embed: embed.Build());
                Console.WriteLine("Rank Response sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
