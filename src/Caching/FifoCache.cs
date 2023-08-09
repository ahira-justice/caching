using System;
using System.Collections.Generic;
using System.Linq;
using Caching.Models;
using Caching.Options;

namespace Caching
{
    public class FifoCache : ICache
    {
        private readonly int _sizeLimit;
        private readonly Dictionary<string, CacheEntry> _cache;
        private readonly Queue<string> _entryQueue;

        private int CacheSize
        {
            get
            {
                return _cache.Values.Sum(entry => entry.Value.Length);
            }
        }

        public FifoCache(CacheOptions options)
        {
            _sizeLimit = options.SizeLimit;
            _cache = new Dictionary<string, CacheEntry>();
            _entryQueue = new Queue<string>();
        }

        public byte[] Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            
            if (!_cache.ContainsKey(key))
                return null;
            
            byte[] value = null;
            var entry = _cache[key];

            if (!entry.ExpiresIn.HasValue || DateTime.UtcNow < entry.EntryTime + entry.ExpiresIn)
                value = entry.Value;
            else
                _cache.Remove(key);

            return value;
        }

        public void Set(string key, byte[] value)
        {
            Set(key, value, null);
        }

        public void Set(string key, byte[] value, int? ttl)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (ttl <= 0)
                throw new ArgumentNullException(nameof(ttl));

            if (CacheSize + value.Length > _sizeLimit)
                InvalidateCache();

            var entry = new CacheEntry
            {
                Value = value,
                EntryTime = DateTime.UtcNow,
                ExpiresIn = !ttl.HasValue ? (TimeSpan?)null : new TimeSpan(0, 0, ttl.Value)
            };

            _cache[key] = entry;
            _entryQueue.Enqueue(key);
        }

        private void InvalidateCache()
        {
            var fi = _entryQueue.Dequeue();
            _cache.Remove(fi);
        }
    }
}