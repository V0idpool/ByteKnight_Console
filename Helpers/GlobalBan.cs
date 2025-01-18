using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.Helpers
{
    public static class GlobalBan
    {
        /// <summary>
        /// Removes an entry corresponding to the given username and user ID from the global ban list file.
        /// </summary>
        /// <param name="username">The username to remove from the ban list.</param>
        /// <param name="userId">The user ID to remove from the ban list.</param>
        public static void RemoveFromGlobalBans(string username, string userId)
        {
            try
            {
                string filePath = "bans_global.txt";
                var lines = File.ReadAllLines(filePath).ToList();
                string entryToRemove = $"Username: {username} | UserID: {userId}";
                lines.Remove(entryToRemove);

                File.WriteAllLines(filePath, lines);
                Console.WriteLine($"Entry removed from {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing entry from bans_global.txt: {ex.Message}");
            }
        }
        /// <summary>
        /// Saves the provided content to the global ban list file if it does not already exist.
        /// </summary>
        /// <param name="content">The content to save to the ban list.</param>
        public static void SaveGlobalBanList(string content)
        {
            try
            {
                string filePath = "bans_global.txt";
                if (!File.Exists(filePath) || !File.ReadLines(filePath).Contains(content))
                {
                    System.IO.File.AppendAllText(filePath, content + Environment.NewLine);
                    Console.WriteLine($"Content saved to {filePath}");
                }
                else
                {
                    Console.WriteLine($"Content already exists in {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to text file: {ex.Message}");
            }
        }
    }
}
