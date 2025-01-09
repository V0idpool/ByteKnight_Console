using Discord;
using Discord.WebSocket;
using System.Text;
using Color = Discord.Color;
namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Leaderboard
    {
        private readonly ByteKnightEngine _botInstance;

        public Leaderboard(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                // Top 10, change as needed
                int leaderboardSize = 10;

                if (ByteKnightEngine._database == null)
                {
                    await slashCommand.RespondAsync("Database not initialized.");
                    return;
                }

                try
                {
                    var topUsers = await ByteKnightEngine.GetTopUsersWithNamesAsync(ByteKnightEngine._database, serverId, leaderboardSize);

                    if (topUsers == null || !topUsers.Any())
                    {
                        await slashCommand.RespondAsync("No top users found.");
                        return;
                    }

                    var embed = new EmbedBuilder
                    {
                        Title = "🔥 Server Leaderboard 🔥",
                        Color = Color.Gold,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                            IconUrl = "https://i.imgur.com/SejD45x.png"
                        },
                        Timestamp = DateTime.UtcNow,
                        ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/podium.png",
                    };

                    int rank = 1;
                    var mentions = new StringBuilder();

                    foreach (var user in topUsers)
                    {
                        mentions.AppendLine($"**#{rank}**. {user.Key.Mention}\nLevel: {user.Value.Level} | XP: {user.Value.XP}");
                        rank++;
                    }

                    embed.Description = mentions.ToString();
                    await slashCommand.RespondAsync(embed: embed.Build());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing leaderboard: {ex.Message}");
                    await slashCommand.RespondAsync("An error occurred while processing the leaderboard.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
