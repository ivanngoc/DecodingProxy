using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Vector = IziHardGames.Collections.Buffers.NetStd21.IziVector<IziHardGames.Pulse.Abstractions.PulseTask>;
namespace IziHardGames.Pulse.Abstractions
{
    public static class IziPulse
    {
        public readonly static PulseTaskExecutorSelector executors = new PulseTaskExecutorSelector();
        public readonly static PulseTaskSchedulerSelector schedulers = new PulseTaskSchedulerSelector();

        internal static PulseTask Task()
        {
            return new PulseTask();
        }
        internal static PulseTask Task(Action value)
        {
            throw new System.NotImplementedException();
        }
        internal static PulseTask Task(Func<Task> value)
        {
            throw new NotImplementedException();
        }
        public static T GetResult<T>(PulseTask task) => throw new System.NotImplementedException();
        public static PulseResult<T> GetResultValid<T>(PulseTask task)
        {
            throw new System.NotImplementedException();
        }
        public static PulseTask When(PulseTask t1, PulseTask t2)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Из-за природы структуры хранящееся значение может быть не актуальным. Актуальне значение всегда хранится в <see cref="PulseTaskRegistry"/> на который указывает <see cref="PulseTask.idRegistry"/>
        /// </summary>
        /// <param name="pulseTask"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static PulseTask Actual(PulseTask pulseTask)
        {
            throw new System.NotImplementedException();
        }
    }

    public readonly struct PulseResult<T>
    {
        public readonly bool isValid;
        public readonly T value;
    }

    /// <summary>
    /// Единица для идентификации некоторой задачи. По ней можно ассоциированные с задачей данные. Сам тип легковесный и аналогичен <see cref="Task"/> и <see cref="ValueTask"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct PulseTask
    {
        public const ushort ID_ERROR = 0;
        public const ushort ID_COMPLETED = 1;
        public const ushort ID_CANCELLED = 2;
        public const ushort ID_FAILED = 3;

        /// <summary>
        /// 0 = means somethings go wrong <see cref="ID_ERROR"/>.<br/>
        /// 1 = means always completed task <see cref="ID_COMPLETED"/>
        /// 2 = <see cref="ID_CANCELLED"/><br/>
        /// 3 = <see cref="ID_FAILED"/><br/>
        /// </summary>
        [FieldOffset(0)] public readonly ushort id;
        [FieldOffset(2)] public readonly byte idRegistry;
        /// <summary>
        /// 0-7 <see cref="TaskStatus"/>. Далее свои статусы
        /// </summary>
        [FieldOffset(3)] public readonly byte status;
    }

    [StructLayout(LayoutKind.Explicit, Size = 1)]
    internal readonly struct Monada
    {
        [FieldOffset(0)] public readonly byte value;
    }

#if DEBUG
    public static class TestPulse
    {
        public static async Task Test()
        {
            var t1 = IziPulse.Task(async () => await Task.Delay(1100)).Then();
            var t2 = IziPulse.Task(() => { Console.WriteLine("Something"); });
        }
    }
#endif

    public static class ExtensionsForPulseTaskStd
    {
        public static PulseTask Merge(this in PulseTask pulseTask, PulseTask with)
        {
            throw new System.NotImplementedException();
        }
        public static PulseTask Then(this in PulseTask pulseTask)
        {
            return pulseTask;
        }
    }

    public sealed class PulseTaskSchedulerSelector
    {

    }

    public sealed class PulseTaskExecutorSelector
    {

    }

    /// <summary>
    /// аналогично <see cref="TaskScheduler"/>
    /// </summary>
    public abstract class PulseTaskScheduler
    {

    }

    public abstract class PulseTaskExecutor
    {

    }
    public abstract class PulseTaskRegistry
    {
        public readonly Vector vector = new Vector();
    }
}
