using Discord;
using Discord.WebSocket;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class PM
    {
        private readonly ByteKnightEngine _botInstance;

        public PM(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var isAdmin = (slashCommand.User as IGuildUser)?.GuildPermissions.Administrator ?? false;

                if (isAdmin)
                {
                    var mentionedUserOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "user");
                    var mentionedUser = mentionedUserOption?.Value as IUser;

                    if (mentionedUser != null)
                    {
                        var messageContentOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "message");
                        var messageContent = messageContentOption?.Value?.ToString();

                        try
                        {
                            await mentionedUser.SendMessageAsync(messageContent ?? "No message content provided.");
                            await slashCommand.RespondAsync("PM successfully sent", ephemeral: true);
                            Console.WriteLine("PM Command successfully sent");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("PM Command Error: " + ex.Message);
                            await slashCommand.RespondAsync("Failed to send a private message. User may not allow PMS.", ephemeral: true);
                        }
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Please mention a user to send a PM.", ephemeral: true);
                    }
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
