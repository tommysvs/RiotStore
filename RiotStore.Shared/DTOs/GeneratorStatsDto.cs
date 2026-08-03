using System.Collections.Generic;

namespace RiotStore.Shared.Dtos
{
    public class GeneratorStatsDto
    {
        public int TotalAttempts { get; set; }

        public Dictionary<string, int> AttemptsByCategory { get; set; } = new();

        public Dictionary<string, int> AttemptsBySegment { get; set; } = new();

        public int TotalQuantityRequested { get; set; }

        public int RetryAttempts { get; set; }

        public double ElapsedSeconds { get; set; }
    }
}