using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdwardHsu.Dns.Main
{
    public class DnsRequestResolver : IRequestResolver
    {
        private IStoreProvider _storeProvider;
        public DnsRequestResolver(IStoreProvider storeProvider)
        {
            _storeProvider = storeProvider;
        }

        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            IResponse response = Response.FromRequest(request);

            response.RecursionAvailable = false;

            foreach (var query in request.Questions)
            {
                var records = _storeProvider.FindResourceRecords(query.Name.ToString(), query.Type);

                foreach (var record in records)
                {
                    response.AnswerRecords.Add(ConvertToResourceRecord(query.Name, record));
                }
            }

            foreach (var query in response.AnswerRecords.Where(x => x.Type == RecordType.CNAME).ToArray())
            {
                var records = _storeProvider.FindResourceRecords((query as CanonicalNameResourceRecord).CanonicalDomainName.ToString());

                foreach (var record in records)
                {
                    response.AnswerRecords.Add(ConvertToResourceRecord(query.Name, record));
                }
            }

            return response;
        }

        private IResourceRecord ConvertToResourceRecord(Domain domain, StoreRecord record)
        {
            switch (record.Type)
            {
                case RecordType.A:
                case RecordType.AAAA:
                case RecordType.NS:
                    return new IPAddressResourceRecord(domain, IPAddress.Parse(record.Content));
                case RecordType.CNAME:
                    return new CanonicalNameResourceRecord(domain, Domain.FromString(record.Content));
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
