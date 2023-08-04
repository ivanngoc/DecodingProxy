using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Lib.Networking.Exceptions
{
    /// <summary>
    /// Простой соединения. В течение определеннго времени не было приема/передачи данных
    /// </summary>
    public class StalledConnection : Exception
    {
        public static readonly StalledConnection shared = new StalledConnection();
    }

    public class ZeroReadException : Exception
    {
        public static readonly ZeroReadException shared = new ZeroReadException();
    }
    /// <summary>
    /// Резко оборвалась связь. во время приема или передачи. следует произвести переподключение и повторить отправку
    /// </summary>
    public class SuddenBreakException : Exception
    {
        public static readonly SuddenBreakException shared = new SuddenBreakException();
    }
    /// <summary>
    /// Не удается подключиться
    /// </summary>
    public class ConnectException : Exception
    {
        public static readonly ConnectException shared = new ConnectException();
    }
}
