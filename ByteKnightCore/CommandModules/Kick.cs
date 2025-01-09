using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Kick
    {
        private readonly ByteKnightEngine _botInstance;

        public Kick(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var author = slashCommand.User as SocketGuildUser;

                if (author.GuildPermissions.KickMembers)
                {
                    var mention = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user")?.Value as SocketUser;

                    if (mention is SocketGuildUser userToKick)
                    {
                        if (userToKick.IsBot)
                        {
                            await slashCommand.RespondAsync("You cannot kick a bot.");
                            return;
                        }

                        await userToKick.KickAsync();

                        var embed = new EmbedBuilder
                        {
                            Title = "🦵  User Kicked  🦵",
                            Description = $"{author.Mention} kicked {userToKick.GlobalName ?? userToKick.Username} from the server.",
                            Color = Color.Red,

                            Footer = new EmbedFooterBuilder
                            {
                                Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                                IconUrl = "https://i.imgur.com/SejD45x.png",
                            },
                            Timestamp = DateTime.UtcNow
                        };

                        await slashCommand.RespondAsync(embed: embed.Build());

                        Console.WriteLine($"{author.GlobalName ?? author.Username} kicked {userToKick.GlobalName ?? userToKick.Username}#{userToKick.Discriminator} from the server.");
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Please mention the user you want to kick.");
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to kick members.", ephemeral: true);
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
