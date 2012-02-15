namespace SassAndCoffee.Core {
    using System.Collections.Generic;

    public class CacheDependency {
        public string PhysicalPath { get; set; }
        public HashSet<CacheItem> Produces { get; private set; }

        public CacheDependency(string physicalPath) {
            PhysicalPath = physicalPath;
            Produces = new HashSet<CacheItem>();
        }
    }
}
