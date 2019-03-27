namespace Tools
{
    /// <summary>
    /// Represents actions that LER and LSR are capable of.
    /// </summary>
    public static class LabelActions
    {
        /// <summary>
        /// Pops the outermost label from the stack
        /// </summary>
        public const string POP = "POP";
        /// <summary>
        /// Pushes a new label on top of the stack
        /// </summary>
        public const string PUSH = "PUSH";
        /// <summary>
        /// Pops the outermost label from the stack and
        /// pushes a new label on top of the stack
        /// </summary>
        public const string SWAP = "SWAP";
    }
}
