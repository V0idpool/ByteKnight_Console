using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Steal
    {
        private readonly ByteKnightEngine _botInstance;

        public Steal(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                await slashCommand.DeferAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var userId = slashCommand.User.Id;
                        var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;

                        var userLevel = await MongoDBDriver.Helpers.LoadUserLevel(userId, serverId) ?? new UserLevelData
                        {
                            Id = ObjectId.GenerateNewId(),
                            ID = userId,
                            ServerId = serverId
                        };

                        // Check if the command is on cooldown
                        if (userLevel.LastStealTime.HasValue &&
                            DateTime.UtcNow - userLevel.LastStealTime.Value < TimeSpan.FromHours(6))
                        {
                            var timeRemaining = TimeSpan.FromHours(6) - (DateTime.UtcNow - userLevel.LastStealTime.Value);

                            var cooldownEmbed = new EmbedBuilder()
                                .WithTitle("You already stole from someone!")
                                .WithDescription($"You can't steal from anyone yet!\nYou already commited a crime.")
                                .WithThumbnailUrl("https://voidbot.lol/img/anti-theft-system.png")
                                .AddField("Time Remaining", $"{timeRemaining.Hours}h {timeRemaining.Minutes}m", true)
                                .WithColor(Color.DarkRed)
                                .WithCurrentTimestamp()
                                .Build();

                            await slashCommand.FollowupAsync(embed: cooldownEmbed);
                            return;
                        }

                        var guild = ((SocketGuildChannel)slashCommand.Channel).Guild;
                        var randomUser = guild.Users
                            .Where(u => u.Id != userId && !u.IsBot)
                            .OrderBy(_ => Guid.NewGuid())
                            .FirstOrDefault();

                        if (randomUser == null)
                        {
                            await slashCommand.FollowupAsync("No eligible users to steal from.");
                            return;
                        }

                        var targetLevel = await MongoDBDriver.Helpers.LoadUserLevel(randomUser.Id, serverId) ?? new UserLevelData
                        {
                            Id = ObjectId.GenerateNewId(),
                            ID = randomUser.Id,
                            ServerId = serverId
                        };

                        if (targetLevel.XP <= 0)
                        {
                            await slashCommand.FollowupAsync($"{randomUser.Mention} has no XP to steal, try again.");
                            return;
                        }

                        // Calculate random amount to steal
                        var stealAmount = new Random().Next(1, Math.Min(500, targetLevel.XP));

                        // Adjust XP values
                        targetLevel.XP -= stealAmount;
                        userLevel.XP += stealAmount;

                        // Update statistics
                        userLevel.StealCount += 1;
                        userLevel.StealTotal += stealAmount;
                        targetLevel.StolenFromCount += 1;
                        targetLevel.StolenTotal += stealAmount;

                        // Update LastStealTime
                        userLevel.LastStealTime = DateTime.UtcNow;

                        // Save to database
                        await MongoDBDriver.Helpers.SaveUserLevel(targetLevel);
                        await MongoDBDriver.Helpers.SaveUserLevel(userLevel);

                        // Create embed
                        var embed = new EmbedBuilder()
                            .WithTitle($"{slashCommand.User.GlobalName ?? slashCommand.User.Username} just robbed {randomUser.GlobalName ?? randomUser.Username}!")
                            .WithDescription($"{slashCommand.User.Mention} stole {stealAmount} XP from {randomUser.Mention}.")
                             .WithThumbnailUrl("https://static-00.iconduck.com/assets.00/robber-icon-2048x2048-wl3dzu8i.png")
                            .AddField("Your XP", userLevel.XP, false)
                            .AddField($"Victims XP", targetLevel.XP, false)
                            .WithColor(Color.DarkRed)
                            .WithCurrentTimestamp();

                        await slashCommand.FollowupAsync(embed: embed.Build());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing steal command: {ex.Message}");
                        await slashCommand.FollowupAsync("An error occurred while processing the steal command.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
