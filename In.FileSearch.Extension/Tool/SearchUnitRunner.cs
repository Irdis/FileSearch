using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace In.FileSearch.Extension.Tool
{
    public class SearchUnitRunner
    {
        private const int MaxRunningTasks = 10;
        private readonly Dictionary<Guid, ISearchExtensionUnit> _requests = new Dictionary<Guid, ISearchExtensionUnit>();
        private readonly Dictionary<Guid, ISearchExtensionUnit> _runningRequests = new Dictionary<Guid, ISearchExtensionUnit>(MaxRunningTasks);
        private readonly Queue<Guid> _requestQueue = new Queue<Guid>();
        private readonly object _lock = new object();
        private volatile bool _completed = false;
        private volatile ManualResetEvent _completedEvent = new ManualResetEvent(false);

        public SearchUnitRunner()
        {
            Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (!_completed)
            {
                lock (_lock)
                {
                    if (_requests.Count == 0 ||
                        _runningRequests.Count == MaxRunningTasks)
                    {
                        Monitor.Wait(_lock);
                    }
                    while (_requestQueue.Count > 0 && _runningRequests.Count < MaxRunningTasks)
                    {
                        var requestId = _requestQueue.Dequeue();
                        ISearchExtensionUnit unit;
                        if (!_requests.TryGetValue(requestId, out unit))
                        {
                            continue;
                        }
                        _requests.Remove(requestId);
                        _runningRequests.Add(requestId, unit);
                        Task.Factory.StartNew(() => RunUnit(requestId, unit));
                    }
                }
            }
            _completedEvent.Set();
        }

        private void RunUnit(Guid requestId, ISearchExtensionUnit unit)
        {
            unit.Start();
            lock (_lock)
            {
                _runningRequests.Remove(requestId);
                Monitor.Pulse(_lock);
            }
        }

        public void Start(Guid requestId, ISearchExtensionUnit unit)
        {
            lock (_lock)
            {
                _requests.Add(requestId, unit);
                _requestQueue.Enqueue(requestId);

                Monitor.Pulse(_lock);
            }
        }

        public void Stop(Guid requestId)
        {
            lock (_lock)
            {
                ISearchExtensionUnit pending;
                if (_requests.TryGetValue(requestId, out pending))
                {
                    pending.Callback.Canceled();
                    _requests.Remove(requestId);
                    Monitor.Pulse(_lock);
                    return;
                }
                ISearchExtensionUnit running;
                if (_runningRequests.TryGetValue(requestId, out running))
                {
                     running.Cancel();
                }
            }
        }

        public void Terminate()
        {
            lock (_lock)
            {
                foreach (var pending in _requests.Values)
                {
                    pending.Callback.Canceled();
                }
                _requests.Clear();
                _requestQueue.Clear();
                foreach (var running in _runningRequests.Values)
                {
                    running.Cancel();
                }
                _completed = true;
                Monitor.Pulse(_lock);
            }
            _completedEvent.WaitOne();
        }
    }
}