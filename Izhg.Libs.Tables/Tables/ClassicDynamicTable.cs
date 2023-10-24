using IziHardGames.Libs.Tables.Abstractions;
using IziHardGames.Libs.Tables.Contracts;
using System.Collections.Generic;

namespace IziHardGames.Libs.Tables.Classic
{
    public class ClassicDynamicTable : IDynamicTable
    {
        protected TableRaw[] raws;
        protected TableColumn[] columns;
        protected TableCell[] cells;
        protected TableController controller;
        protected readonly List<TableModifier> modifiers = new List<TableModifier>();
        public TableController Controller => controller;

        public void SetAsJoinable()
        {
            CellJoin cellJoin = new CellJoin();
            modifiers.Add(cellJoin);
        }
    }
    public class TableLine
    {
        protected CellIndexer indexer;
        public TableCell this[int index] { get => indexer[index]; set => indexer[index] = value; }
    }

   
   
    public class CellIndexer
    {
        public TableCell this[int index]
        {
            get => Get(); set => Set(value);
        }
        protected virtual void Set(TableCell value)
        {
            throw new System.NotImplementedException();
        }
        protected virtual TableCell Get()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Соединения ячеек
    /// </summary>
    public class TableJoint
    {
        public TableCellWrap head;
    }


    public class TableCellWrap
    {
        public CellIndexer value;
        public TableCellWrap tail;
    }
}
