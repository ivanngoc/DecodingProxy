using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.ForHttp.Common;
using IziHardGames.Libs.Streams;

namespace IziHardGames.Libs.ForHttp.Recording
{
    public class HttpRecordAnalyzer
    {
        private readonly List<Record> records = new List<Record>();
        private readonly List<Timeline> timelines = new List<Timeline>();

        public async Task Run()
        {
            await LoadFiles();
            Analyz();
        }

        private void Analyz()
        {
            foreach (var item in records)
            {
                item.AnalyzStage2();
            }
            var ids = records.Select(x => x.guid).Distinct().ToArray();

            foreach (var id in ids)
            {
                var items = records.Where(x => x.guid == id).OrderBy(x => x.index).ToArray();
                Entry entry = new Entry(items);
                var timeline = DecodeMessagesAndBuildTimeline(entry);
                timelines.Add(timeline);
            }
        }

        private Timeline DecodeMessagesAndBuildTimeline(Entry entry)
        {
            Timeline timeline = new Timeline(entry);
            throw new System.NotImplementedException();
        }

        private async Task LoadFiles()
        {
            string dir = "C:\\Builds\\DecodingProxy\\Records";
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileInfo[] files = dirInfo.GetFiles();

            foreach (var file in files)
            {
                Record record = new Record();
                var fs = file.OpenRead();
                byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
                int readed = await fs.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                record.source = buffer;
                record.datas = new ReadOnlyMemory<byte>(buffer, 0, readed);
                record.filename = file.Name;
                record.ext = file.Extension;
                record.Analyz();
                records.Add(record);
            }
        }


        /// <summary>
        /// 11 cbb272c3-da27-4f6c-8ef9-cd3d40b173a4.writer
        /// </summary>
        private class Record : IDisposable
        {
            public List<Delimeter> delimeters = new List<Delimeter>();
            public List<ReadOnlyMemory<byte>> slices = new List<ReadOnlyMemory<byte>>();
            public ReadOnlyMemory<byte> datas;

            public byte[] source;
            public Guid guid;
            public DateTime dateTime;
            public int index;
            public string filename;
            public string ext;

            public void Analyz()
            {
                string[] splits = filename.Split(' ');
                index = int.Parse(splits[0]);
                guid = Guid.Parse(splits[1]);
            }

            public void AnalyzStage2()
            {
                var mem = datas;
                int length = mem.Length;
                while (length > 0)
                {
                    Delimeter del = BufferReader.ToStruct<Delimeter>(mem.Slice(0, 16));
                    delimeters.Add(del);
                    mem = mem.Slice(16);
                    var slice = mem.Slice(0, del.length);
                    slices.Add(slice);
                    mem = mem.Slice(del.length);
                    length -= (16 + slice.Length);
                }
            }

            public void Dispose()
            {
                ArrayPool<byte>.Shared.Return(source);
                source = default;
                datas = default;
                delimeters.Clear();
                slices.Clear();
            }
        }

        private struct Entry
        {
            public Record agentRequests;
            public Record agentResponses;

            public Record originRequests;
            public Record originResponses;

            public Entry(Record[] items) : this()
            {
                var temp1 = items[0];
                if (temp1.ext == "writer")
                {
                    agentResponses = items[0];
                    agentRequests = items[1];
                }
                else
                {
                    agentResponses = items[1];
                    agentRequests = items[0];
                }
                var temp2 = items[2];

                if (temp2.ext == "writer")
                {
                    originResponses = items[2];
                    originRequests = items[3];
                }
                else
                {
                    originResponses = items[3];
                    originRequests = items[2];
                }
            }
        }

        private class Timeline
        {
            private Entry entry;
            public List<HttpMessageRecord> requests = new List<HttpMessageRecord>();
            public List<HttpMessageRecord> responses = new List<HttpMessageRecord>();
            public Timeline(Entry entry)
            {
                this.entry = entry;
                Build();
            }

            private void Build()
            {
                var req = entry.agentRequests;
                var header = ReaderHttpBlind.FindHeadersHttp11(in req.datas);


                if (ReaderHttpBlind.TryReadStartLine(in header, out StartLineReadResult result))
                {

                }
                throw new System.NotImplementedException();
            }
        }
    }
}