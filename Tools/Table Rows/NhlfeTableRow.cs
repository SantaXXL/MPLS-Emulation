
namespace NetworkNodes
{
    /// <summary>
    /// Reprents an entry in an NHLFE table.
    /// </summary>
    public class NHLFETableRow : ITableRow
    {
        /// <summary>
        /// Entry's ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// An action to do: POP/SWAP/PUSH corresponding to NHLFE's <c>ID</c>
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Optional: an outcoming label of a package, non-null only after completing last NHLFE <c>Action</c>.
        /// </summary>
        public short? OutLabel { get; set; }
        
        /// <summary>
        /// Optional: an outcoming port of a package; non-null only after completing last NHLFE <c>Action</c>
        /// </summary>
        public ushort? OutPort { get; set; }

        /// <summary>
        /// Optional: a "pointer" to a next NHLFE entry with a given <c>ID</c>.
        /// </summary>
        public int? NextID { get; set; }

        /// <summary>
        /// Name of a router to which an NHLFE entry belogs to.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// ID | Action | OutLabel | OutPort | NextID
        /// </summary>
        public NHLFETableRow(string serializedString) : base(serializedString)
        {
            var parts = serializedString.Split(null);

            ID = int.Parse(parts[0]);
            Action = parts[1];

            if (parts[2] == "-")
            {
                OutLabel = null;
            }
            else
            {
                OutLabel = short.Parse(parts[2]);
            }

            if (parts[3] == "-")
            {
                OutPort = null;
            }
            else
            {
                OutPort = ushort.Parse(parts[3]);
            }

            if (parts[4] == "-")
            {
                NextID = null;
            }
            else
            {
                NextID = int.Parse(parts[4]);
            }
        }

        /// <summary>
        /// Serializes object into a string containing all of its public variables.
        /// </summary>
        /// <returns>Serialized object as a string.</returns>
        public override string Serialize()
        {
            var nextID = NextID?.ToString() ?? "-";
            var outPort = OutPort?.ToString() ?? "-";
            var outLabel = OutLabel?.ToString() ?? "-";
            return $"{ID} {Action} {outLabel} {outPort} {nextID}";
        }

        /// <summary>
        /// Converts a whole NHLFE entry to a string; used in logs.
        /// </summary>
        /// <returns>Converted NHLFE entry to a string.</returns>
        public override string ToString()
        {
            var nextID = NextID?.ToString() ?? "-";
            var outPort = OutPort?.ToString() ?? "-";
            var outLabel = OutLabel?.ToString() ?? "-";

            return $"{nameof(ID)}: {ID}, {nameof(Action)}: {Action}, {nameof(OutLabel)}: {outLabel}, {nameof(OutPort)}: {outPort}, {nameof(NextID)}: {nextID}";
        }

        /// <summary>
        /// Generates a hashcode of the entry.
        /// </summary>
        /// <returns>Generated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ID * 397) ^ 4;
            }
        }

        /// <summary>
        /// Checks if two NHLFE entries are equal.
        /// </summary>
        /// <param name="other">An NHLFE entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        protected bool Equals(NHLFETableRow other)
        {
            return ID == other.ID && Action == other.Action && OutLabel == other.OutLabel && OutPort == other.OutPort && NextID == other.NextID;
        }

        /// <summary>
        /// Checks if two NHLFE entries are equal.
        /// </summary>
        /// <param name="obj">An NHLFE entry we are comparing our entry to.</param>
        /// <returns>True, if entries are equal; false - otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NHLFETableRow)obj);
        }
    }
}
