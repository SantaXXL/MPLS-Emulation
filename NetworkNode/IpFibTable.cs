using System.Collections.Generic;

namespace NetworkNodes
{
    public class IpFibTable
    {
        public HashSet<IpFibTableRow> Rows { get; set; }

        public IpFibTable()
        {
            Rows = new HashSet<IpFibTableRow>();
        }
    }
}
