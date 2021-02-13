using DNS.Server;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdwardHsu.Dns.Main
{
    public class DnsWorker : IHostedService
    {
        DnsServer _server;
        IStoreProvider _storeProvider;
        public DnsWorker(IConfiguration configuration, IStoreProvider storeProvider)
        {
            _storeProvider = storeProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // All dns requests received will be handled by the localhost request resolver
            _server = new DnsServer(new DnsRequestResolver(_storeProvider));
            _server.Listen();
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Dispose();
        }
    }
}