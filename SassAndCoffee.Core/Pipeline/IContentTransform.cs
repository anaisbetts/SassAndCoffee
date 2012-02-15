namespace SassAndCoffee.Core {

    public interface IContentTransform {
        void PreExecute(ContentTransformState state);
        void Execute(ContentTransformState state);
    }
}
