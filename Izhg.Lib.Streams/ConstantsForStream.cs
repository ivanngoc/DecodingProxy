namespace IziHardGames.Libs.Streams
{
    public class ConstantsForStream
    {
        public class Timeouts
        {
            /// <summary>
            /// Время оттдышки чтения в буфер если он заполнен
            /// </summary>
            public const int DEFAULT_TIMEOUT_BUFFER_FILLED = 200;
            public const int DEFAULT_ZERO_READ_TIMEOUT = 100;
            public const int TIMES_ZERO_READ = 50;
        }
    }
}
