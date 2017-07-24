using System.Collections.Generic;

namespace In.FileSearch.Engine
{
    public interface IExtensionWatcher
    {
        void ExtensionListChanged(List<string> addedExtensions, List<string> removedExtesions);
    }
}