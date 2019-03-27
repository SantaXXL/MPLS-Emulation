using System.Net;

namespace NetworkNodes
{
    /// <summary>
    /// Represents an entry in MPLS-FIB table.
    /// </summary>
    public class MplsFibTableRow : ITableRow
    {
        /// <summary>
        /// Package destination adress.
        /// </summary>
        public IPAddress DestAddress { get; set; }

        /// <summary>
        /// FEC corresponding to <c>DestAddress</c>
        /// </summary>
        public int FEC { get; set; }

        /// <summary>
        /// Name of a router to which an MPLS-FIB entry belogs to.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// DestAddress | FEC
        /// </summary>
        public MplsFibTableRow(string serializedString) : base(serializedString)
        {
            var parts = serializedString.Split(null);
            DestAddress = IPAddress.Parse(parts[0]);
            FEC = int.Parse(parts[1]);
        }

        /// <summary>
        /// Serializes object into a string containing all of its public variables.
        /// </summary>
        /// <returns>Serialized object as a string.</returns>
        public override string Serialize()
        {
            return $"{DestAddress} {FEC}";
        }

        /// <summary>
        /// Checks if two MPLS-FIB entries are equal.
        /// </summary>
        /// <param name="other">An MPLS-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        protected bool Equals(MplsFibTableRow other)
        {
            return Equals(DestAddress, other.DestAddress) && FEC == other.FEC;
        }

        /// <summary>
        /// Checks if two MPLS-FIB entries are equal.
        /// </summary>
        /// <param name="obj">An MPLS-FIB entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MplsFibTableRow) obj);
        }

        /// <summary>
        /// Generates a hashcode of the entry.
        /// </summary>
        /// <returns>Generated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1;
                hashCode = (hashCode * 397) ^ (DestAddress != null ? DestAddress.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FEC;
                return hashCode;
            }
        }

        /// <summary>
        /// Converts a whole MPLS-FIB entry to a string; used in logs.
        /// </summary>
        /// <returns>Converted MPLS-FIB  entry to a string.</returns>
        public override string ToString()
        {
            return $"{nameof(DestAddress)}: {DestAddress}, {nameof(FEC)}: {FEC}";
        }
    }
}
