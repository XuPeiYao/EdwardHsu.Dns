using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

using EnumsNET;

using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EdwardHsu.Dns.RedisStore
{
    public class RedisStoreProvider : IStoreProvider
    {
        ConnectionMultiplexer _connection;
        RedisStoreOptions _options;
        public RedisStoreProvider(IOptions<RedisStoreOptions> options)
        {
            if (options == null || options.Value == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _connection = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _options = options.Value;
        }

        public Task CreateResourceRecords(IEnumerable<StoreRecord> resourceRecords)
        {
            throw new NotImplementedException();
        }

        public Task DeleteResourceRecords(IEnumerable<StoreRecord> resourceRecords)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StoreRecord> FindResourceRecords(string fqdn, RecordType type = RecordType.ANY)
        {
            List<StoreRecord> result = new List<StoreRecord>();

            if (type == RecordType.ANY)
            {
                foreach (var member in Enums.GetMembers<RecordType>())
                {
                    if (member.Value == RecordType.ANY) continue;
                    result.AddRange(FindResourceRecords(fqdn, member.Value));
                }
                return result.ToArray();
            }

            var db = GetDatabase();

            var key = new RedisKey(fqdn);

            if (db.KeyExists(key))
            {
                var storedRecords = db.ListRange(key)
                    .Select(x => x.ToString())
                    .Select(x => JObject.Parse(x));

                foreach (var storedRecord in storedRecords)
                {
                    result.Add(new StoreRecord()
                    {
                        Domain = fqdn,
                        Content = storedRecord["content"].Value<string>(),
                        TTL = storedRecord["ttl"]?.Value<int>() ?? 0,
                        Type = Enums.Parse<RecordType>(storedRecord["type"].Value<string>())
                    });
                }
            }

            result = result.Where(x => x.Type == type).ToList();

            return result.ToArray();
        }

        public Task UpdateResourceRecords(IEnumerable<StoreRecord> resourceRecords)
        {
            throw new NotImplementedException();
        }

        private IDatabase GetDatabase()
        {
            return _connection.GetDatabase(_options.DBNumber);
        }
    }
}
