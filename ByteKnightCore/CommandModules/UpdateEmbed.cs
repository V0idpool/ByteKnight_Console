using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class UpdateEmbed
    {
        private readonly ByteKnightEngine _botInstance;

        public UpdateEmbed(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User as SocketGuildUser;

                // Check if the user is an admin or has KickMembers permissions
                var isAdmin = user?.GuildPermissions.Administrator ?? false;
                var hasKickPerm = user?.GuildPermissions.KickMembers ?? false;
                var isBot = user?.IsBot ?? false;

                if (isAdmin || hasKickPerm || isBot)
                {
                    // Get the title and message content from the slash command options
                    var titleOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "title")?.Value as string;
                    var messageContentOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "message")?.Value as string;

                    if (string.IsNullOrEmpty(titleOption) || string.IsNullOrEmpty(messageContentOption))
                    {
                        await slashCommand.RespondAsync("You must provide both a title and a message.", ephemeral: true);
                        return;
                    }

                    // Replace \n with actual newline characters
                    string messageContent = messageContentOption.Replace(@"\n", Environment.NewLine);

                    string formattedDateTime = DateTime.Now.ToString("MMMM dd yyyy" + Environment.NewLine + "'Time:' h:mm tt");

                    // Create an embed response with a custom color
                    var embed = new EmbedBuilder
                    {
                        Title = titleOption, // Use the title option here
                        Description = $"Date: {formattedDateTime}\n\n{messageContent}",
                        Color = Color.DarkRed,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "\u200B\n┤|ByteKnight Discord Bot|├\nhttps://voidbot.lol/",
                            IconUrl = "https://i.imgur.com/SejD45x.png",
                        },
                        Timestamp = DateTime.UtcNow,
                        ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/update.png",
                    };

                    await slashCommand.RespondAsync(embed: embed.Build());

                    Console.WriteLine("Server Update Message sent");
                }
                else
                {
                    // Inform the user that they don't have the necessary permission
                    await slashCommand.RespondAsync("You don't have permission to use this command. Only admins, moderators (Kick permission) can use this.", ephemeral: true);
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
