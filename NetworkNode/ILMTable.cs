using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNodes
{
    public class ILMTable
    {
        public HashSet<IlmTableRow> Rows { get; set; }

        public ILMTable()
        {
            Rows = new HashSet<IlmTableRow>();
        }
    }
}
