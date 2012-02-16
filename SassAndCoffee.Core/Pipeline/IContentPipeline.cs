namespace SassAndCoffee.Core {
    using System.Collections.Generic;

    public interface IContentPipeline {
        IList<IContentTransform> Transformations { get; }
        ContentResult ProcessRequest(string physicalPath);
    }
}
