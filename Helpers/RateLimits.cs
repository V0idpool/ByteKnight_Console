using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.Helpers
{
    /// <summary>
    /// Provides utility methods for handling Discord API rate limits.
    /// Implements retry logic to ensure actions are retried after hitting rate limits.
    /// </summary>
    public static class RateLimits
    {
        /// <summary>
        /// Executes a specified asynchronous action with retry logic to handle Discord API rate limits.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <remarks>
        /// - If the action encounters a rate limit (`HttpException` with HTTP 429 status), it waits and retries.
        /// - Implements exponential backoff to handle repeated rate limit hits.
        /// - Retries up to a maximum number of attempts before giving up.
        /// </remarks>
        public static async Task RetryOnRateLimit(Func<Task> action)
        {
            int retryCount = 0;
            const int maxRetries = 3;
            int delay = 3500; // Initial delay

            while (retryCount < maxRetries)
            {
                try
                {
                    await action();
                    return; // Success, exit
                }
                catch (Discord.Net.HttpException ex) when (ex.HttpCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine($"Rate limit hit, retrying after delay ({retryCount + 1}/{maxRetries})...");
                    await Task.Delay(delay); // Wait before retrying
                    delay *= 2; // Exponential backoff
                    retryCount++;
                }
            }

            if (retryCount >= maxRetries)
            {
                Console.WriteLine($"Failed to update after {maxRetries} retries due to rate limits.");
            }
        }
    }
}
