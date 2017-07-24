namespace In.FileSearch.Extension
{
    public interface ISearchExtensionCallback
    {
        void Result(bool isMatched);
        void Error();
        void Canceled();
    }
}