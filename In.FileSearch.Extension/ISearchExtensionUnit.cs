namespace In.FileSearch.Extension
{
    public interface ISearchExtensionUnit
    {
        ISearchExtensionCallback Callback { get; }
        void Start();
        void Cancel();
    }
}