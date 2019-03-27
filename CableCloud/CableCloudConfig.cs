using System.Collections.Generic;
using System.IO;

namespace CableCloud
{
    public class CableCloudConfig
    {
        // e.g. H1, 1.1.1.1
        private Dictionary<string, string> NodeNameToIP;
        // e.g. H1, R1
        private Dictionary<string, string> PairsOfConnectedNodes;
        // e.g. H1:1111-R2:2222, WORKING
        public Dictionary<string, string> StatusOfCableBetweenNodes { get; set; }
        public CableCloudConfig(StreamReader file)
        {
            NodeNameToIP = new Dictionary<string, string>();
            string line;
            while (!(line = file.ReadLine()).Equals(""))
            {
                // e.g. H1 1.1.1.1
                var parts = line.Split(' ');
                NodeNameToIP.Add(parts[0], parts[1]);
            }
            file.ReadLine(); file.ReadLine(); // skip 2 lines

            PairsOfConnectedNodes = new Dictionary<string, string>();
            StatusOfCableBetweenNodes = new Dictionary<string, string>();
            while ((line = file.ReadLine()) != null)
            {
                // e.g. R1 20300 H1 10100 WORKING
                var parts = line.Split(' ');
                string node1 = parts[0], port1 = parts[1], node2 = parts[2], port2 = parts[3], status = parts[4];
                PairsOfConnectedNodes.Add(node1 + ":" + port1, node2 + ":" + port2);
                // in config there is only R1 20300 H1 10100, but we need H1 10100 R1 20300 as well
                PairsOfConnectedNodes.Add(node2 + ":" + port2, node1 + ":" + port1);
                var address1 = node1 + ":" + port1;
                var address2 = node2 + ":" + port2;
                StatusOfCableBetweenNodes.Add(address1 + "-" + address2, status); 
                StatusOfCableBetweenNodes.Add(address2 + "-" + address1, status);
            }
        }

        public string GetIP(string node)
        {
            return NodeNameToIP[node];
        }

        public string GetCableStatus(string node1, string port1, string node2, string port2)
        {
            var connectedNodes = node1 + ":" + port1 + "-" + node2 + ":" + port2;
            return StatusOfCableBetweenNodes[connectedNodes];
        }

        public string GetNextNode(string node, string port)
        {
            var parts = PairsOfConnectedNodes[node + ":" + port].Split(':');
            return parts[0];
        }

        public string GetNextPort(string node, string port)
        {
            var parts = PairsOfConnectedNodes[node + ":" + port].Split(':');
            return parts[1];
        }
    }
}
