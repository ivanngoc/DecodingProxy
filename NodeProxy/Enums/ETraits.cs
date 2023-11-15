using System;
using IziHardGames.Graphs.Abstractions.Lib;

namespace IziHardGames.NodeProxies.Nodes
{
    [Flags]
    internal enum ETraits : int
    {
        Error = -1,
        ErrorNotOverrided = 0,
        /// <summary>
        /// Run on another thread and live until kill
        /// </summary>
        Sustainable = 1 << 1,
        Async = 1 << 2,
        sync = 1 << 3,
        Active = 1 << 4,
        Passive = 1 << 5,
        Changing = 1 << 6,

        /// <summary>
        /// There must be at least One Node followed
        /// </summary>
        Next,
        /// <summary>
        /// There must be at least one Node Before
        /// </summary>
        Previous,
        /// <summary>
        /// Need To call <see cref="IIziNodeAdvancingAdapter"/>
        /// </summary>
        FindAdvancing = 1 << 4,



        /// <summary>
        /// Стартовый или конечный узел который либо создает либо утилизирует фрагмент
        /// </summary>
        FragmentDeadend = 1 << 11,
        FragmentOut = 1 << 12,
        FragmentIn = 1 << 13,

        /// <summary>
        /// Отдает/Проталкивает фрагмент в следующий узел
        /// </summary>
        FragmentPut = FragmentOut | Active | Next,
        /// <summary>
        /// Отдается/"Вскрывается"
        /// </summary>
        FragmentPush = FragmentOut | Async | Passive,
        /// <summary>
        /// Вытягивает/Выдать
        /// </summary>
        FragmentPops = FragmentIn | Async | Active,
        /// <summary>
        /// Вытягивается
        /// </summary>
        FragmentPull = FragmentIn | Passive | Previous,


        FragmentCreating = FragmentDeadend | FragmentOut,
        FragmentDestroying = FragmentDeadend | FragmentIn,
        /// <summary>
        /// Изменение содержимого фрагмента
        /// </summary>
        FragmentEdititng = FragmentIn | FragmentDeadend | FragmentOut | Changing,

        /// <summary>
        /// Supress Node transfering. Give <see cref="DataFragment"/> to third party. Node Implements async method of renting. When renting comes to the end awaiting completes and <see cref="DataFragment"/> becomes free of suspending
        /// </summary>
        FragmentRenting = FragmentOut | Passive | Async,

        /// <summary>
        /// Node need source to peek <see cref="DataFragment"/>. Source must have <see cref="FragmentShowing"/> trait
        /// </summary>
        FragmentPeeking = 1 << 6,
        /// <summary>
        /// Node allow to peek its <see cref="DataFragment"/>. Consumer must have <see cref="FragmentPeeking"/> trait
        /// </summary>
        FragmentShowing = 1 << 7,
        /// <summary>
        /// Узел изменяет <see cref="DataFragment.owner"/>
        /// </summary>
        FragmentAffectOwnership = FragmentTransfer | Changing,
        /// <summary>
        /// Узел является частью потока фрагментов. Передает и/или Принимает.
        /// </summary>
        FragmentTransfer = 1 << 9,

        /// <summary>
        /// Takeout <see cref="DataFragment"/> and give it to target, that change <see cref="DataFragment.owner"/> to self
        /// <see cref="IFragTakable"/>
        /// </summary>
        FragmentGiving = FragmentAffectOwnership | FragmentPush,
        /// <summary>
        /// 
        /// </summary>
        FragmentTaking = FragmentAffectOwnership | FragmentPops,
    }

    [Flags]
    internal enum EDynamicStates : int
    {
        All = -1,
        None = 0,
        Demux = 1 << 0,
        Mux = 1 << 1,
        AwaitNextNode = 1 << 2,
    }
}
