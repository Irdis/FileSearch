using System;
using System.Collections.Generic;
using In.FileSearch.Engine;
using In.FileSearch.Extension;

namespace In.FileSearch.Cmd
{
    public class ProxyCallback : MarshalByRefObject, ISearchExtensionCallback
    {

        public void Result(bool isMatched)
        {
        }

        public void Error()
        {
        }

        public void Canceled()
        {
        }
    }
    class Program : IFileSearchCallback, IExtensionWatcher
    {
        static void Main(string[] args)
        {
            var fileSearchManager = new FileSearchManager();
            fileSearchManager.Init();
            fileSearchManager.OnResult += FileSearchManagerOnResult;
            while (true)
            {
                var options = new Dictionary<string, string>()
                {
                    {"Substring", "hello"}
                };
                for (int i = 0; i < 10; i++)
                {
                    fileSearchManager.Search(i.ToString(),
                        new FileSearchOptions
                        {
                            Directory = @"c:\Projects",
                            IncludeSubDir = true,
                            ExtensionName = "SubstringExt",
                            ExtensionOptions = options
                        });
                }
                Console.ReadKey();
                Console.WriteLine("Canceling");
                for (int i = 0; i < 10; i++)
                {
                    fileSearchManager.Cancel(i.ToString());
                }
                Console.WriteLine("Canceled");
                Console.ReadKey();
            }
        }

        private static void FileSearchManagerOnResult(SearchResult searchResult)
        {
            if (!searchResult.Completed)
            {
                new Program().AddResult(searchResult.RequestId, searchResult.FileName);
            }
            else
            {
                new Program().Completed(searchResult.RequestId, searchResult.Canceled);
            }
        }

        public void AddResult(string id, string fileName)
        {
            Console.WriteLine(id+" " + fileName);
        }

        public void Completed(string id, bool canceled)
        {
            Console.WriteLine(id+" Done - canceled " + canceled);
        }

        public void ExtensionListChanged(List<string> addedExtensions, List<string> removedExtesions)
        {
            

        }
    }
}
