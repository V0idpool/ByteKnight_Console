using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Ban
    {
        private readonly ByteKnightEngine _botInstance;

        public Ban(ByteKnightEngine botInstance)
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
                    var mentionBan = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user")?.Value as SocketUser;

                    if (mentionBan is SocketGuildUser userToBan)
                    {

                        string reasonBan = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "reason")?.Value as string ?? "No reason specified";
                        await userToBan.BanAsync(reason: reasonBan);

                        var embedBan = new EmbedBuilder
                        {
                            Title = "🔨  BAN Hammer  🔨",
                            Description = $"{authorBan.Mention} banned {userToBan.GlobalName ?? userToBan.Username} from the server.",
                            Color = Color.Red,

                            Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reasonBan,
                            IsInline = false
                        }
                    },
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                                IconUrl = "https://i.imgur.com/SejD45x.png",
                            },
                            Timestamp = DateTime.UtcNow
                        };

                        await slashCommand.RespondAsync(embed: embedBan.Build());

                        Console.WriteLine($"{authorBan.GlobalName ?? authorBan.Username} banned {userToBan.GlobalName ?? userToBan.Username}#{userToBan.Discriminator} from the server. Reason: {reasonBan}");
                    }

                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to ban members.", ephemeral: true);
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
