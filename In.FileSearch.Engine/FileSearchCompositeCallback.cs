namespace In.FileSearch.Engine
{
    public class FileSearchCompositeCallback : IFileSearchCallback
    {
        private readonly IFileSearchCallback _first;
        private readonly IFileSearchCallback _second;

        public FileSearchCompositeCallback(IFileSearchCallback first, IFileSearchCallback second)
        {
            _first = first;
            _second = second;
        }

        public void AddResult(string id, string fileName)
        {
            _first.AddResult(id, fileName);
            _second.AddResult(id, fileName);
        }

        public void Completed(string id, bool canceled, bool failed, string failReason)
        {
            _first.Completed(id, canceled, failed, failReason);
            _second.Completed(id, canceled, failed, failReason);
        }
    }
}