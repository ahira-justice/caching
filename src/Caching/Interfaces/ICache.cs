namespace Caching.Interfaces
{
    public interface ICache
    {
        byte[] Get(string key);
        void Set(string key, byte[] value, int duration);
    }
}