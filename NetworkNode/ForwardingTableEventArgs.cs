using System;

namespace NetworkNodes
{
    public class ForwardingTableEventArgs : EventArgs
    {
        public string Action { get; set; }

        public ITableRow Row { get; set; }

        public ForwardingTableEventArgs(string action, ITableRow row)
        {
            Action = action;
            Row = row;
        }

    }
}
