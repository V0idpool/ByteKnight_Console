using ByteKnightConsole.MongoDBSchemas;
using ByteKnightConsole;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class TheftStats
    {
        private readonly ByteKnightEngine _botInstance;

        public TheftStats(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            await slashCommand.DeferAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    // Get the target user or default to the command invoker
                    var targetUser = (SocketGuildUser)(slashCommand.Data.Options?.FirstOrDefault()?.Value ?? slashCommand.User);
                    var userId = targetUser.Id;
                    var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;

                    // Load user level data from the database
                    var userLevel = await ByteKnightConsole.MongoDBDriver.Helpers.LoadUserLevel(userId, serverId) ?? new UserLevelData
                    {
                        Id = ObjectId.GenerateNewId(),
                        ID = userId,
                        ServerId = serverId
                    };
                    var Footer = new EmbedFooterBuilder
                    {
                        Text = "┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    };
                    // Create styled embed
                    var embed = new EmbedBuilder()
                        .WithTitle($"{targetUser.GlobalName ?? targetUser.Username}'s Theft Stats 🥷")
                        .WithDescription("These are the **Monthly** Theft Stats.\n**This resets on the 1st of each month**")
                        .WithThumbnailUrl("https://voidbot.lol/img/xp.png") // Optional: Replace with a stats-themed image URL
                        .AddField("📈 Gains", "** **", false) // Heading for the section
                        .AddField("Total Heists", userLevel.StealCount, true)
                        .AddField("Total Stolen", userLevel.StealTotal + " XP", true)
                        // Losses Section
                        .AddField("📉 Losses", "** **", false) // Heading for the section
                        .AddField("Times Robbed", userLevel.StolenFromCount, true)
                        .AddField("Total Lost", userLevel.StolenTotal + " XP", true)
                        .WithColor(Color.Gold) // Gold for a rich "theft" theme
                        .WithFooter(Footer)
                        .WithCurrentTimestamp();

                    // Send the embed as a response
                    await slashCommand.FollowupAsync(embed: embed.Build());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing theftstats command: {ex.Message}");
                    await slashCommand.FollowupAsync("An error occurred while fetching the theft stats.");
                }
            });
        }
    }
}
