namespace In.FileSearch.Engine
{
    public class SearchResult
    {
        public string RequestId { get; set; }
        public bool Completed { get; set; }
        public bool Canceled { get; set; }
        public bool Failed { get; set; }
        public string FailReason { get; set; }
        public string FileName { get; set; }
    }
}