using System;
using System.Runtime.Remoting.Lifetime;

namespace In.FileSearch.Extension.Sponsor
{
    public class ExtensionCallbackSponsorManager
    {
        private readonly ClientSponsor _callbackSponsor;

        public ExtensionCallbackSponsorManager()
        {
            _callbackSponsor = new ClientSponsor();
        }

        public ISearchExtensionCallback Register(ISearchExtensionCallback callback)
        {
            var marshaledRef = (MarshalByRefObject)callback;
            var lease = (ILease)marshaledRef.GetLifetimeService();
            lease.Register(_callbackSponsor);
            return new WrappedCallback(callback, () =>
            {
                lease.Unregister(_callbackSponsor);
            });
        }
    }
}