namespace In.FileSearch.Engine
{
    public static class FileSearchManagerFactory
    {
        private static volatile IFileSearchManager _instance;
        private static readonly object _lock = new object();

        public static IFileSearchManager Get()
        {
            if (_instance != null)
                return _instance;
            lock (_lock)
            {
                if (_instance != null)
                    return _instance;
                return _instance = new FileSearchManager();
            }
        }
    }
}