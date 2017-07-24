using System;
using System.Collections.Generic;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine
{
    public interface IFileSearchManager : IDisposable
    {
        List<ExtensionInfo> GetExtensionList();
        void Search(string id, FileSearchOptions options);
        void Search(string id, FileSearchOptions options, IFileSearchCallback callback);
        void Cancel(string id);

        event Action<SearchResult> OnResult;
    }
}