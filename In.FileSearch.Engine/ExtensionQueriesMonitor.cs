using System.Collections.Generic;
using System.Linq;

namespace In.FileSearch.Engine
{
    public class ExtensionQueriesMonitor
    {
        private readonly Dictionary<string, HashSet<string>> _runningExtensions = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> _runningRequests = new Dictionary<string, string>();

        public void Add(string extensionId, string requestId)
        {
            HashSet<string> set;
            if (_runningExtensions.TryGetValue(extensionId, out set))
            {
                set.Add(requestId);
            }
            else
            {
                _runningExtensions[extensionId] = new HashSet<string>() {requestId};
            }
            _runningRequests[requestId] = extensionId;
        }

        public void Remove(string extensionId, string requestId)
        {
            HashSet<string> set;
            _runningExtensions.TryGetValue(extensionId, out set);
            set.Remove(requestId);
            if (set.Count == 0)
            {
                _runningExtensions.Remove(extensionId);
            }
            _runningRequests.Remove(requestId);
        }

        public void RemoveExtension(string extensionId)
        {
            _runningExtensions.Remove(extensionId);
        }

        public void RemoveRequest(string requestId)
        {
            var extensionId = _runningRequests[requestId];
            HashSet<string> set;
            _runningExtensions.TryGetValue(extensionId, out set);
            set.Remove(requestId);
            if (set.Count == 0)
            {
                _runningExtensions.Remove(extensionId);
            }
            _runningRequests.Remove(requestId);
        }

        public IEnumerable<string> GetRequests(string extensionId)
        {
            HashSet<string> set;
            if (_runningExtensions.TryGetValue(extensionId, out set))
            {
                return set;
            }
            return Enumerable.Empty<string>();
        }
    }
}