namespace IziHardGames.MappedFrameReader
{
    internal enum ENodeType
    {
        None,
        Begin,
        SchemeBegin,
        SchemeEnd,
        FrameBegin,
        FrameEnd,
        BeginItem,
        BeginTypeDefenition,
        BeginField,
        ItemBegin,
        ReadFromSource,
        ReadFromLinkedValue,
        Condition,
        ReadCondition,
        SwitchBegin,
        SwitchItemBegin,
        SwitchItemEnd,
        SwitchEnd,
        Repeat,
        Compare,
        Result,
        End,
    }
}