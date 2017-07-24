using System;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine
{
    public interface IExtensionLoader : IDisposable
    {
        void Init(IExtensionWatcher watcher);
        string[] GetExtensions();
        bool CreateExtension(string path, out ISearchExtension extension);
        void DisposeExtension(string path);
    }
}