using System;
using System.Collections.Generic;

namespace In.FileSearch.Extension.TxtExtension
{
    public class TxtExtension : ExtensionBase
    {
        private static readonly ExtensionInfo _info;
        private const string Substring = "Substring";
        static TxtExtension()
        {
            _info = new ExtensionInfo
            {
                Name = "Text Extension",
                FileExtension = "txt",
                Options = new List<ExtensionOption>
                {
                    new ExtensionOption { AttributeName = Substring, AttributeType = OptionType.String }
                }
            };
        }

        public override ExtensionInfo GetInfo()
        {
            return _info;
        }

        public override ISearchExtensionUnit CreateUnit(Guid requestId, SearchTarget target, ISearchExtensionCallback callback)
        {
            return new TxtExtensionUnit(target.FilePath, target.Options[Substring], callback);
        }
    }
}