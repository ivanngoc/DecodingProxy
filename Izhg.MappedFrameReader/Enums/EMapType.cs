namespace IziHardGames.MappedFrameReader
{
    [Flags]
    public enum EMapType : int
    {
        None,
        Payload,
        LengthSource,
        /// <summary>
        /// Based on this value, there would be branching based on Value. If this type present, than author must specified operator switch values
        /// </summary>
        TypeSelector,
        Value,
        /// <summary>
        /// Sequence of bytes specified by <see cref="ILengthSource"/>
        /// </summary>
        Vector,
    }
}