using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Slap
    {
        private readonly ByteKnightEngine _botInstance;

        public Slap(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var userOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "user");
                ulong userId = userOption?.Value is SocketUser targetUser ? targetUser.Id : 0;

                if (userId == 0)
                {
                    await slashCommand.RespondAsync("Please provide a valid user.");
                    return;
                }

                var author = slashCommand.User;
                var target = ByteKnightEngine._client.GetUser(userId);

                var responseMessage = $"{author.Mention} just slapped TF out of {target.Mention}!";

                // Create the embed
                var embedBuilder = new EmbedBuilder
                {
                    Title = "👋👋👋👋👋👋",
                    Color = Color.DarkRed,
                    Description = responseMessage,
                    ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/slap.png",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "\u200B\n┤|ByteKnight Discord Bot|├\nhttps://voidbot.lol/",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    },

                };

                var embed = embedBuilder.Build();

                await slashCommand.RespondAsync(embed: embed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
