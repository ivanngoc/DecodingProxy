using System;

namespace IziHardGames.Libs.XmlGraph
{
    public interface IXmlStateMachine
    {

    }
    public interface IXmlElement
    {

    }
    public interface IXmlGraph
    {

    }
    public interface IXmlNode
    {

    }
    public interface IXmlEdge
    {

    }

    public enum EElement
    {
        None,
        /// <summary>
        /// Simple Node
        /// </summary>
        Node,
        /// <summary>
        /// Operator. Inside that element variants of <see cref="Case"/> is located based On condition
        /// </summary>
        Switch,
        /// <summary>
        /// Contains condition. If Condition is match than this Branch Selected
        /// </summary>
        Case,
        /// <summary>
        /// Starting <see cref="Node"/> 
        /// </summary>
        Head,
        /// <summary>
        /// Демультиплексор. 1 вход и много выходов
        /// Параллельные независимые ноды.
        /// </summary>
        Demux,
        /// <summary>
        /// Мультиплексор
        /// </summary>
        Mux,
        /// <summary>
        /// Сопоставляет множество входов с множеством выходов. Кол-во входов и выходнов одинаково
        /// </summary>
        Commutator,
    }
}
