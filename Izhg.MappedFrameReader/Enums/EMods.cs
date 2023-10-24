namespace IziHardGames.MappedFrameReader
{
    [Flags]
    internal enum EMods
    {
        None = 0,
        Repeat = 1 << 0,
    }

    internal enum ERepeatMode
    {
        None,
        Once,
        /// <summary>
        /// Perform Advance-Compare-Repeat scheme.
        /// </summary>
        WhileCondition,
        /// <summary>
        /// Repeat specified times
        /// </summary>
        Fixed,
        /// <summary>
        /// Get/Read Value which specify repeating times
        /// </summary>
        FixedFromSource,
        Func,
    }
}