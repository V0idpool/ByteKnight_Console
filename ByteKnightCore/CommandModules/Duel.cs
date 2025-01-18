using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Duel
    {
        private readonly ByteKnightEngine _botInstance;

        public Duel(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }
        public int RollDice()
        {
            return new Random().Next(1, 7);
        }
        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var challengedUser = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user")?.Value as IUser;

                if (challengedUser != null)
                {
                    IUser challenger = slashCommand.User;
                    int challengerRoll = RollDice();
                    int challengedRoll = RollDice();
                    IUser winner = (challengerRoll > challengedRoll) ? challenger : challengedUser;

                    var embed = new EmbedBuilder
                    {
                        Title = "⚔️  Duel Results  ⚔️",
                        Description = $"**{MentionUtils.MentionUser(challenger.Id)} challenges {MentionUtils.MentionUser(challengedUser.Id)} to a duel!**",
                        Color = Color.DarkRed,
                        ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/duel.png",
                        Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = $"{challenger.GlobalName ?? challenger.Username}'s Roll",
                    Value = $"**{challengerRoll}**",
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = $"{challengedUser.GlobalName ?? challengedUser.Username}'s Roll",
                    Value = $"**{challengedRoll}**",
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "__**Winner**__",
                    Value = $"{MentionUtils.MentionUser(winner.Id)}",
                    IsInline = false
                }
            },
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                            IconUrl = "https://i.imgur.com/SejD45x.png",
                        },
                    };

                    await slashCommand.RespondAsync(embed: embed.Build());
                    Console.WriteLine("Duel response sent");
                }
                else
                {
                    await slashCommand.RespondAsync("Please mention a user to challenge to a duel.");
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
