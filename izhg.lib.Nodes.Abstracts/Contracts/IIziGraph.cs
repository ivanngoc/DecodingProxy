namespace IziHardGames.Graphs.Abstractions.Lib
{
    public interface IIziNode
    {

    }
    public interface IIziEdge
    {

    }

    public interface IIziGraph
    {

    }

    /// <summary>
    /// Объект который хранит сопоставления с <see cref="IIziNode"/> и ассоциированными данными (объект определенного типа / набор объектв и т.д.)
    /// </summary>
    public interface IIziNodesAssociations
    {

    }

    /// <summary>
    /// Если граф строит продолжение динамически то нужен объект, релизующий этот интерфейс, 
    /// который в зависимости от текущего состояния графа и прописанных в конкретной реализации правил решать какой узел вставить следующим
    /// </summary>
    public interface IIziNodeAdvancingAdapter
    {

    }
    /// <summary>
    /// Объект который сопоставляет текущий <see cref="IIziNode"/> c подходящим <see cref="IIziNodeAdvancingAdapter"/>
    /// </summary>
    public interface IIziNodeAdvancingSearcher
    {

    }
    /// <summary>
    /// State-machine to advance through <see cref="IIziGraph"/> or <see cref="IIziNode"/>
    /// </summary>
    public interface INodeIterator
    {

    }

}
