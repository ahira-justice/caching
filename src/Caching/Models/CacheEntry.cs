using System;

namespace Caching.Models
{
    public class CacheEntry
    {
        public byte[] Value { get; set; }
        public DateTime EntryTime { get; set; }
        public TimeSpan Expiry { get; set; }
    }
}