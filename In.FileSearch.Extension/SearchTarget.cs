using System;
using System.Collections.Generic;

namespace In.FileSearch.Extension
{
    [Serializable]
    public class SearchTarget 
    {
        public string FilePath { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
}