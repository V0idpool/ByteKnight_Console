using ByteKnightConsole.MongoDBSchemas;
using Discord;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.Helpers
{
    /// <summary>
    /// Provides methods for interacting with Top.gg votes.
    /// </summary>
    public class TOPGGVotes
    {

        /// <summary>
        /// Checks if a user has voted for the bot on Top.gg.
        /// </summary>
        /// <param name="botId">The bot's Top.gg ID.</param>
        /// <param name="topGGToken">The Top.gg API token.</param>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>True if the user has voted; otherwise, false.</returns>
        public static async Task<bool> CheckIfUserVotedOnTopGG(string botId, string topGGToken, ulong userId)
        {
            var url = $"https://top.gg/api/bots/{botId}/check?userId={userId}";
            using (var httpClient = new HttpClient())
            {
                // Set authorization header with your top.gg API token
                httpClient.DefaultRequestHeaders.Add("Authorization", topGGToken);

                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var voteResponse = JsonConvert.DeserializeObject<TopGGVoteResponse>(content);
                        return voteResponse?.Voted == 1;
                    }
                    else
                    {
                        Console.WriteLine($"Error: Unable to check vote status on top.gg. Status Code: {response.StatusCode}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while checking vote status: {ex.Message}");
                    return false;
                }
            }
        }
        /// <summary>
        /// Starts a periodic check for Top.gg votes every 15 minutes.
        /// </summary>
        public static async Task StartPeriodicVoteCheck()
        {
            // Initialize the timer to trigger every 15 minutes
            ByteKnightEngine._voteCheckTimer = new System.Threading.Timer(async _ =>
            {
                await CheckAndRewardVotes(); // Ensure that only one CheckAndRewardVotes() is running
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

            Console.Write("Vote Check Started...");
        }
        /// <summary>
        /// Checks if users have voted on Top.gg and rewards them if eligible.
        /// </summary>
        private static async Task CheckAndRewardVotes()
        {
            // Check if any other instance of CheckAndRewardVotes is currently running
            if (await ByteKnightEngine._voteCheckSemaphore.WaitAsync(0))
            {
                try
                {
                    // Loop through all guilds the bot is connected to
                    foreach (var guild in ByteKnightEngine._client.Guilds)
                    {
                        ulong serverId = guild.Id;

                        // Only retrieve users with the current guild's ServerId
                        var filter = Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId);
                        var users = await ByteKnightEngine._userLevelsCollection.Find(filter).ToListAsync();

                        // Your Top.gg bot ID and API token
                        var topGGToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEzMTUwNDYzODAzOTMzMzY5MjQiLCJib3QiOnRydWUsImlhdCI6MTczNDk4MDI5Mn0.G1lLeY_dpE1ai53nOoGHxWArygiOT2-eDoP2KK-4Eus"; // Replace with your actual token
                        var botId = "1315046380393336924"; // Replace with your bot's ID

                        foreach (var user in users)
                        {
                            // Add a delay to space out requests
                            await Task.Delay(2000); // 500 ms delay between each request; adjust as needed
                            bool hasVoted = await CheckIfUserVotedOnTopGG(botId, topGGToken, user.ID);
                            DateTime? lastVoteTime = user.LastVoteRewardTime;
                            // Check if 12 hours have passed since the last reward
                            bool eligibleForReward = lastVoteTime == null ||
                                              (DateTime.UtcNow - lastVoteTime.Value).TotalHours >= 12;


                            // Reward if they have voted and haven't been rewarded in the last 12 hours
                            if (hasVoted && eligibleForReward)
                            {
                                var filteruser = Builders<UserLevelData>.Filter.And(
                                    Builders<UserLevelData>.Filter.Eq(nameof(UserLevelData.ID), user.ID),
                                    Builders<UserLevelData>.Filter.Eq(nameof(UserLevelData.ServerId), serverId)
                                );
                                var update = Builders<UserLevelData>.Update
                                    .Inc(u => u.XP, 100) // Reward XP
                                    .Set(u => u.LastVoteRewardTime, DateTime.UtcNow); // Update the reward time

                                // Execute the update operation
                                var updateResult = await ByteKnightEngine._userLevelsCollection.UpdateOneAsync(filteruser, update);

                                // Confirm the update was successful before logging or proceeding
                                if (updateResult.ModifiedCount > 0)
                                {
                                    Console.WriteLine($"User {user.ID} in guild {serverId} has been rewarded 100 XP for voting.");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to update LastVoteRewardTime for user {user.ID} in guild {serverId}. Skipping XP award.");
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in CheckAndRewardVotes: {ex.Message}");
                }
                finally
                {
                    // Release the semaphore to allow future executions
                    ByteKnightEngine._voteCheckSemaphore.Release();
                }
            }
            else
            {
                Console.WriteLine("CheckAndRewardVotes is already running, skipping this iteration.");
            }
        }
    }
}
