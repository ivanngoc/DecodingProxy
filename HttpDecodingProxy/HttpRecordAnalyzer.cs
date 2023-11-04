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
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Recording;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.ForHttp20;
using IziHardGames.Libs.HttpCommon.Common;
using IziHardGames.Libs.HttpCommon.Info;
using IziHardGames.Libs.Streams;
using IziHardGames.MappedFrameReader;
using static IziHardGames.Libs.Cryptography.Readers.Tls12.ParserForTls12;
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
            await LoadFiles().ConfigureAwait(false);
            await Analyz().ConfigureAwait(false);
        }

        private async Task Analyz()
        {
            foreach (var item in records)
            {
                item.AnalyzStage2();
            }
            var ids = records.Select(x => x.guid).Distinct().ToArray();

            foreach (var id in ids)
            {
                var items = records.Where(x => x.guid == id).OrderBy(x => x.index).ToArray();
                if (items.Length != 4) continue;
                Entry entry = new Entry(items);
                await entry.AnalyzEntries().ConfigureAwait(false);
                var timeline = await DecodeMessagesAndBuildTimeline(entry).ConfigureAwait(false);
                timelines.Add(timeline);
            }
        }

        private async Task<Timeline> DecodeMessagesAndBuildTimeline(Entry entry)
        {
            Timeline timeline = new Timeline(entry);
            await timeline.Run().ConfigureAwait(false);
            return timeline;
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
                record.fullName = file.FullName;

                if (file.Extension == ".clear")
                {
                    string fname = file.Name;
                    string name = fname.Substring(0, fname.Length - 6);
                    var fs = file.OpenRead();
                    byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
                    int readed = await fs.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    record.datasAsBytesRaw = buffer;
                    record.datasAsMemRaw = new ReadOnlyMemory<byte>(buffer, 0, readed);
                    record.filename = name.Remove(name.Length - 7);
                    record.dir = file.Directory!.FullName;
                    Console.WriteLine(file.Name);
                    record.ext = file.Extension;
                    record.Analyz();
                    records.Add(record);
                }
                else if (false)
                {
                    string name = file.Name;
                    var fs = file.OpenRead();
                    byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
                    int readed = await fs.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    record.datasAsBytesRaw = buffer;
                    record.datasAsMemRaw = new ReadOnlyMemory<byte>(buffer, 0, readed);
                    record.filename = name.Remove(name.Length - 7);
                    record.dir = file.Directory!.FullName;
                    Console.WriteLine(file.Name);
                    record.ext = file.Extension;
                    record.Analyz();
                    records.Add(record);
                }
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
            public ReadOnlyMemory<byte> datasAsMemRaw;
            public ReadOnlyMemory<byte> datasAsMem;
            public byte[] datasAsBytesRaw;
            public byte[] datasAsBytes;
            public Guid guid;
            public DateTime dateTime;
            public int index;
            public string dir;
            public string filename;
            public string ext;
            internal string fullName;

            public void Analyz()
            {
                string[] splits = filename.Split(' ');
                index = int.Parse(splits[0]);
                guid = Guid.Parse(splits[1]);
            }

            public void AnalyzStage2()
            {
                Console.WriteLine($"Begin analyz stage 2: {filename}{ext}");
                var mem = datasAsMemRaw;
                // If file without headers
                if (ext != ".clear")
                {
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
                    var seqExample = new ReadOnlySequence<byte>(datasAsMemRaw);
                    var sequence = SequenceFactory.FromEnumerable(slices);
                    var lengthSeq = sequence.Length;
                    var span = sequence.FirstSpan;
                    datasAsBytes = sequence.ToArray();
                    File.WriteAllBytes(Path.Combine(dir, filename + ext + ".clear"), datasAsBytes);
                    datasAsMem = datasAsBytes.AsMemory();
                }
                else
                {
                    this.datasAsBytes = datasAsBytesRaw;
                    this.datasAsMem = datasAsBytes.AsMemory();
                }
            }

            public void Dispose()
            {
                ArrayPool<byte>.Shared.Return(datasAsBytesRaw);
                datasAsBytesRaw = default;
                datasAsMemRaw = default;
                delimeters.Clear();
                slices.Clear();
            }
        }

        private struct Entry
        {
            /// <summary>
            /// Данные от агента (браузера/клиента)
            /// </summary>
            public Record proxyReadFromClient;
            /// <summary>
            /// Если прокси не изменял данные то эти данные Должны совпадать с <see cref="proxyReadFromOrigin"/>
            /// </summary>
            public Record proxyWriteToClient;

            /// <summary>
            /// Если прокси не изменял данные то эти данные Должны совпадать с <see cref="proxyReadFromClient"/>
            /// </summary>
            public Record proxyWriteToOrigin;
            /// <summary>
            /// Данные от сервера
            /// </summary>
            public Record proxyReadFromOrigin;

            public Entry(Record[] items) : this()
            {
                var temp1 = items[0];
                if (temp1.ext == ".writer")
                {
                    proxyWriteToClient = items[0];
                    proxyReadFromClient = items[1];
                }
                else
                {
                    proxyReadFromClient = items[0];
                    proxyWriteToClient = items[1];
                }
                var temp2 = items[2];

                if (temp2.ext == ".writer")
                {
                    proxyWriteToOrigin = items[2];
                    proxyReadFromOrigin = items[3];
                }
                else
                {
                    proxyReadFromOrigin = items[2];
                    proxyWriteToOrigin = items[3];
                }
            }

            public async Task AnalyzEntries()
            {
                return;
                var t1 = Parse(proxyReadFromClient.datasAsBytes);
                await t1.ConfigureAwait(false);

                var t2 = Parse(proxyWriteToClient.datasAsBytes);
                await t2.ConfigureAwait(false);

                var t3 = Parse(proxyWriteToOrigin.datasAsBytes);
                await t3.ConfigureAwait(false);

                var t4 = Parse(proxyReadFromOrigin.datasAsBytes);
                await t4.ConfigureAwait(false);
            }
            private static async Task Parse(byte[] testData)
            {
                SchemeImporter importer = new SchemeImporter();
                importer.tableOfFuncs.AddAdvancingFunc($"ReadBodyHttp11", PopularFuncs.ReadBodyHttp11);
                Scheme scheme = await importer.FromFileAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Examples\\SchemeSsl.txt");
                Reader reader = new Reader(scheme);
                reader.OnResult(ReportPublisher.ReportFunc);
                await reader.ReadAllAsync(testData);
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
            }

            public async Task Run()
            {

                await BuildRequests().ConfigureAwait(false);
                BuildResponses();
            }

            private void BuildResponses()
            {

            }


            private async Task BuildRequests()
            {
                var req = entry.proxyReadFromClient;
                var proxyReadFromClientMem = req.datasAsMem;
                var proxyWriteToClientMem = entry.proxyWriteToClient.datasAsMem;
                var sliceProxyReadFromClientMem = proxyReadFromClientMem;
                var memRaw = req.datasAsMem;
#if DEBUG
                string s = Encoding.UTF8.GetString(req.datasAsMem.Span.Slice(0, 500));
                Console.WriteLine(s);
#endif
                var headerRequest = ReaderHttpBlind.FindHeadersHttp11(in sliceProxyReadFromClientMem);
                if (ReaderHttpBlind.ValidateHeadersForRequest(in headerRequest))
                {
                    sliceProxyReadFromClientMem = sliceProxyReadFromClientMem.Slice(headerRequest.Length);

                    if (ReaderHttpBlind.TryFindBody(in headerRequest, out int lengthBody))
                    {
                        sliceProxyReadFromClientMem = sliceProxyReadFromClientMem.Slice(lengthBody);
                    }

                    if (ReaderHttpBlind.TryReadStartLine(in headerRequest, out StartLineReadResult result))
                    {
                        Console.WriteLine($"Start Line: {result.ToStringInfo()}");
                        var flags = result.flags;
                        if (flags.HasFlag(EStartLine.MethodConnect))
                        {
                            if (false) // когда новый дамп сделаю
                            {
                                var headerResponse = ReaderHttpBlind.FindHeadersHttp11(in proxyWriteToClientMem);
                                if (!ReaderHttpBlind.ValidateHeadersForRequest(in headerResponse)) throw new FormatException();
                                proxyWriteToClientMem = proxyWriteToClientMem.Slice(headerResponse.Length);

                                if (ReaderHttpBlind.TryFindBody(in headerResponse, out int lengthBodyResponse))
                                {
                                    proxyWriteToClientMem = proxyWriteToClientMem.Slice(lengthBodyResponse);
                                }
                            }
                            proxyReadFromClientMem = sliceProxyReadFromClientMem;
                            TlsPlayerWithProxy tlsPlayer = new TlsPlayerWithProxy(result.Host, proxyReadFromClientMem, proxyWriteToClientMem, entry.proxyWriteToOrigin.datasAsMem, entry.proxyReadFromOrigin.datasAsMem);
                            var decryptedData = await tlsPlayer.Decrypt();
                            if (decryptedData is null) return;
                            //var session = ParseTlsHandshake(memRaw, ref sliceRequest);
                            //ParseEncryptedHttp(session, memRaw, ref sliceRequest);
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                    throw new System.NotImplementedException();
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            private void ParseEncryptedHttp(TlsSession session, ReadOnlyMemory<byte> memRaw, ref ReadOnlyMemory<byte> sliceRequest)
            {
                var serverCert = session.proxyToClient.Cert;

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