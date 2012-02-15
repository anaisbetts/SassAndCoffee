namespace SassAndCoffee.Core {
    using System.Collections.Generic;

    public class CacheItem {
        public string PhysicalPath { get; set; }
        public HashSet<CacheDependency> Dependencies { get; private set; }

        public CacheItem(string physicalPath) {
            PhysicalPath = physicalPath;
            Dependencies = new HashSet<CacheDependency>();
        }
    }
}
