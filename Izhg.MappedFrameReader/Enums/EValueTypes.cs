namespace IziHardGames.MappedFrameReader
{
    internal enum EValueTypes
    {
        None,
        Byte,
        /// <summary>
        /// Array of Bytes
        /// </summary>
        Slice,
        Uint8,
        Uint16,
        Uint32,
        Vector,
        String,
    }
}