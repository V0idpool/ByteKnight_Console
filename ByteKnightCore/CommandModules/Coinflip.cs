using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Coinflip
    {
        private readonly ByteKnightEngine _botInstance;

        public Coinflip(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                Random random = new Random();
                bool isHeads = random.Next(2) == 0;
                string result = isHeads ? "Heads" : "Tails";
                var embed = new EmbedBuilder
                {
                    Title = "🪙  Coin Flip Result  🪙",
                    Description = $"The coin landed on: **{result}**",
                    Color = Color.DarkRed,
                };
                await slashCommand.RespondAsync(embed: embed.Build());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling setup command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
