using System.Text;
using System.Text.RegularExpressions;

namespace ByteKnightConsole.Helpers
{
    public class Chunker
    {
        /// <summary>
        /// Splits the given text into chunks that do not exceed the specified maximum size, 
        /// attempting to split at sentence boundaries.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="maxChunkSize">The maximum size for each chunk.</param>
        /// <returns>An enumerable of text chunks.</returns>
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
