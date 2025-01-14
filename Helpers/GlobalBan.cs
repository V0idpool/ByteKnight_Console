using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.Helpers
{
    public static class GlobalBan
    {
        // Global Ban System (Not used) Left this as a basic example
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
        // Global Ban System (Not used) Left this as a basic example
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
