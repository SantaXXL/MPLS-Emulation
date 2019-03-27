using System.Collections.Generic;
using System.Linq;
using Tools;

namespace NetworkNodes
{
    /// <summary>
    /// Represents an entry in ILM table.
    /// </summary>
    public class IlmTableRow : ITableRow
    {
        /// <summary>
        /// Router's port that received an incoming package.
        /// </summary>
        public ushort IncPort { get; set; }

        /// <summary>
        /// The lowest level label that will decide which
        /// ILM entry should be chosen. For more information, see example.
        ///</summary>
        /// <example>For instance, imagine such scenario: there are 2 incoming tunnels to 
        /// one LSR's port.
        /// One tunnel has only one level and label 17.
        /// The other one has two levels, the inner label 17 and the outermost 30.
        /// For (incPort, incLabel) = (incPort, 30) the action is POP. Hence,
        /// we have 2 DIFFERENT labels numbered 17 that we MUST somehow distuinguish
        /// from each other. The solution for this problem is to create a 
        /// <c>PoppedLabelStack</c> variable, which will store popped labels.
        /// To sum up: when package with label 17 is received, 
        /// <c>IncLabel</c> is 17 and <c>PoppedLabelStack</c> is NULL.
        /// When package with label 30 is received, 
        /// <c>IncLabel</c> is 30 and <c>PoppedLabelStack</c> is NULL.
        /// After popping 30, the next NHLFE is being looked up in ILM table.
        /// And the following entry will match: 
        /// <c>IncLabel</c>=17, <c>PoppedLabelStack</c>=30.</example>
        public int IncLabel { get; set; }

        /// <summary>
        /// "Pointer" to NHLFE entry.
        /// </summary>
        public int NHLFE_ID { get; set; }

        /// <summary>
        /// Name of the router that has ILM table.
        /// </summary>
        public string RouterName { get; set; }

        /// <summary>
        /// Stack of popped labels in given LSR; for more information take
        /// a look at <c>IncPort</c> description. The latest popped label
        /// will be added at the top of the stack.
        /// </summary>
        /// <seealso cref="IncPort"/>
        public string PoppedLabelStack { get; set; }

        /// <summary>
        /// Creates a row from an entry like this:
        /// IncPort | IncLabel | NHLFE_ID | poppedLabel1;poppedLabel2;...
        /// </summary>
        public IlmTableRow(string serializedString) : base(serializedString)
        {
            var parts = serializedString.Split(null);
            IncPort = ushort.Parse(parts[0]);
            IncLabel = int.Parse(parts[1]);
            PoppedLabelStack = parts[2].Replace(';', ',');
            NHLFE_ID = int.Parse(parts[3]);
        }

        /// <summary>
        /// Serializes data from ILM entry to a string.
        /// </summary>
        /// <returns>Serialized string.</returns>
        public override string Serialize()
        {
            return $"{IncPort} {IncLabel} {PoppedLabelStack} {NHLFE_ID}";
        }

        /// <summary>
        /// Compares whether two ILM entries are identical.
        /// </summary>
        /// <param name="other">Second ILM entry.</param>
        /// <returns>True, if are identical; false, if otherwise.</returns>
        protected bool Equals(IlmTableRow other)
        {
            return IncPort == other.IncPort && IncLabel == other.IncLabel && NHLFE_ID == other.NHLFE_ID && PoppedLabelStack.Equals(other.PoppedLabelStack);
        }

        /// <summary>
        /// Overrides built-in <c>Equals</c> method. Compares, if given two
        /// objects are identical.
        /// </summary>
        /// <param name="obj">Object to compare ILM entry with.</param>
        /// <returns>True, if compared object is this or if data in object and this
        /// are identical; false, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IlmTableRow)obj);
        }

        /// <summary>
        /// Calculates and returns a hashcode, based on data from ILM entry.
        /// </summary>
        /// <returns>Calculated hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IncPort.GetHashCode();
                hashCode = (hashCode * 397) ^ IncLabel;
                hashCode = (hashCode * 397) ^ NHLFE_ID;
                return hashCode;
            }
        }

        /// <summary>
        /// Converts ILM entry data to a string; used in logs.
        /// </summary>
        /// <returns>Data from ILM entry as a string.</returns>
        public override string ToString()
        {
            return $"{nameof(IncPort)}: {IncPort}, {nameof(IncLabel)}: {IncLabel}, {nameof(PoppedLabelStack)}: {PoppedLabelStack}, {nameof(NHLFE_ID)}: {NHLFE_ID}";
        }
    }
}
