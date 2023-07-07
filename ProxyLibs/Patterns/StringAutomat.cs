using IziHardGames.Libs.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs
{
    /// <summary>
    /// Автомат для выполнения команд из потока. Формат передачи данных определяет управляющие слова и следующий за ней поток данных.
    /// </summary>
    public class StringAutomat<T>
    {
        public const int KEYWORD_MAX_SIZE = 128;
        public Dictionary<int, T> actions = new Dictionary<int, T>();

        public T this[string key] => throw new System.NotImplementedException();
        public T this[Span<char> key] => throw new System.NotImplementedException();
        public T this[Span<byte> key] => throw new System.NotImplementedException();

        public void Write(byte[] buffer, int offset, int count)
        {
            Span<char> keyword = stackalloc char[KEYWORD_MAX_SIZE];
            int length = default;
            int end = offset + count;

            for (int i = offset; i < end; i++)
            {
                keyword[length] = (char)buffer[i];
                // наращивать длину управляющего слова до тех пор пока не будет найдено совпадение
                if (actions.TryGetValue(HashIndex.GetHash(keyword.Slice(0, length)), out T action))
                {
                    if (action is Action actionDelegate) actionDelegate.Invoke();
                    return;
                }
                length++;
            }
        }

    }
}
