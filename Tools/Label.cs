using System;
using System.Collections.Generic;

namespace Tools
{
    /// <summary>
    /// Represents an MPLS label.
    /// </summary>
    public class Label
    {
        /// <summary>
        /// Label's number (local ID).
        /// </summary>
        public short Number { get; set; }

        /// <summary>
        /// Time To Live field - determines how many hops are left.
        /// Note: TTL is being examined only at the outermost label.
        /// </summary>
        public byte TTL { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Label()
        {
        }

        /// <summary>
        /// Creates a label with a given <c>number</c>.
        /// </summary>
        /// <param name="number">Label's local identifier</param>
        public Label(short number)
        {
            Number = number;
        }

        /// <summary>
        /// Converts stream of bytes to a valid MPLs label.
        /// </summary>
        /// <param name="bytes">Stream of bytes to be converted to label</param>
        /// <returns>A valid label.</returns>
        public static Label FromBytes(byte[] bytes)
        {
            Label label = new Label();

            label.Number = (short)(bytes[1] + bytes[0] << 8);
            label.TTL = bytes[2];

            return label;
        }

        /// <summary>
        /// Converts label to a stream of bytes.
        /// </summary>
        /// <returns>A stream of bytes.</returns>
        public List<byte> GetBytes()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Number));
            bytes.Add(TTL);

            return bytes;
        }

        /// <summary>
        /// Checks if two labels have identical numbers.
        /// </summary>
        /// <param name="other">The label we are comparing a label to.</param>
        /// <returns>True, if numbers are equal, false - if otherwise.</returns>
        protected bool Equals(Label other)
        {
            return Number == other.Number;
        }

        /// <summary>
        /// Checks if two labels are identical.
        /// </summary>
        /// <param name="obj">The label we are comparing a label to.</param>
        /// <returns>True, if labels are equal, false - if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Label)obj);
        }

        /// <summary>
        /// Gets hashcode of <c>Number</c> variable.
        /// </summary>
        /// <returns>Haschode of <c>Number</c> variable.</returns>
        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }
    }
}
