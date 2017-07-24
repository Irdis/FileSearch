using System;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine.Loader
{
    public class ProxyExtension : ISearchExtension
    {
        private readonly ISearchExtension _extension;

        public ProxyExtension(ISearchExtension extension)
        {
            _extension = extension;
        }

        public ExtensionInfo GetInfo()
        {
            return _extension.GetInfo();
        }

        public void Start(Guid requestId, SearchTarget target, ISearchExtensionCallback callback)
        {
            var searchExtensionCallback = new ProxyCallback(callback);
            _extension.Start(requestId, target, searchExtensionCallback);
        }

        public void Stop(Guid requestId)
        {
            _extension.Stop(requestId);
        }

        public void Unload()
        {
            _extension.Unload();
        }
    }
}