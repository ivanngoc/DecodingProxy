using System;

namespace IziHardGames.Libs.NonEngine.Memory
{
    public interface IReusingInfo
    {
        public bool IsDisposed { get; set; }
        public uint Generation { get; set; }
        public uint Deaths { get; set; }
    }

    public class ReusingInfo : IReusingInfo, IDisposable, IPoolBind<ReusingInfo>
    {
        private IPoolReturn<ReusingInfo>? pool;
        public bool IsDisposed { get; set; }
        public uint Generation { get; set; }
        public uint Deaths { get; set; }
        public void Dispose()
        {
            IsDisposed = false;
            Generation = default;
            Deaths = default;
            pool.Return(this);
            pool = default;
        }
        public void BindToPool(IPoolReturn<ReusingInfo> pool)
        {
            this.pool = pool;
        }
        public void Revive()
        {
            Generation++;
        }
        public void Die()
        {
            Deaths++;
        }

        public static ReusingInfo Get()
        {
            var pool = PoolObjectsConcurent<ReusingInfo>.Shared;
            var rent = pool.Rent();
            rent.BindToPool(pool);
            return rent;
        }
    }
}