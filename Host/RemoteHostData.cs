using System.Net;

namespace Host
{
    public class RemoteHostData
    {
        public IPAddress IPAddress { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Tworzy obiekt z reprezentacji stringowej
        /// </summary>
        public RemoteHostData(string info)
        {
            var parts = info.Split('#');

            IPAddress = IPAddress.Parse(parts[0]);
            Description = parts[1];
        }

        public override string ToString()
        {
            return $"{Description}, {IPAddress.ToString()}";
        }

    }
}
