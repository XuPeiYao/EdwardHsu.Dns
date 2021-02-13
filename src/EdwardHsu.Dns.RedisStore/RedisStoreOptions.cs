using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.Dns.RedisStore
{
    public class RedisStoreOptions
    {
        public string ConnectionString { get; set; }
        public int DBNumber { get; set; }
    }
}
