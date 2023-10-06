using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Sequences;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.ForHttp20;
using IziHardGames.Libs.HttpCommon.Common;
using IziHardGames.Libs.HttpCommon.Info;
using IziHardGames.Libs.Streams;
using static IziHardGames.Libs.Cryptography.Tls12.TlsConnection12;
using EnumHttp11 = IziHardGames.Libs.ForHttp11.Maps.EnumerabableFromBufferForHttp11;
using EnumHttp20 = IziHardGames.Libs.ForHttp20.Maps.EnumerabableFromBufferForHttp20;


namespace IziHardGames.Libs.HttpCommon.Recording
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
            //string dir = "C:\\Builds\\DecodingProxy\\Records\\s1";
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
                record.datasRaw = new ReadOnlyMemory<byte>(buffer, 0, readed);
                record.filename = file.Name.Remove(file.Name.Length - file.Extension.Length);
                Console.WriteLine(file.Name);
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
            /// <summary>
            /// Data from file with <see cref="Delimeter"/>
            /// </summary>
            public ReadOnlyMemory<byte> datasRaw;
            public ReadOnlyMemory<byte> datas;
            public byte[] source;
            public Guid guid;
            public DateTime dateTime;
            public int index;
            public string filename;
            public string ext;
            public byte[] datasBytes;

            public void Analyz()
            {
                string[] splits = filename.Split(' ');
                index = int.Parse(splits[0]);
                guid = Guid.Parse(splits[1]);
            }

            public void AnalyzStage2()
            {
                Console.WriteLine($"Begin analyz stage 2: {filename}{ext}");
                var mem = datasRaw;
                int length = mem.Length;

                while (length > 0)
                {
                    Delimeter del = BufferReader.ToStruct<Delimeter>(mem.Slice(0, 16));
                    //Console.WriteLine($"{del.ToStringInfo()}");
                    delimeters.Add(del);
                    mem = mem.Slice(16);
                    var slice = mem.Slice(0, del.length);
                    slices.Add(slice);
                    mem = mem.Slice(del.length);
                    length -= (16 + slice.Length);
                }
                var seqExample = new ReadOnlySequence<byte>(datasRaw);
                var sequence = SequenceFactory.FromEnumerable(slices);
                var lengthSeq = sequence.Length;
                var span = sequence.FirstSpan;
                datasBytes = sequence.ToArray();
                datas = datasBytes.AsMemory();
            }

            public void Dispose()
            {
                ArrayPool<byte>.Shared.Return(source);
                source = default;
                datasRaw = default;
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
                if (temp1.ext == ".writer")
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

                if (temp2.ext == ".writer")
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
                BuildRequests();
                BuildResponses();
            }

            private void BuildResponses()
            {
                throw new NotImplementedException();
            }

            private void BuildRequests()
            {
                var req = entry.agentRequests;
                var sliceRequest = req.datas;
                var memRaw = req.datas;
                string s = Encoding.UTF8.GetString(req.datas.Span.Slice(0, 100));
                Console.WriteLine(s);
                var header = ReaderHttpBlind.FindHeadersHttp11(in sliceRequest);
                sliceRequest = sliceRequest.Slice(header.Length);

                if (ReaderHttpBlind.TryReadStartLine(in header, out StartLineReadResult result))
                {
                    Console.WriteLine($"Start Line: {result.ToStringInfo()}");
                    var flags = result.flags;
                    if (flags.HasFlag(EStartLine.MethodConnect))
                    {
                        var copyReader = sliceRequest;
                        Console.WriteLine($"Client Begin Raw frames");
                        while (TlsConnection12.TryParse(ref copyReader, out ParseResult parseResult))
                        {
                            if (copyReader.Length == 0)
                            {
                                Console.WriteLine("Reached Zero");
                            }
                            Console.WriteLine(parseResult.ToStringInfo() + $" LEFT:{copyReader.Length}");
                        }
                        Console.WriteLine($"Client ended");

                        var clientHello = TlsConnection12.ParseClientHello(ref sliceRequest, out ReadOnlyMemory<byte> payload0);
                        var clientKeyExchange = TlsConnection12.ParseClientKeyExchange(ref sliceRequest, out ReadOnlyMemory<byte> payload2);
                        var clientChangeCipherSpec = TlsConnection12.ParseClientChangeCipherSpec(ref sliceRequest, out ReadOnlyMemory<byte> payload3);
                        var clientHandshakeFinished = TlsConnection12.ParseClientHandshakeFinished(ref sliceRequest, out ReadOnlyMemory<byte> payload4);
                        var clientApplicationData = TlsConnection12.ParseClientApplicationData(ref sliceRequest, out ReadOnlyMemory<byte> payload5);
                        var clientCloseNotify = TlsConnection12.ParseClientCloseNotify(ref sliceRequest, out ReadOnlyMemory<byte> payload6);

                        HandshakeHelloInfo handshakeInfo = BuildTlsHandshakeAsClient(in clientHello, in payload0);

                        Console.WriteLine(handshakeInfo.ToStringInfo());

                        if (handshakeInfo.isAlpnH3)
                        {
                            throw new System.NotImplementedException();
                        }
                        else if (handshakeInfo.isAlpnH2)
                        {
                            var sliceResponse = entry.agentResponses.datas;
                            X509Certificate2Collection certCollection = default;

                            var serverHello = TlsConnection12.ParseServerHello(ref sliceResponse, out var payloadServerHello);
                            if (!HandshakeHelloFromServerAnalyz.TryAnalyz(in serverHello, in payloadServerHello, out var serverHello0))
                            {

                            }

                            while (TlsConnection12.TryParse(ref sliceResponse, out ParseResult parseResult))
                            {
                                if (sliceResponse.Length == 0)
                                {
                                    Console.WriteLine("Reached Zero");
                                }
                                Console.WriteLine(parseResult.ToStringInfo() + $" LEFT:{sliceResponse.Length}");

                                if (parseResult.record.TypeRecord == ETlsTypeRecord.Handshake)
                                {
                                    if (parseResult.handsakeHeader.Type == ETlsTypeHandshakeMessage.Certificate)
                                    {
                                        certCollection = TlsParser.ParseServerCert(in parseResult, in parseResult.payload);
                                    }
                                }
                            }

                            var serverCert = certCollection[0];
                            SslServerAuthenticationOptions optionsSever = new SslServerAuthenticationOptions()
                            {
                                ServerCertificate = serverCert,
                                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                                EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11,
                                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http2, SslApplicationProtocol.Http11 },
                            };


                            var fakeStream = new StreamForReadOnlyMemory();
                            fakeStream.Initilize(memRaw);
                            // SSL traffic
                            SslStream sslStream = new SslStream(fakeStream);
                            sslStream.AuthenticateAsServer(optionsSever);

                            var preface = sliceRequest.Slice(0, ConstantsForHttp20.CLIENT_PREFACE_SIZE);
                            if (!preface.CompareWith(ConstantsForHttp20.clientPrefaceBytes))
                            {
                                throw new ArgumentOutOfRangeException("Preface not passed!");
                            }
                            sliceRequest = sliceRequest.Slice(ConstantsForHttp20.CLIENT_PREFACE_SIZE);

                            foreach (var map in new EnumHttp20(sliceRequest))
                            {
                                HttpInfoMessage msg = HttpInfoMessage.Create(map);
                                Console.WriteLine(msg.ToInfoString());
                            }
                        }
                        else if (handshakeInfo.isHttp11)
                        {

                            foreach (var map in new EnumHttp11(sliceRequest))
                            {

                            }
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                throw new System.NotImplementedException();
            }

            private HandshakeCertificateInfo BuildCertificate(in ReadOnlyMemory<byte> mem)
            {
                HandshakeCertificateInfo info = new HandshakeCertificateInfo();
                int length = info.AnalyzeAsClient(in mem);
                return info;
            }

            private HandshakeHelloInfo BuildTlsHandshakeAsClient(in HandshakeRecord clientHello, in ReadOnlyMemory<byte> payload0)
            {
                HandshakeHelloInfo info = new HandshakeHelloInfo();
                int length = info.AnalyzeAsClient(in clientHello, in payload0);
                return info;
            }
        }
    }
}