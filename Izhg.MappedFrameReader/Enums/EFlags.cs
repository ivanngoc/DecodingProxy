namespace IziHardGames.MappedFrameReader
{
    [Flags]
    public enum EFlags : int
    {
        None,
    }

    public enum EKeyWord
    {
        None,
        Item,

    }


    internal enum EReadType
    {
        None,
        /// <summary>
        /// Stream означает что данные для продвижения приходят последовательно. По мере прихода этих данных, например, может происходить продвижение на основе пришедших данных (на указанный размер). 
        /// Интерпретация может происходить по мере прихода данных на лету
        /// </summary>
        Stream,
        /// <summary>
        /// NonStream означает что нужно дождаться полностью кадра а лишь затем создавать связи и анализировать данные
        /// </summary>
        NonStream,
    }
}