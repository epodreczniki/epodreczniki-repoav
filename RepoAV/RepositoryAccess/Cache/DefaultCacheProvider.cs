using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace PSNC.RepoAV.Services.RepositoryAccess.Cache
{
    public class DefaultCacheProvider : ICacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public object Get(string key)
        {
            return Cache[key];
        }

        public void Set(string key, object data, int cacheTime)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime);

            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(string key)
        {
            return (Cache[key] != null);
        }

        public void Invalidate(string key)
        {
            Cache.Remove(key);
        }

        public void InvalidateSartsWith(string keypattern)
        {
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<String, Object> kvp in Cache)
            {
                if (kvp.Key.StartsWith(keypattern))
                    toRemove.Add(kvp.Key);
            }
            foreach (string key in toRemove)
                Cache.Remove(key);
        }
    }
}
