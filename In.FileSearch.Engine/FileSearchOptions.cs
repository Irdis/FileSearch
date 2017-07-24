using System;
using System.Collections.Generic;

namespace In.FileSearch.Engine
{
    public class FileSearchOptions
    {
        public string Directory { get; set; }
        public bool IncludeSubDir { get; set; }
        public int? FileAttributes { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public DateTime? ModifyDateFrom { get; set; }
        public DateTime? ModifyDateTo { get; set; }
        public string ExtensionName { get; set; }
        public Dictionary<string, string> ExtensionOptions { get; set; }
    }
}