using System;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine
{
    internal class RunningQueryCallback : ISearchExtensionCallback
    {
        private readonly Guid _id;
        private readonly string _path;
        private readonly RunningQuery _query;

        public RunningQueryCallback(Guid id, string path, RunningQuery query)
        {
            _id = id;
            _path = path;
            _query = query;
        }

        public void Result(bool isMatched)
        {
            _query.Result(_id, _path, isMatched);
        }

        public void Error()
        {
            _query.Error(_id);
        }

        public void Canceled()
        {
        }
    }
}