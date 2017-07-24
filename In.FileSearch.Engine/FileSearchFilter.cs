using System;
using System.IO;

namespace In.FileSearch.Engine
{
    public static class FileSearchFilter
    {
        public static bool Match(string path, string ext, FileSearchOptions options)
        {
            if (!string.Equals(Path.GetExtension(path), "." + ext, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (options.CreatedDateFrom.HasValue && File.GetCreationTime(path) < options.CreatedDateFrom.Value)
            {
                return false;
            }
            if (options.CreatedDateTo.HasValue && File.GetCreationTime(path) >= options.CreatedDateTo.Value)
            {
                return false;
            }
            if (options.ModifyDateFrom.HasValue && File.GetLastWriteTime(path) < options.ModifyDateFrom.Value)
            {
                return false;
            }
            if (options.ModifyDateTo.HasValue && File.GetLastWriteTime(path) >= options.ModifyDateTo.Value)
            {
                return false;
            }
            if (options.FileAttributes.HasValue &&
                ((int) File.GetAttributes(path) & options.FileAttributes.Value) != options.FileAttributes.Value)
            {
                return false;
            }
            return true;
        }
    }
}