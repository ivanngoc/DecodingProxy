using System.IO.Pipelines;

namespace IziHardGames.Libs.Pipelines
{
    public static class SharedPipes
    {
        public readonly static PipeOptions pipeOptions = new PipeOptions();

        static SharedPipes()
        {
            pipeOptions = new PipeOptions(  pool: null,
                                            readerScheduler: null,
                                            writerScheduler: null,
                                            pauseWriterThreshold: -1,
                                            resumeWriterThreshold: -1,
                                            minimumSegmentSize: (1 << 10) * 32,
                                            useSynchronizationContext: false);
        }
    }
}