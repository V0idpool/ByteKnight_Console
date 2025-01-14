using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class GoogleIt
    {
        private readonly ByteKnightEngine _botInstance;

        public GoogleIt(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                string searchTerms = slashCommand.Data.Options.First().Value.ToString();
                string encodedSearchTerms = Uri.EscapeDataString(searchTerms);
                string lmgtfyLink = $"https://www.google.com/search?q={encodedSearchTerms}";

                var embed = new EmbedBuilder
                {
                    Title = "🔎  Google It  🔎",
                    Description = $"Searched For: **{searchTerms}**",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    },
                    Timestamp = DateTime.UtcNow,
                    ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/search.png",
                };

                embed.AddField("\u200B", $"➡️ [[Click Here for the Search]]({lmgtfyLink})");

                await slashCommand.RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
