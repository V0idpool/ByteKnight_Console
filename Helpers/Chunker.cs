using System.Text;
using System.Text.RegularExpressions;

namespace ByteKnightConsole.Helpers
{
    public class Chunker
    {
        // Helper method to split text into chunks at sentence boundaries
        public static IEnumerable<string> SplitTextIntoChunks(string text, int maxChunkSize)
        {
            var sentences = Regex.Split(text, @"(?<=[.!?])\s*(?=[^a-zA-Z0-9])");
            var currentChunk = new StringBuilder();

            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length + 3 > maxChunkSize)
                {
                    yield return currentChunk.ToString().Trim() + "...";
                    currentChunk.Clear();
                }
                currentChunk.Append(sentence);
            }
            if (currentChunk.Length > 0)
            {
                yield return currentChunk.ToString().Trim();
            }
        }
    }
}
