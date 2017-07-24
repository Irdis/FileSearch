namespace In.FileSearch.Engine
{
    public interface IFileSearchCallback
    {
        void AddResult(string id, string fileName);
        void Completed(string id, bool canceled, bool failed, string failReason);
    }
}