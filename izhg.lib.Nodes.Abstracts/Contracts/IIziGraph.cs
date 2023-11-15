namespace IziHardGames.Graphs.Abstractions.Lib
{
    public interface IIdProvider
    {

    }

    public interface IIziNode
    {

    }
    public interface IIziEdge
    {

    }

    public interface IIziGraph
    {

    }
    public interface IIziNodesRegistry<T> where T : IIziNode
    {
        T this[int id] { get; set; }
        void Add(T node);
    }

    public interface IIziNodesNavigator
    {

    }

    public interface IIziNodesRelations
    {

    }

    public interface IIziNodesAssociationsStore
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
    /// State-machine to advance through <see cref="IIziGraph"/> or <see cref="IIziNode"/>.<br/>
    /// Имплементация объекта который будет обходить граф или двигаться по цепочки <see cref="IIziNode"/>
    /// </summary>
    public interface INodeAdvancer
    {
        public void FromGraph<T>(T graph) where T : IIziGraph;
        public void SetRelations<T>(T nodeRelations) where T : IIziNodesRelations;
    }

}
