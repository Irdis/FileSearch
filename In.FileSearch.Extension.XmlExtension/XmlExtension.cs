using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In.FileSearch.Extension.XmlExtension
{
    public class XmlExtension : ExtensionBase
    {
        private static readonly ExtensionInfo _info;
        private const string TagName = "Attribute Name";
        private const string TagValue = "Attribute Value";

        static XmlExtension()
        {
            _info = new ExtensionInfo
            {
                Name = "Xml Extension",
                FileExtension = "xml",
                Options = new List<ExtensionOption>
                {
                    new ExtensionOption
                    {
                        AttributeName = TagName,
                        AttributeType = OptionType.String
                    },
                    new ExtensionOption
                    {
                        AttributeName = TagValue,
                        AttributeType = OptionType.String
                    }
                }
            };
        }
        public override ExtensionInfo GetInfo()
        {
            return _info;
        }

        public override ISearchExtensionUnit CreateUnit(Guid requestId, SearchTarget target, ISearchExtensionCallback callback)
        {
            return new XmlExtensionUnit(target.FilePath, target.Options[TagName], target.Options[TagValue], callback);
        }
    }
}
