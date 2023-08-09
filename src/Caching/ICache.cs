namespace Caching
{
    public interface ICache
    {
        byte[] Get(string key);
        void Set(string key, byte[] value);
        void Set(string key, byte[] value, int? ttl);

    }
}