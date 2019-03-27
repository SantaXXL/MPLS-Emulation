namespace NetworkNodes
{
    /// <summary>
    /// Represents an entry in FTN table.
    /// </summary>
    public class FtnTableRow : ITableRow
    {
        /// <summary>
        /// Forwarding Equivalence Class; represents a local tunnel ID in MPLS network.
        /// </summary>
        public int FEC { get; set; }

        /// <summary>
        /// "Pointer" to NHLFE entry.
        /// </summary>
        public int NHLFE_ID { get; set; }

        /// <summary>
        /// Name of the router that has FTN table.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// FEC | NHLFE_ID
        /// </summary>
        /// <param name="serializedString">Serialized string containing public variables from the FtnTableRow object.</param>
        public FtnTableRow(string serializedString) : base(serializedString)
        {
            var parts = serializedString.Split(null);

            FEC = int.Parse(parts[0]);
            NHLFE_ID = int.Parse(parts[1]);
        }

        /// <summary>
        /// Serializes a FTN table row.
        /// </summary>
        /// <returns>Serialized FTN table row.</returns>
        public override string Serialize()
        {
            return $"{FEC} {NHLFE_ID}";
        }

        /// <summary>
        /// Compares whether FTN entries are identical.
        /// </summary>
        /// <param name="other">Second FTN entry.</param>
        /// <returns>True, if are identical; false, if otherwise.</returns>
        protected bool Equals(FtnTableRow other)
        {
            return FEC == other.FEC && NHLFE_ID == other.NHLFE_ID;
        }

        /// <summary>
        /// Overrides built-in <c>Equals</c> method. Compares, if given two
        /// objects are identical.
        /// </summary>
        /// <param name="obj">Object to compare FTN entry with.</param>
        /// <returns>True, if compared object is this or if data in object and this
        /// are identical; false, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FtnTableRow) obj);
        }

        /// <summary>
        /// Calculates and returns a hashcode, based on data from FTN entry.
        /// </summary>
        /// <returns>Calculated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (FEC * 397) ^ NHLFE_ID;
            }
        }

        /// <summary>
        /// Converts FTN entry data to a string; used in logs.
        /// </summary>
        /// <returns>Data from FTN entry as a string.</returns>
        public override string ToString()
        {
            return $"{nameof(FEC)}: {FEC}, {nameof(NHLFE_ID)}: {NHLFE_ID}";
        }
    }
}
