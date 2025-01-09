using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Roll
    {
        private readonly ByteKnightEngine _botInstance;

        public Roll(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                // Generate a random number between 1 and 6

                var result = new Random().Next(1, 13);
                var embed = new EmbedBuilder
                {
                    Title = "🎲  Dice Roll  🎲",
                    Description = $"\n{slashCommand.User.Mention} rolled the dice and got: **{result}**",
                    Color = Color.DarkRed,
                    ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/dice.png",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png",
                    },
                };
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
