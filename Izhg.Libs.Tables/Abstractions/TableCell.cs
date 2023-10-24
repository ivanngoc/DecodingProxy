using IziHardGames.Libs.Tables.Contracts;

namespace IziHardGames.Libs.Tables.Abstractions
{
    public abstract class TableCell : ITableCell
    {
        public virtual void SetValue<T>(T value)
        {
            throw new System.NotImplementedException();
        }
    }
}
