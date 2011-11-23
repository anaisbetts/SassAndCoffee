namespace SassAndCoffee.Core.Pipeline {
    using System;

    public abstract class ContentTransformBase : IContentTransform {
        public virtual void PreExecute(ContentTransformState state) {
        }

        public abstract void Execute(ContentTransformState state);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }
    }
}
