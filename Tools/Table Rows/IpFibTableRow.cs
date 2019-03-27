using System.Net;

namespace NetworkNodes
{
    /// <summary>
    /// Represents an entry in IP-FIB table.
    /// </summary>
    public class IpFibTableRow : ITableRow
    {
        /// <summary>
        /// Package destination adress.
        /// </summary>
        public IPAddress DestAddress { get; set; }

        /// <summary>
        /// Package output port.
        /// </summary>
        public ushort OutPort { get; set; }

        /// <summary>
        /// Name of a router to which an IP-FIB entry belogs to.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// DestAddress | OutPort
        /// </summary>
        public IpFibTableRow(string serializedString) : base (serializedString)
        {
            var parts = serializedString.Split(null);

            DestAddress = IPAddress.Parse(parts[0]);
            OutPort = ushort.Parse(parts[1]);
        }

        /// <summary>
        /// Checks if two IP-FIB entries are equal.
        /// </summary>
        /// <param name="other">An IP-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        protected bool Equals(IpFibTableRow other)
        {
            return Equals(DestAddress, other.DestAddress) && OutPort == other.OutPort;
        }

        /// <summary>
        /// Checks if two IP-FIB entries are equal.
        /// </summary>
        /// <param name="obj">An IP-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IpFibTableRow) obj);
        }

        /// <summary>
        /// Generates a hashcode of the entry.
        /// </summary>
        /// <returns>Generated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DestAddress != null ? DestAddress.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OutPort.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes a IP-FIB table row.
        /// </summary>
        /// <returns>Serialized IP-FIB table row.</returns>
        public override string Serialize()
        {
            return $"{DestAddress} {OutPort}";
        }

        /// <summary>
        /// Converts a whole IP-FIB entry to a string; used in logs.
        /// </summary>
        /// <returns>Converted IP-FIB  entry to a string.</returns>
        public override string ToString()
        {
            return $"{nameof(DestAddress)}: {DestAddress}, {nameof(OutPort)}: {OutPort}";
        }
    }
}
