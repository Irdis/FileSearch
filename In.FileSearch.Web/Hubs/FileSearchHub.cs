using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using In.FileSearch.Engine;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace In.FileSearch.Web.Hubs
{
    public class FileSearchHub : Hub
    {
        private static readonly FileSearchStreamer _manager = new FileSearchStreamer();

        public override Task OnConnected()
        {
            _manager.Register(Context.ConnectionId, this);
            return Task.FromResult(0);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _manager.Remove(Context.ConnectionId);
            return Task.FromResult(0);
        }

        public void Subscribe(string requestId)
        {
            _manager.Subscribe(requestId, Context.ConnectionId);
        }

        public void Unsubscribe(string requestId)
        {
            _manager.Unsubscribe(requestId, Context.ConnectionId);
        }

        public void Notify(SearchResult searchResult, string connectionId)
        {
            var client = Clients.Client(connectionId);
            client.notify(searchResult);
        }
    }
}