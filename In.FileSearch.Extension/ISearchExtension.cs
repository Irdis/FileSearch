using System;

namespace In.FileSearch.Extension
{
    public interface ISearchExtension
    {
        ExtensionInfo GetInfo();
        void Start(Guid requestId, SearchTarget target, ISearchExtensionCallback callback);
        void Stop(Guid requestId);
        void Unload();
    }
}