using System;
using In.FileSearch.Extension.Sponsor;
using In.FileSearch.Extension.Tool;

namespace In.FileSearch.Extension
{
    public abstract class ExtensionBase : MarshalByRefObject, ISearchExtension
    {
        private readonly SearchUnitRunner _runner = new SearchUnitRunner();
        private readonly ExtensionCallbackSponsorManager _sponsorManager = new ExtensionCallbackSponsorManager();

        public abstract ExtensionInfo GetInfo();
        public abstract ISearchExtensionUnit CreateUnit(Guid requestId, SearchTarget target, ISearchExtensionCallback callback);

        public void Start(Guid requestId, SearchTarget target, ISearchExtensionCallback callback)
        {
            var wrappedCallback = _sponsorManager.Register(callback);
            var unit = CreateUnit(requestId, target, wrappedCallback);
            _runner.Start(requestId, unit);
        }

        public void Stop(Guid requestId)
        {
            _runner.Stop(requestId);
        }

        public void Unload()
        {
            _runner.Terminate();
        }
    }
}