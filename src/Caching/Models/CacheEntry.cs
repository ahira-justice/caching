using System;

namespace Caching.Models
{
    public struct CacheEntry
    {
        public byte[] Value { get; set; }
        public DateTime EntryTime { get; set; }
        public TimeSpan? ExpiresIn { get; set; }
    }
}