namespace SassAndCoffee.Core.Pipeline {
    using System;
    using System.Collections.Generic;

    public interface IContentPipeline : IDisposable {
        IList<IContentTransform> Transformations { get; }
        ContentResult ProcessRequest(string physicalPath);
    }
}
