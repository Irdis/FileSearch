using System;
using System.Collections.Generic;

namespace In.FileSearch.Extension
{
    [Serializable]
    public class ExtensionInfo
    {
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public List<ExtensionOption> Options { get; set; }
    }
}