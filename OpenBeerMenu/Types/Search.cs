namespace OpenBeerMenu.Types
{
    public static class Search
    {
        public static IEnumerable<T> Rank<T>(string query, IEnumerable<T> items, Func<T, string> selector) 
            => items.ToDictionary(x => x, x => Distance(selector(x), query)).Where(x => x.Value <= 5 || selector(x.Key).Contains(query, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key);

        public static int Distance(string source, string query) 
        {
            // Special cases
            if (source == query) return 0;
            if (source.Length == 0) return query.Length;
            if (query.Length == 0) return source.Length;
            // Initialize the distance matrix
            var distance = new int[source.Length + 1, query.Length + 1];
            for (var i = 0; i <= source.Length; i++) distance[i, 0] = i;
            for (var j = 0; j <= query.Length; j++) distance[0, j] = j;
            // Calculate the distance
            for (var i = 1; i <= source.Length; i++) {
                for (var j = 1; j <= query.Length; j++) {
                    var cost = (source[i - 1] == query[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            // Return the distance
            return distance[source.Length, query.Length];
        }
    }
}