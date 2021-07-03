using System;
using System.Collections.Generic;
using Caching.Exceptions;
using Caching.Interfaces;
using Caching.Models;
using Caching.Options;

namespace Caching
{
    public class Cache : ICache
    {
        private readonly int _sizeLimit;
        private readonly Dictionary<string, CacheEntry> _cache;

        public int CacheSize
        {
            get
            {
                int size = 0;

                foreach (var entry in _cache.Values)
                {
                    size += entry.Value.Length;
                }

                return size;
            }
        }

        public Cache(CacheOptions options)
        {
            _sizeLimit = options.SizeLimit;
            _cache = new Dictionary<string, CacheEntry>();
        }

        public byte[] Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            byte[] value = null;

            if (_cache.ContainsKey(key))
            {
                var entry = _cache[key];

                if (DateTime.UtcNow < entry.EntryTime + entry.Expiry)
                    value = entry.Value;
                else
                    _cache.Remove(key);
            }

            return value;
        }

        public void Set(string key, byte[] value, int duration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (duration <= 0)
                throw new ArgumentNullException(nameof(duration));

            if (CacheSize + value.Length > _sizeLimit)
                throw new CacheSizeLimitExceededException();

            var entry = new CacheEntry
            {
                Value = value,
                EntryTime = DateTime.UtcNow,
                Expiry = new TimeSpan(0, 0, duration)
            };

            _cache[key] = entry;
        }
    }
}