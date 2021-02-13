using DNS.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdwardHsu.Dns
{
    public class StoreRecord
    {
        public RecordType Type { get; set; }
        public string Content { get; set; }
        public int TTL { get; set; }
    }
}
