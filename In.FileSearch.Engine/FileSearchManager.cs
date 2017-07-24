using System;
using System.Collections.Generic;
using System.Linq;
using In.FileSearch.Engine.Loader;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine
{
    public class FileSearchManager : IFileSearchManager, IExtensionWatcher, IFileSearchCallback
    {
        private readonly IExtensionLoader _loader;

        private readonly Dictionary<string, ISearchExtension> _extensions = new Dictionary<string, ISearchExtension>();
        private readonly Dictionary<string, string> _extensionPaths = new Dictionary<string, string>();

        private readonly ExtensionQueriesMonitor _extensionMonitor = new ExtensionQueriesMonitor();
        private readonly Dictionary<string, RunningQuery> _runningQueries = new Dictionary<string, RunningQuery>();

        private readonly object _lock = new object();

        public event Action<SearchResult> OnResult;

        public FileSearchManager()
        {
            _loader = new ExtensionLoader();
            Init();
        }

        private void Init()
        {
            var extensionsPaths = _loader.GetExtensions();
            AddExtension(extensionsPaths);
            _loader.Init(this);
        }

        public List<ExtensionInfo> GetExtensionList()
        {
            lock (_lock)
            {
                return _extensions.Values.Select(extension => extension.GetInfo()).ToList();
            }
        }

        public void Search(string id, FileSearchOptions options)
        {
            Search(id, options, null);
        }

        public void Search(string id, FileSearchOptions options, IFileSearchCallback callback)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Invalid query id");
            }
            string error;
            if (!ValidateOptions(options, out error))
            {
                throw new ArgumentException(error, "options");
            }
            lock (_lock)
            {
                ISearchExtension ext;
                if (!_extensions.TryGetValue(options.ExtensionName, out ext))
                {
                    throw new InvalidOperationException("Invalid extension name");
                }
                if (_runningQueries.ContainsKey(id))
                {
                    throw new InvalidOperationException("Query with the same id is already running");
                }
                var searchCallback = CreateCallback(callback);
                var query = new RunningQuery(id, options, ext, searchCallback);
                _runningQueries.Add(id, query);
                _extensionMonitor.Add(options.ExtensionName, id);
                query.Run();
            }
        }

        private IFileSearchCallback CreateCallback(IFileSearchCallback callback)
        {
            if (callback == null)
            {
                return this;
            }
            return new FileSearchCompositeCallback(this, callback);
        }

        private bool ValidateOptions(FileSearchOptions options, out string error)
        {
            error = null;
            if (options == null)
            {
                error = "No parameters";
                return false;
            }
            if (string.IsNullOrEmpty(options.ExtensionName))
            {
                error = "No extension";
                return false;
            }
            return true;
        }

        public void Cancel(string id)
        {
            RunningQuery q;
            lock (_lock)
            {
                _runningQueries.TryGetValue(id, out q);
            }
            if (q != null)
            {
                q.Cancel();
            }
        }

        public void ExtensionListChanged(List<string> addedExtensions, List<string> removedExtesions)
        {
            RemoveExtensions(removedExtesions);
            AddExtension(addedExtensions);
        }

        private void AddExtension(IEnumerable<string> addedExtensions)
        {
            foreach (var addedExtension in addedExtensions)
            {
                lock (_lock)
                {
                    ISearchExtension extension;
                    if (_loader.CreateExtension(addedExtension, out extension))
                    {
                        var name = extension.GetInfo().Name;
                        _extensionPaths.Add(addedExtension, name);
                        _extensions.Add(name, extension);
                    }
                }
            }
        }

        private void RemoveExtensions(IEnumerable<string> removedExtensions)
        {
            foreach (var removedExtension in removedExtensions)
            {
                List<RunningQuery> runningQueries = null;
                bool initializedExtension = false;
                ISearchExtension extension = null;
                lock (_lock)
                {
                    string extensionName;
                    if (_extensionPaths.TryGetValue(removedExtension, out extensionName))
                    {
                        runningQueries = _extensionMonitor.GetRequests(extensionName)
                            .Select(requestId => _runningQueries[requestId]).ToList();
                        extension = _extensions[extensionName];
                        _extensions.Remove(extensionName);
                        _extensionPaths.Remove(removedExtension);
                        initializedExtension = true;
                    }
                }
                if (initializedExtension)
                {
                    foreach (var runningQuery in runningQueries)
                    {
                        runningQuery.Cancel();
                    }
                    extension.Unload();
                    _loader.DisposeExtension(removedExtension);
                }
            }
        }
        

        public void AddResult(string id, string fileName)
        {
            EmitResult(() => new SearchResult
            {
                RequestId = id,
                Completed = false,
                Canceled = false,
                FileName = fileName
            });
        }

        public void Completed(string id, bool canceled, bool failed, string failReason)
        {
            lock (_lock)
            {
                _extensionMonitor.RemoveRequest(id);
                _runningQueries.Remove(id);
            }
            EmitResult(() => new SearchResult
            {
                RequestId = id,
                Completed = true,
                Canceled = canceled,
                Failed = failed,
                FailReason = failReason
            });
        }

        protected virtual void EmitResult(Func<SearchResult> arg1)
        {
            OnResult?.Invoke(arg1());
        }

        public void Dispose()
        {
            _loader?.Dispose();
        }
    }
}