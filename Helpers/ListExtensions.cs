namespace ByteKnightConsole.Helpers
{
    using System.Collections.Generic;

    public static class ListExtensions
    {
        /// <summary>
        /// Breaks a list into smaller lists (batches) of the specified size.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="batchSize">The desired batch size.</param>
        /// <returns>An enumerable of list batches.</returns>
        public static IEnumerable<List<T>> Batch<T>(this List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
            }
        }
    }
}
