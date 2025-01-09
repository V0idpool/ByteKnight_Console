using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class WarnInfo
    {
        private readonly ByteKnightEngine _botInstance;
        // Define the maximum number of warnings per page here
        const int warningsPerPage = 5;
        public WarnInfo(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var authorBan = slashCommand.User as SocketGuildUser;

                if (authorBan.GuildPermissions.Administrator || authorBan.GuildPermissions.BanMembers)
                {
                    var usersIdOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "user");
                    ulong usersId = usersIdOption?.Value is SocketUser user ? user.Id : 0;

                    if (usersId == 0)
                    {
                        await slashCommand.RespondAsync("Please provide a valid user.");
                        return;
                    }
                    ulong serverId = (slashCommand.Channel as ITextChannel)?.Guild?.Id ?? 0;

                    var warnings = await Warn.GetWarnings(usersId, serverId);
                    if (warnings.Count == 0)
                    {
                        await slashCommand.RespondAsync("This user has no warnings.", ephemeral: true);
                        return;
                    }

                    int totalPages = (int)Math.Ceiling(warnings.Count / (double)warningsPerPage);
                    int currentPage = 1;

                    var embed = BuildWarningsEmbed(warnings, currentPage, totalPages, usersId);
                    var components = BuildPaginationComponents(usersId, serverId, currentPage, totalPages);

                    await slashCommand.RespondAsync(embed: embed, components: components, ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
        // Build the warnings embed
        public Embed BuildWarningsEmbed(List<Warning> warnings, int currentPage, int totalPages, ulong userId)
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = $"⚠️ {ByteKnightEngine._client.GetUser(userId)?.GlobalName ?? "User"}'s Warnings",
                ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/crisis.png",
                Description = $"**‼️ Total Warnings:** {warnings.Count}",
                Color = Color.Orange,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Page {currentPage}/{totalPages}\n┤|ByteKnight Discord Bot|├",
                    IconUrl = "https://i.imgur.com/SejD45x.png",
                },
                Timestamp = DateTime.UtcNow
            };

            var paginatedWarnings = warnings
                .Skip((currentPage - 1) * warningsPerPage)
                .Take(warningsPerPage)
                .ToList();

            foreach (var (warning, index) in paginatedWarnings.Select((w, i) => (w, i)))
            {
                var issuer = ByteKnightEngine._client.GetUser(warning.IssuerId);

                embedBuilder.AddField(
                     "═══════════════════════════",
                    $"📢 **Warning ID: {index + 1 + (currentPage - 1) * warningsPerPage}**",

                    false
                );

                embedBuilder.AddField(
                    "Reason",
                    warning.Reason,
                    false
                );

                embedBuilder.AddField(
                    "Issued By",
                    issuer != null ? $"<@{issuer.Id}>" : "Unknown",
                    true
                );
                embedBuilder.AddField(
                    "Date",
                    $"{warning.Date:MMMM dd, yyyy}",
                    true
                );
                embedBuilder.AddField(
                          "═══════════════════════════",
                          $"\u200B",
                          false
                      );
            }

            return embedBuilder.Build();
        }
        // Method to build the pagination components/buttons
        public MessageComponent BuildPaginationComponents(ulong userId, ulong serverId, int currentPage, int totalPages)
        {
            var buttons = new ComponentBuilder();
            buttons.WithButton("Previous", customId: $"warn_page_{userId}_{serverId}_{currentPage - 1}", ButtonStyle.Secondary, disabled: currentPage == 1);
            buttons.WithButton("Next", customId: $"warn_page_{userId}_{serverId}_{currentPage + 1}", ButtonStyle.Secondary, disabled: currentPage == totalPages);
            return buttons.Build();
        }

        public static async Task UnsetXpForNextLevel()
        {
            var userLevelsCollection = ByteKnightEngine._database.GetCollection<BsonDocument>(ByteKnightEngine._userLevels);

            var update = Builders<BsonDocument>.Update.Unset("xpForNextLevel");

            var result = await userLevelsCollection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, update);

            Console.WriteLine($"{result.ModifiedCount} documents updated.");
        }
    }
}
