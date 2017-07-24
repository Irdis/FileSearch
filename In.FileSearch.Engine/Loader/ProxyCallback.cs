using System;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine.Loader
{
    public class ProxyCallback : MarshalByRefObject, ISearchExtensionCallback
    {
        private readonly ISearchExtensionCallback _callback;

        public ProxyCallback(ISearchExtensionCallback callback)
        {
            _callback = callback;
        }

        public void Result(bool isMatched)
        {
            _callback.Result(isMatched);
        }

        public void Error()
        {
            _callback.Error();
        }

        public void Canceled()
        {
            _callback.Canceled();
        }
    }
}