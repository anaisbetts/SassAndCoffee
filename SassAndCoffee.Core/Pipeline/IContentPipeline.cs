namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;

    public interface IContentPipeline {
        IList<IContentTransform> Transformations { get; }
        ContentResult ProcessRequest(string physicalPath);
    }
}
