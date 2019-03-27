using System.Collections.Generic;

namespace NetworkNodes
{
    public class NHLFETable
    {
        public HashSet<NHLFETableRow> Rows { get; set; }

        public NHLFETable()
        {
            Rows = new HashSet<NHLFETableRow>();
        }

    }
}
