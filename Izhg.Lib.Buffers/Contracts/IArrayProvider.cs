using System;

namespace IziHardGames.Core
{
    public interface IReadOnlySpanProvider<T>
    {
        ReadOnlySpan<T> GetSpan(int offset, int length);
    }

    public interface IArrayProvider<T>
    {
        T[] AsArray();
    }
}