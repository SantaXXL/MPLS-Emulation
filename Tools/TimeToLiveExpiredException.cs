using System;

namespace Tools
{
    /// <summary>
    /// This class is used to throw an exception when TTL decrements to 0.
    /// </summary>
    public class TimeToLiveExpiredException : Exception
    {
    }
}
