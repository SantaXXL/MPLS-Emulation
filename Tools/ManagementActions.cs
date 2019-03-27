namespace Tools
{
    /// <summary>
    /// A set of rules that are handled by a router.
    /// </summary>
    public static class ManagementActions
    {
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an NHLFE entry
        /// will be added to LSR's NHLFE table
        /// </summary>
        public const string ADD_NHLFE_ENTRY = "ADD_NHLFE_ENTRY";
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an NHLFE entry
        /// will be removed from LSR's NHLFE table
        /// </summary>
        public const string REMOVE_NHLFE_ENTRY = "REMOVE_NHFLE_ENTRY";

        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an FTN entry
        /// will be added to LSR's FTN table
        /// </summary>
        public const string ADD_FTN_ENTRY = "ADD_FTN_ENTRY";
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an FTN entry
        /// will be removed from LSR's FTN table
        /// </summary>
        public const string REMOVE_FTN_ENTRY = "REMOVE_FTN_ENTRY";

        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an ILM entry
        /// will be added to LSR's ILM table
        /// </summary>
        public const string ADD_ILM_ENTRY = "ADD_ILM_ENTRY";
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an ILM entry
        /// will be removed from LSR's ILM table
        /// </summary>
        public const string REMOVE_ILM_ENTRY = "REMOVE_ILM_ENTRY";

        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an MPLS-FIB entry
        /// will be added to LSR's MPLS-FIB table
        /// </summary>
        public const string ADD_MPLS_FIB_ENTRY = "ADD_MPLS_FIB_ENTRY";
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an MPLS-FIB entry
        /// will be removed from LSR's MPLS-FIB table
        /// </summary>
        public const string REMOVE_MPLS_FIB_ENTRY = "REMOVE_MPLS_FIB_ENTRY";

        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an IP-FIB entry
        /// will be added to LSR's IP-FIB table
        /// </summary>
        public const string ADD_IP_FIB_ENTRY = "ADD_IP_FIB_ENTRY";
        /// <summary>
        /// The beginning of a message sent to an LSR, indicating that an IP-FIB entry
        /// will be removed from LSR's IP-FIB table
        /// </summary>
        public const string REMOVE_IP_FIB_ENTRY = "REMOVE_IP_FIB_ENTRY";

        /// <summary>
        /// The beginning of a first message, when programs are estabilishing connection
        /// </summary>
        public const string HELLO = "HELLO";

        /// <summary>
        /// A message sent by MS to all connected LSRs, indicating that more
        /// detailed routing logs should be shown.
        /// </summary>
        public const string SHOW_MORE_ROUTING_LOGS = "SHOW_MORE_ROUTING_LOGS";
        /// <summary>
        /// A message sent by MS to all connected LSRs, indicating that less
        /// detailed routing logs should be shown.
        /// </summary>
        public const string SHOW_LESS_ROUTING_LOGS = "SHOW_LESS_ROUTING_LOGS";

        /// <summary>
        /// A message sent by MS to all connected LSRs, indicating that more
        /// detailed TTL logs should be shown.
        /// </summary>
        public const string SHOW_MORE_TTL_LOGS = "SHOW_MORE_TTL_LOGS";
        /// <summary>
        /// A message sent by MS to all connected LSRs, indicating that less
        /// detailed TTL logs should be shown.
        /// </summary>
        public const string SHOW_LESS_TTL_LOGS = "SHOW_LESS_TTL_LOGS";
    }
}
