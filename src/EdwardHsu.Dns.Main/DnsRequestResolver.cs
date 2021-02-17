using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdwardHsu.Dns.Main
{
    public class DnsRequestResolver : IRequestResolver
    {
        private IStoreProvider _storeProvider;
        private DnsClient _dnsClient;
        public DnsRequestResolver(IStoreProvider storeProvider)
        {
            _storeProvider = storeProvider;

            _dnsClient = new DnsClient(GetDnsAdress());
        }

        private static IPAddress GetDnsAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        /*if (dnsAdress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return IPAddress.Parse("8.8.8.8");
                        }*/
                        return dnsAdress;
                    }
                }
            }

            throw new InvalidOperationException("Unable to find DNS Address");
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            IResponse response = Response.FromRequest(request);

            response.RecursionAvailable = true;

            // First Query
            foreach (var query in request.Questions)
            {
                var records = await RecursiveFindResourceRecords(query.Name, query.Type).ConfigureAwait(false);

                foreach (var record in records)
                {
                    response.AnswerRecords.Add(ConvertToResourceRecord(record));
                }
            }

            return response;
        }

        private async Task<IEnumerable<StoreRecord>> RecursiveFindResourceRecords(Domain domain, RecordType type, bool external = false)
        {
            var basicQuery = _storeProvider.FindResourceRecords(domain.ToString(), type);

            if (basicQuery.Count() == 0 && (type == RecordType.A || type == RecordType.AAAA))
            {
                basicQuery = _storeProvider.FindResourceRecords(domain.ToString(), RecordType.CNAME);
            }

            List<StoreRecord> finalResult = new List<StoreRecord>();
            finalResult.AddRange(basicQuery);

            foreach (var result in basicQuery)
            {
                if (result.Type == RecordType.CNAME ||
                   result.Type == RecordType.MX ||
                   result.Type == RecordType.NS ||
                   result.Type == RecordType.SOA)
                {
                    finalResult.AddRange(await RecursiveFindResourceRecords(new Domain(result.Content), RecordType.A).ConfigureAwait(false));
                    finalResult.AddRange(await RecursiveFindResourceRecords(new Domain(result.Content), RecordType.AAAA).ConfigureAwait(false));
                    finalResult.AddRange(await RecursiveFindResourceRecords(new Domain(result.Content), RecordType.CNAME).ConfigureAwait(false));
                }
            }


            // Not found in this server
            if (finalResult.Count == 0 && _storeProvider.FindResourceRecords(domain.ToString()).Count() == 0)
            {
                ClientRequest ext_req = _dnsClient.Create();
                ext_req.Questions.Add(new Question(domain));
                ext_req.RecursionDesired = true;
                IResponse ext_resp = null;

                using (CancellationTokenSource s_cts = new CancellationTokenSource())
                {
                    s_cts.CancelAfter(1500);
                    ext_resp = await ext_req.Resolve(s_cts.Token).ConfigureAwait(false);
                }

                finalResult.AddRange(ext_resp.AnswerRecords.Select(x => ConvertToStoreRecord(x)));
            }

            return finalResult.Distinct();
        }


        private IResourceRecord ConvertToResourceRecord(StoreRecord record)
        {
            switch (record.Type)
            {
                case RecordType.A:
                case RecordType.AAAA:
                    return new IPAddressResourceRecord(new Domain(record.Domain), IPAddress.Parse(record.Content));
                case RecordType.NS:
                case RecordType.CNAME:
                case RecordType.MX:
                case RecordType.SOA:
                    return new CanonicalNameResourceRecord(new Domain(record.Domain), Domain.FromString(record.Content));
                default:
                    throw new NotSupportedException();
            }
        }

        private StoreRecord ConvertToStoreRecord(IResourceRecord record)
        {
            switch (record.Type)
            {
                case RecordType.A:
                case RecordType.AAAA:
                    return new StoreRecord()
                    {
                        Domain = record.Name.ToString(),
                        TTL = (int)record.TimeToLive.TotalSeconds,
                        Type = record.Type,
                        Content = (record as IPAddressResourceRecord).IPAddress.ToString()
                    };
                case RecordType.CNAME:
                    return new StoreRecord()
                    {
                        Domain = record.Name.ToString(),
                        TTL = (int)record.TimeToLive.TotalSeconds,
                        Type = record.Type,
                        Content = (record as CanonicalNameResourceRecord).CanonicalDomainName.ToString()
                    };
                case RecordType.MX:
                    return new StoreRecord()
                    {
                        Domain = record.Name.ToString(),
                        TTL = (int)record.TimeToLive.TotalSeconds,
                        Type = record.Type,
                        Content = (record as MailExchangeResourceRecord).ExchangeDomainName.ToString()
                    };
                case RecordType.NS:
                    return new StoreRecord()
                    {
                        Domain = record.Name.ToString(),
                        TTL = (int)record.TimeToLive.TotalSeconds,
                        Type = record.Type,
                        Content = (record as NameServerResourceRecord).NSDomainName.ToString()
                    };
                case RecordType.SOA:
                    return new StoreRecord()
                    {
                        Domain = record.Name.ToString(),
                        TTL = (int)record.TimeToLive.TotalSeconds,
                        Type = record.Type,
                        Content = (record as StartOfAuthorityResourceRecord).MasterDomainName.ToString()
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
