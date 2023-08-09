using System;
using System.Collections.Generic;
using System.Linq;
using Caching.Models;
using Caching.Options;

namespace Caching
{
    public class LfuCache : ICache
    {
        private readonly int _sizeLimit;
        private readonly Dictionary<string, CacheEntry> _cache;
        private readonly Dictionary<int, List<string>> _frequencyTable;

        private int CacheSize
        {
            get
            {
                return _cache.Values.Sum(entry => entry.Value.Length);
            }
        }

        public LfuCache(CacheOptions options)
        {
            _sizeLimit = options.SizeLimit;
            _cache = new Dictionary<string, CacheEntry>();
            _frequencyTable = new Dictionary<int, List<string>>();
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
            {
                value = entry.Value;
                UpdateFrequency(key);
            }
            else
                _cache.Remove(key);

            return value;
        }

        private void UpdateFrequency(string entry)
        {
            var entryFrequency = _frequencyTable.FirstOrDefault(
                pair => {
                    var entryFound = pair.Value.Contains(entry);
                    if (entryFound)
                        pair.Value.Remove(entry);

                    return entryFound; 
                }
                ).Key + 1;
            
            if (_frequencyTable.Keys.Contains(entryFrequency))
                _frequencyTable[entryFrequency].Add(entry);
            else
                _frequencyTable[entryFrequency] = new List<string> {entry};
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
        }

        private void InvalidateCache()
        {
            var leastFrequentlyUsed = _frequencyTable[_frequencyTable.Keys.Min()];

            foreach (var lfu in leastFrequentlyUsed)
                _cache.Remove(lfu);

            _frequencyTable.Remove(_frequencyTable.Keys.Min());
        }
    }
}