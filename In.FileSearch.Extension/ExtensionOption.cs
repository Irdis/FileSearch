using System;

namespace In.FileSearch.Extension
{
    [Serializable]
    public class ExtensionOption
    {
        public string AttributeName { get; set; }
        public OptionType AttributeType { get; set; }
    }
}