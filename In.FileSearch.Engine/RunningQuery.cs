using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine
{
    internal class RunningQuery
    {
        private readonly string _id;
        private readonly FileSearchOptions _options;
        private readonly string _ext;
        private readonly ISearchExtension _extension;
        private readonly IFileSearchCallback _callback;
        private readonly object _lock = new object();
        private volatile bool _completed = false;
        private volatile bool _canceled = false;
        private volatile bool _addingCompleted = false;
        private readonly ManualResetEvent _addingCompletedEvent = new ManualResetEvent(false);

        private readonly HashSet<Guid> _pendingMatches = new HashSet<Guid>();

        public RunningQuery(string id, FileSearchOptions options, ISearchExtension extension, IFileSearchCallback callback)
        {
            _id = id;
            _options = options;
            _extension = extension;
            _callback = callback;
            _ext = extension.GetInfo().FileExtension;
        }

        public void Run()
        {
            Task.Factory.StartNew(Search, TaskCreationOptions.LongRunning);
        }

        private void Search()
        {
            if (_canceled)
            {
                _addingCompleted = true;
                _addingCompletedEvent.Set();
                return;
            }
            try
            {
                var query = Directory.EnumerateFiles(_options.Directory, "*.*",
                    _options.IncludeSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var path in query)
                {
                    if (_canceled)
                    {
                        break;
                    }
                    if (!FileSearchFilter.Match(path, _ext, _options))
                    {
                        continue;
                    }
                    RunExtension(path);
                }
            }
            catch (Exception e)
            {
                Fail(e);
                return;
            }

            lock (_lock)
            {
                _addingCompleted = true;
                _addingCompletedEvent.Set();
                SetCompleted();
            }
        }

        private void Fail(Exception e)
        {
            var pendingMatches = GetPendingMatches();
            foreach (var pending in pendingMatches)
            {
                _extension.Stop(pending);
            }
            lock (_lock)
            {
                _pendingMatches.Clear();
                _completed = true;
                _addingCompleted = true;
                _addingCompletedEvent.Set();
                _callback.Completed(_id, false, true, e.Message);
            }
        }

        private void RunExtension(string path)
        {
            lock (_lock)
            {
                var guid = Guid.NewGuid();
                var callback = new RunningQueryCallback(guid, path, this);
                var target = new SearchTarget { FilePath = path, Options = _options.ExtensionOptions };
                _pendingMatches.Add(guid);
                _extension.Start(guid, target, callback);
            }
        }


        public void Result(Guid guid, string path, bool isMatched)
        {
            lock (_lock)
            {
                if (_completed)
                    return;
                _pendingMatches.Remove(guid);
                if (isMatched)
                {
                    _callback.AddResult(_id, path);
                }
                SetCompleted();
            }
        }

        public void Error(Guid guid)
        {
            lock (_lock)
            {
                if (_completed)
                    return;
                _pendingMatches.Remove(guid);
                SetCompleted();
            }
        }

        private void SetCompleted()
        {
            if (_addingCompleted && _pendingMatches.Count == 0 && !_canceled)
            {
                _callback.Completed(_id, false, false, null);
                _completed = true;
            }
        }

        private void CompleteAdding()
        {
            _canceled = true;
            _addingCompletedEvent.WaitOne();
        }

        public void Cancel()
        {
            CompleteAdding();
            var guids = GetPendingMatches();
            foreach (var pendingMatch in guids)
            {
                _extension.Stop(pendingMatch);
            }
            SetCanceled();
        }

        private void SetCanceled()
        {
            lock (_lock)
            {
                if (_completed)
                    return;
                _pendingMatches.Clear();
                _callback.Completed(_id, true, false, null);
                _completed = true;
            }
        }

        private Guid[] GetPendingMatches()
        {
            Guid[] guids;
            lock (_lock)
            {
                guids = new Guid[_pendingMatches.Count];
                _pendingMatches.CopyTo(guids);
            }
            return guids;
        }
    }
}