using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Async.Contracts
{
    public interface ITaskBased
    {

    }

    /// <summary>
    /// using  <see cref="ValueTask"/>. Optimized for lesser heap's allocations
    /// </summary>
    public interface IValueTaskBased
    {

    }
    public interface IContinuable
    {

    }
    /// <summary>
    /// Реализует ручное управление освобождения ожидания (await)
    /// </summary>
    public interface IAwaitComntrol
    {

    }
}
