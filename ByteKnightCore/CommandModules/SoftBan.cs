using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class SoftBan
    {
        private readonly ByteKnightEngine _botInstance;

        public SoftBan(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var author = slashCommand.User as SocketGuildUser;

                if (author.GuildPermissions.Administrator || author.GuildPermissions.KickMembers)
                {
                    var mention = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user")?.Value as SocketUser;

                    if (mention is SocketGuildUser userToSoftBan)
                    {

                        string reason = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "reason")?.Value as string ?? "No reason specified";
                        await userToSoftBan.Guild.AddBanAsync(userToSoftBan, 1, reason);
                        await userToSoftBan.Guild.RemoveBanAsync(userToSoftBan);

                        var embed = new EmbedBuilder
                        {
                            Title = "🤏  User Softbanned  🤏",
                            Description = $"{author.Mention} softbanned {userToSoftBan.GlobalName ?? userToSoftBan.Username}.",
                            Color = Color.DarkOrange,

                            Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reason,
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

                        await slashCommand.RespondAsync(embed: embed.Build());

                        Console.WriteLine($"{author.GlobalName ?? author.Username} softbanned {userToSoftBan.GlobalName ?? userToSoftBan.Username}#{userToSoftBan.Discriminator} from the server. Reason: {reason}");
                    }

                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to soft ban members.", ephemeral: true);
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
