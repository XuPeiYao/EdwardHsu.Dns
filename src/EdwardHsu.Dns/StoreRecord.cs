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
        public string Domain { get; set; }
        public RecordType Type { get; set; }
        public string Content { get; set; }
        public int TTL { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is StoreRecord record)
            {
                return record.GetHashCode() == GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Domain.GetHashCode() ^ Type.GetHashCode() ^ Content.GetHashCode() ^ TTL.GetHashCode();
        }
    }
}
