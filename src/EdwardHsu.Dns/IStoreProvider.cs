using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdwardHsu.Dns
{
    public interface IStoreProvider
    {
        IEnumerable<StoreRecord> FindResourceRecords(string fqdn, RecordType type = RecordType.ANY);
        Task CreateResourceRecords(IEnumerable<StoreRecord> resourceRecords);
        Task UpdateResourceRecords(IEnumerable<StoreRecord> resourceRecords);
        Task DeleteResourceRecords(IEnumerable<StoreRecord> resourceRecords);
    }
}
