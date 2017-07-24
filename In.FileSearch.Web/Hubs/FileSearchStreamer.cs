using System;
using System.Collections.Generic;
using In.FileSearch.Engine;

namespace In.FileSearch.Web.Hubs
{
    public class FileSearchStreamer
    {
        private readonly IFileSearchManager _manager = FileSearchManagerFactory.Get();
        private readonly object _lock = new object();
        private readonly Dictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> _requests = new Dictionary<string, string>();
        private readonly Dictionary<string, FileSearchHub> _hubs = new Dictionary<string, FileSearchHub>();

        public FileSearchStreamer()
        {
            _manager.OnResult += ManagerOnResult;
        }

        private void ManagerOnResult(SearchResult obj)
        {
            lock (_lock)
            {
                string connectionId;
                if (_requests.TryGetValue(obj.RequestId, out connectionId))
                {
                    _hubs[connectionId].Notify(obj, connectionId);
                }
            }
        }

        public void Subscribe(string requestId, string connectionId)
        {
            lock (_lock)
            {
                HashSet<string> clientRequests;
                if (_connections.TryGetValue(connectionId, out clientRequests))
                {
                    clientRequests.Add(requestId);
                }
                else
                {
                    _connections[connectionId] = new HashSet<string> { requestId };
                }
                _requests[requestId] = connectionId;
            }
        }

        public void Unsubscribe(string requestId, string connectionId)
        {
            lock (_lock)
            {
                HashSet<string> clientRequests;
                if (_connections.TryGetValue(connectionId, out clientRequests))
                {
                    clientRequests.Remove(requestId);
                    if (clientRequests.Count == 0)
                    {
                        _connections.Remove(connectionId);
                    }
                }
                _requests.Remove(requestId);
            }
        }

        public void Register(string connectionId, FileSearchHub fileSearchHub)
        {
            lock (_lock)
            {
                _hubs.Add(connectionId, fileSearchHub);
            }
        }

        public void Remove(string connectionId)
        {
            lock (_lock)
            {
                HashSet<string> requests;
                if (_connections.TryGetValue(connectionId, out requests))
                {
                    foreach (var request in requests)
                    {
                        _requests.Remove(request);
                    }
                    _connections.Remove(connectionId);
                }
                _hubs.Remove(connectionId);
            }
        }
    }
}