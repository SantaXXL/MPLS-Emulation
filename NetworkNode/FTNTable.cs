using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNodes
{
    public class FTNTable
    {
        public HashSet<FtnTableRow> Rows { get; set; }

        public FTNTable()
        {
            Rows = new HashSet<FtnTableRow>();
        }
    }
}
