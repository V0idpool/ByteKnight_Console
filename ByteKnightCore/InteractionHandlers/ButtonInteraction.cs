using ByteKnightConsole.MongoDBSchemas;
using Discord.WebSocket;
using Discord;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore.InteractionHandlers
{
    public static class ButtonInteraction
    {
        /// <summary>
        /// Handles various button interactions based on their custom IDs.
        /// </summary>
        /// <param name="component">The socket component representing the button interaction.</param>
        public static async Task HandleButtonInteraction(SocketMessageComponent component)
        {
            try
            {
                var customId = component.Data.CustomId;
                var serverId = (component.Channel as ITextChannel)?.Guild?.Id ?? 0;
                string userConfigs = Path.Combine(ByteKnightEngine.startupPath, ByteKnightEngine.userFile);
                if (!component.HasResponded)
                {
                    await component.DeferAsync(ephemeral: true);
                }
                if (customId.StartsWith("initiate_verify_button_"))
                {

                    var userId = ulong.Parse(customId.Replace("initiate_verify_button_", ""));
                    if (component.User.Id != userId)
                    {
                        await component.FollowupAsync("You cannot verify for another user.", ephemeral: true);
                        return;
                    }
                    var verificationEmbed = new EmbedBuilder
                    {
                        Title = $" Verification Required ??",
                        Description = "To gain access to this server, you need to verify your account first.\n\nClick **Verify** below to complete the process, \nor **Support** if you need assistance.",
                        Color = new Color(255, 69, 58), // A modern red color
                        ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/padlock.png", // Optional: add a small icon for visual interest
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Please complete the verification process to enjoy full access!",
                            IconUrl = "https://i.imgur.com/SejD45x.png"
                        },
                        Timestamp = DateTime.UtcNow
                    };

                    var verifyButton = new ButtonBuilder
                    {
                        Label = "? Verify",
                        CustomId = $"final_verify_button_{component.User.Id}",
                        Style = ButtonStyle.Success
                    };

                    var supportButton = new ButtonBuilder
                    {
                        Label = "? Support",
                        CustomId = $"support_button_{component.User.Id}",
                        Style = ButtonStyle.Primary
                    };

                    var componentBuilder = new ComponentBuilder()
                        .WithButton(verifyButton)
                        .WithButton(supportButton)
                        .Build();

                    await component.FollowupAsync(embed: verificationEmbed.Build(), components: componentBuilder, ephemeral: true);
                    return;
                }
                if (customId.StartsWith("final_verify_button_"))
                {
                    var userId = ulong.Parse(customId.Replace("final_verify_button_", ""));
                    if (component.User.Id != userId)
                    {
                        await component.FollowupAsync("You cannot verify for another user.", ephemeral: true);
                        return;
                    }

                    var guildUser = component.User as SocketGuildUser;
                    if (guildUser == null)
                    {
                        await component.FollowupAsync("An error occurred: Unable to find the guild user.", ephemeral: true);
                        return;
                    }

                    var serverSettings = await MongoDBDriver.Helpers.GetServerSettings(serverId); // Fetch server-specific settings

                    if (serverSettings == null)
                    {
                        await component.FollowupAsync("An error occurred: Server settings not found. Please contact support.", ephemeral: true);
                        return;
                    }
                    // Fetch the user's context from MongoDB
                    var userContext = await UserContextStore.GetAsync(serverId, guildUser.Id);
                    if (userContext != null && userContext.HasVerified)
                    {
                        // If the user has already verified, show a styled embed
                        var alreadyVerifiedEmbed = new EmbedBuilder()
                            .WithTitle("?Already Verified")
                            .WithDescription("You have already verified your account, You can only verify once.\nContact a Staff Member, and they will manually verify you.")
                            .WithColor(new Color(255, 69, 0)) // A modern orange color for warning
                            .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/refs/heads/main/Img/warning.png") // Optional: add an icon for locked state
                            .WithFooter(footer =>
                            {
                                footer.Text = "-|ByteKnight Discord Bot|-";
                                footer.IconUrl = "https://i.imgur.com/SejD45x.png";
                            })
                            .WithTimestamp(DateTime.UtcNow)
                            .Build();

                        await component.FollowupAsync(embed: alreadyVerifiedEmbed, ephemeral: true);
                        return;
                    }
                    // Retrieve the AutoRoleId from UserSettings, in ByteKnight Paid Tiers these settings can be configured via the easy to use GUI
                    ulong autoRoleId = ByteKnightEngine.verificationAutoRole;
                    if (ByteKnightEngine.verificationAutoRole != 0) // Check if AutoRoleId is set
                    {
                        await guildUser.AddRoleAsync(serverSettings.AutoRoleId);
                        var successEmbed = new EmbedBuilder()
                            .WithTitle($"? Verification Successful ??")
                            .WithDescription("Congratulations! You have successfully verified your account and now have full access to the server.\n\nEnjoy your stay and make the most of our community!")
                            .WithColor(new Color(0, 204, 102)) // A modern green color
                            .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/unlocked.png") // Optional: add a small icon for visual interest
                            .WithFooter(footer =>
                            {
                                footer.Text = "Thank you for verifying!";
                                footer.IconUrl = "https://i.imgur.com/SejD45x.png";
                            })
                            .WithTimestamp(DateTime.UtcNow)
                            .Build();

                        // Update the original ephemeral verification message
                        await component.FollowupAsync(embed: successEmbed, ephemeral: true);
                        Console.WriteLine("Sent success embed to user.");


                        if (userContext != null)
                        {
                            Console.WriteLine("User context found.");
                            var channel = guildUser.Guild.GetTextChannel(component.Channel.Id);
                            if (channel == null)
                            {
                                Console.WriteLine("Channel not found.");
                                return;
                            }

                            var originalMessage = await channel.GetMessageAsync(userContext.WelcomeMessageId) as IUserMessage;
                            if (originalMessage != null)
                            {
                                Console.WriteLine("Original message found. Modifying message to remove buttons.");
                                await originalMessage.ModifyAsync(msg =>
                                {
                                    var embed = originalMessage.Embeds.FirstOrDefault()?.ToEmbedBuilder().Build();
                                    msg.Embed = embed;
                                    msg.Components = new ComponentBuilder().Build(); // Remove all buttons
                                });

                                Console.WriteLine("Buttons removed from the original message.");

                                // Delete the ping message
                                var pingMessage = await channel.GetMessageAsync(userContext.PingMessageId) as IUserMessage;
                                if (pingMessage != null)
                                {
                                    await pingMessage.DeleteAsync();
                                    Console.WriteLine("Ping message deleted.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Original message not found.");
                            }
                            // Set HasVerified to true in MongoDB
                            userContext.HasVerified = true;
                            await UserContextStore.AddOrUpdateAsync(userContext); // Update the user context in MongoDB
                        }
                        else
                        {
                            Console.WriteLine("User context not found.");
                        }

                        return;
                    }
                    else
                    {
                        await component.FollowupAsync("An error occurred: AutoRole is not configured. Please contact support.", ephemeral: true);
                        return;
                    }
                }



                if (customId.StartsWith("support_button_"))
                {
                    var userId = ulong.Parse(customId.Replace("support_button_", ""));
                    if (component.User.Id != userId)
                    {
                        await component.FollowupAsync("You cannot request support for another user.", ephemeral: true);
                        return;
                    }

                    var guildUser = component.User as SocketGuildUser;
                    var owner = guildUser?.Guild.Owner;

                    if (owner != null)
                    {
                        var dmChannel = await owner.CreateDMChannelAsync();

                        var embed = new EmbedBuilder()
                            .WithTitle("?? Verification Support Request")
                            .WithDescription($"{guildUser.Mention} is having trouble with verification in your server \n**{guildUser.Guild.Name}**.\n\nPlease assist them in verifying their account.")
                            .WithColor(Color.LightOrange) // Orange color
                            .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/notice.png") // Replace with a relevant icon URL
                            .WithTimestamp(DateTimeOffset.Now)
                            .Build();

                        await dmChannel.SendMessageAsync(embed: embed);
                        await component.FollowupAsync("A support request has been sent to the server owner. Please wait for further assistance.", ephemeral: true);
                    }
                    else
                    {
                        await component.FollowupAsync("An error occurred: Unable to contact the server owner. Please try again later.", ephemeral: true);
                    }
                }
                if (component.Data.CustomId.StartsWith("toggle_steal_reminder"))
                {
                    var user = component.User;

                    if (user == null)
                    {
                        await component.RespondAsync("Could not identify the user.", ephemeral: true);
                        return;
                    }

                    var guildId = ((SocketGuildChannel)component.Channel).Guild.Id;

                    // Filter by both User ID and Server ID
                    var filter = Builders<UserLevelData>.Filter.And(
                        Builders<UserLevelData>.Filter.Eq(u => u.ID, user.Id),
                        Builders<UserLevelData>.Filter.Eq(u => u.ServerId, guildId)
                    );

                    var userLevel = await ByteKnightEngine._userLevelsCollection.Find(filter).FirstOrDefaultAsync();

                    bool stealReminderEnabled = userLevel?.StealReminder ?? false;
                    bool voteReminderEnabled = userLevel?.VoteReminder ?? false; // Ensure vote reminder state is fetched

                    // Toggle the StealReminder status
                    var updatedStealReminderStatus = !stealReminderEnabled;
                    var update = Builders<UserLevelData>.Update.Set(u => u.StealReminder, updatedStealReminderStatus);
                    await ByteKnightEngine._userLevelsCollection.UpdateOneAsync(filter, update);

                    // Prepare button states
                    var voteButtonStyle = voteReminderEnabled ? ButtonStyle.Success : ButtonStyle.Secondary;
                    var voteButtonLabel = voteReminderEnabled ? "Vote Reminder ✅" : "Vote Reminder ❌";
                    var stealButtonStyle = updatedStealReminderStatus ? ButtonStyle.Success : ButtonStyle.Secondary;
                    var stealButtonLabel = updatedStealReminderStatus ? "Steal Reminder ✅" : "Steal Reminder ❌";

                    var actionRow = new ComponentBuilder()
                        .WithButton(voteButtonLabel, "toggle_vote_reminder", voteButtonStyle)
                        .WithButton(stealButtonLabel, "toggle_steal_reminder", stealButtonStyle);

                    var embed = new EmbedBuilder()
                        .WithTitle("🔔 Reminder Settings")
                        .WithDescription("Enable or disable reminders. You'll receive notifications approximately 10 minutes before your selected events begin.")
                        .WithColor(Color.DarkGreen)
                        .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/notice.png")
                        .WithFooter(footer => footer.Text = "Click the buttons below to toggle reminders.")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();

                    // Modify the original response with updated embed and buttons
                    await component.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embed = embed;
                        msg.Components = actionRow.Build();
                    });
                }


                if (component.Data.CustomId.StartsWith("toggle_vote_reminder"))
                {
                    var user = component.User;

                    if (user == null)
                    {
                        await component.RespondAsync("Could not identify the user.", ephemeral: true);
                        return;
                    }

                    var guildId = ((SocketGuildChannel)component.Channel).Guild.Id;

                    // Filter by both User ID and Server ID
                    var filter = Builders<UserLevelData>.Filter.And(
                        Builders<UserLevelData>.Filter.Eq(u => u.ID, user.Id),
                        Builders<UserLevelData>.Filter.Eq(u => u.ServerId, guildId)
                    );

                    var userLevel = await ByteKnightEngine._userLevelsCollection.Find(filter).FirstOrDefaultAsync();

                    bool voteReminderEnabled = userLevel?.VoteReminder ?? false;
                    bool stealReminderEnabled = userLevel?.StealReminder ?? false; // Ensure steal reminder state is fetched

                    // Toggle the VoteReminder status
                    var updatedVoteReminderStatus = !voteReminderEnabled;
                    var update = Builders<UserLevelData>.Update.Set(u => u.VoteReminder, updatedVoteReminderStatus);
                    await ByteKnightEngine._userLevelsCollection.UpdateOneAsync(filter, update);

                    // Prepare button states
                    var voteButtonStyle = updatedVoteReminderStatus ? ButtonStyle.Success : ButtonStyle.Secondary;
                    var voteButtonLabel = updatedVoteReminderStatus ? "Vote Reminder ✅" : "Vote Reminder ❌";
                    var stealButtonStyle = stealReminderEnabled ? ButtonStyle.Success : ButtonStyle.Secondary;
                    var stealButtonLabel = stealReminderEnabled ? "Steal Reminder ✅" : "Steal Reminder ❌";

                    var actionRow = new ComponentBuilder()
                        .WithButton(voteButtonLabel, "toggle_vote_reminder", voteButtonStyle)
                        .WithButton(stealButtonLabel, "toggle_steal_reminder", stealButtonStyle);

                    var embed = new EmbedBuilder()
                        .WithTitle("🔔 Reminder Settings")
                        .WithDescription("Enable or disable reminders. You'll receive notifications approximately 10 minutes before your selected events begin.")
                        .WithColor(Color.DarkGreen)
                        .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/notice.png")
                        .WithFooter(footer => footer.Text = "Click the buttons below to toggle reminders.")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();

                    // Modify the original response with updated embed and buttons
                    await component.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embed = embed;
                        msg.Components = actionRow.Build();
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (if necessary) and send an error response
                Console.WriteLine($"Error handling button interaction: {ex.Message}");
                if (!component.HasResponded)
                {
                    await component.FollowupAsync("An unexpected error occurred. Please try again later.", ephemeral: true);
                }
            }
        }
    }
}
