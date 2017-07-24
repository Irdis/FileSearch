using System;

namespace In.FileSearch.Extension.Sponsor
{
    public class WrappedCallback : ISearchExtensionCallback
    {
        private readonly ISearchExtensionCallback _callback;
        private readonly Action _done;

        public WrappedCallback(ISearchExtensionCallback callback, Action done)
        {
            _callback = callback;
            _done = done;
        }

        public void Result(bool isMatched)
        {
            _callback.Result(isMatched);
            _done();
        }

        public void Error()
        {
            _callback.Error();
            _done();
        }

        public void Canceled()
        {
            _callback.Canceled();
            _done();
        }
    }
}