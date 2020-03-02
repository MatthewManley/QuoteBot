using System.Collections.Concurrent;

namespace Infrastructure
{
    public class MemCache
    {
        public ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        public void Set<T>(string key, T value)
        {
            cache.AddOrUpdate(key, a => value, (a, b) => value);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var result = cache.TryGetValue(key, out object outputValue);
            value = (T)outputValue;
            return result;
        }

        public void Clear()
        {
            cache.Clear();
        }

        public void Remove(string key)
        {
            cache.TryRemove(key, out var value);
        }
    }
}