using System;

namespace Caching.Exceptions
{
    public class CacheSizeLimitExceededException : Exception
    {
        public CacheSizeLimitExceededException() : base("Cannot insert into cache, cache limit exceeded")
        {

        }
    }
}