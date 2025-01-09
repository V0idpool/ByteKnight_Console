using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Say
    {
        private readonly ByteKnightEngine _botInstance;

        public Say(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User as SocketGuildUser;

                if (user != null && user.GuildPermissions.Administrator)
                {
                    string messageContent = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "message")?.Value.ToString();

                    var embed = new EmbedBuilder
                    {
                        Description = $"{messageContent}",
                        Color = Color.DarkRed,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                            IconUrl = "https://i.imgur.com/SejD45x.png",
                        },
                    };

                    await slashCommand.RespondAsync("\u200B", ephemeral: true);
                    await slashCommand.Channel.SendMessageAsync(embed: embed.Build());
                    Console.WriteLine("Say Command Sent");
                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to use this command.", ephemeral: true);
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
