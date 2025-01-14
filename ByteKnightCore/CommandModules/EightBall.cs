using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class EightBall
    {
        private readonly ByteKnightEngine _botInstance;

        public EightBall(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                string[] EightBallResponses = { "Yes", "No", "Maybe", "Ask again later", "What do you think?", "...", "Possibly", "No... I mean yes... Well... Ask again later.", "The answer is unclear... Seriously I double checked.", "I won't answer that.", "Yes... Sorry I wan't really listening", "I could tell you, but then I'd have to ban you", "Maybe... I don't know, could you repeat the question?", "You really think I'm answering THAT?", "Yes No Yes No Yes No.", "Ask yourself the same question in the mirror three times, the answer will become clear.", "Noooope" };
                Random rand = new Random();

                var questionOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "question");

                if (questionOption != null && questionOption.Value is string question)
                {
                    int randomEightBallMessage = rand.Next(EightBallResponses.Length);
                    string messageToPost = EightBallResponses[randomEightBallMessage];

                    var embed = new EmbedBuilder
                    {
                        Title = "🎱  8-Ball Answer  🎱",
                        Description = $"**Question:** {question}\n\n**Answer:** {messageToPost}",
                        ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/8ball.png",
                        Color = Color.DarkRed,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                            IconUrl = "https://i.imgur.com/SejD45x.png",
                        },
                    };

                    await slashCommand.RespondAsync(embed: embed.Build());
                    Console.WriteLine("8ball Response sent");
                }
                else
                {
                    await slashCommand.RespondAsync("```" + "Please ask a question after `/8ball`." + "```");
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
