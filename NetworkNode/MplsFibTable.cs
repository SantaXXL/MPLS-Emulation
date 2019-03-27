using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNodes
{
    public class MplsFibTable
    {
        public HashSet<MplsFibTableRow> Rows { get; set; }

        public MplsFibTable()
        {
            Rows = new HashSet<MplsFibTableRow>();
        }
    }
}
