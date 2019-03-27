using System;

namespace NetworkNodes
{
    /// <summary>
    /// Base for all types of rows.
    /// </summary>
    public abstract class ITableRow
    {
        /// <summary>
        /// Constructor for a table row. Checks, if numbers of parameters is equal to number of properties.
        /// </summary>
        /// <param name="serializedString"></param>
        public ITableRow(string serializedString)
        {       
            var parts = serializedString.Split(null);
            if (GetType().GetProperties().Length - 1 != parts.Length)
            {
                throw new Exception($"Wrong number of properties in serialized string. Expected {this.GetType().GetProperties().Length - 1}, got {parts.Length}.");
            }
        }

        /// <summary>
        /// Serializes object into a string containing all of its public variables.
        /// </summary>
        /// <returns>Serialized object as a string.</returns>
        public abstract string Serialize();
    }
}
