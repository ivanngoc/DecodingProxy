#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace IziHardGames.Libs.DevTools
{
    public static class PerfMetr
    {
        private static List<Record> logs = new List<Record>();
        private static Stopwatch stopwatch = new Stopwatch();
        private static readonly object lok = new object();

        static PerfMetr()
        {
            stopwatch.Start();
        }

        public static void ReportTimeStackTrace(string msg)
        {
            StackTrace stackTrace = new StackTrace(true);
            var trace = Environment.StackTrace;
        }
        public static void ReportTime(int source, string msg)
        {
            lock (lok)
            {
                logs.Add(new Record(source, $"{source} {stopwatch.ElapsedMilliseconds} {msg}"));
            }
        }
        public static void ReportTime(object source, string msg)
        {
            lock (lok)
            {
                logs.Add(new Record(source, $"{source.ToString()} {stopwatch.ElapsedMilliseconds} {msg}"));
            }
        }

        public static void PrintToFile()
        {
            string filenameData = $"perfmetr.txt";
            var fs = File.Open(filenameData, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            for (int i = 0; i < logs.Count; i++)
            {
                fs.Write(Encoding.UTF8.GetBytes(logs[i].msg + Environment.NewLine));
            }
            fs.Dispose();
        }
        public static void PrintToFiles(object filter)
        {
            throw new System.NotImplementedException();
        }

        private readonly struct Record
        {
            public readonly string msg;
            public readonly object source;

            public Record(object source, string msg) : this()
            {
                this.msg = msg;
            }
            public override string ToString()
            {
                return msg;
            }
        }
    }
}
#endif