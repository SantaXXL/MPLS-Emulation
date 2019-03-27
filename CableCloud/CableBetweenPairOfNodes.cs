namespace CableCloud
{
    /// <summary>
    /// This class represents a cable (a fibre) between a pair of nodes, i.e.
    /// router/host - router/host.
    /// </summary>
    class CableBetweenPairOfNodes
    {
        /// <summary>
        /// A name of a node that a cable is connected to from one side.
        /// </summary>
        public string Node1 { get; set; }
        /// <summary>
        /// A port of <c>Node1</c> to which a cable is connected to.
        /// </summary>
        public string Port1 { get; set; }
        /// <summary>
        /// A name of a node that a cable is connected to from the other side.
        /// </summary>
        public string Node2 { get; set; }
        /// <summary>
        /// A port of <c>Node2</c> to which a cable is connected to.
        /// </summary>
        public string Port2 { get; set; }
        /// <summary>
        /// A status of a cable connecting <c>Node1</c> at port <c>Port1</c> to <c>Node2</c> at <c>Port2</c>.
        /// It might be 
        /// </summary>
        public string Status { get; set; }

        //public enum CableStatus { Working, OutOfOrder};

        /// <summary>
        /// Creates an instance of <c>CableBetweenPairOfNodes</c> with given input parameteres.
        /// </summary>
        /// <param name="node1">See <c>Node1</c></param>
        /// <param name="port1">See <c>Port1</c></param>
        /// <param name="node2">See <c>Node2</c></param>
        /// <param name="port2">See <c>Port2</c></param>
        /// <param name="status">See <c>Status</c></param>
        public CableBetweenPairOfNodes(string node1, string port1, string node2, string port2, string status)
        {
            Port1 = port1;
            Node2 = node2;
            Port2 = port2;
            Status = status;
            Node1 = node1;
        }
    }
}
