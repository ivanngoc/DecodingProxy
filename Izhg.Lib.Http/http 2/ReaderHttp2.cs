using System;
using System.Text;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.ForHttp;
using IziHardGames.Libs.ForHttp.Http20;
using IziHardGames.Libs.ForHttp20.DecodingHeaders;
using IziHardGames.Libs.ForHttp20.Huffman;
using static System.Net.Mime.MediaTypeNames;
using static IziHardGames.Libs.ForHttp.Http20.HeadersFrame;
using static IziHardGames.Libs.ForHttp20.ConstantsForHttp20;

namespace IziHardGames.Libs.ForHttp20
{
    public class ReaderHttp2 : EHttpReader
    {
        public static void Test()
        {
            byte[] input = new byte[] { 
                // Preface
                0x50, 0x52, 0x49, 0x20, 0x2A, 0x20, 0x48, 0x54,
                0x54, 0x50, 0x2F, 0x32, 0x2E, 0x30, 0x0D, 0x0A,
                0x0D, 0x0A, 0x53, 0x4D, 0x0D, 0x0A, 0x0D, 0x0A,
                // ==========================
                // SETTINGS FRAME
                0x00, 0x00, 0x12,   // Length = 18
                0x04,               // Type
                0x00,               // Flags
                0x00, 0x00, 0x00,0x00,  // Stream Identifier
                // SETTINGS PAYLOAD
                //Setting 1
                0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 
                //Setting 2
                0x00, 0x04, 0x00, 0x02, 0x00, 0x00, 
                //Setting 3
                0x00, 0x05, 0x00, 0x00, 0x40, 0x00, 
                // ==========================
                // WINDOW_UPDATE Frame
                0x00, 0x00, 0x04,   //length 4
                0x08,               //WINDOW_UPDATE
                0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0xBF, 0x00, 0x01, 
                // ==========================
                // Frame
                0x00, 0x00, 0x05,       // length 5
                0x02,                   //PRIORITY
                0x00,
                0x00, 0x00, 0x00, 0x03,
                0x00, 0x00, 0x00, 0x00, 0xC8, 
                // ==========================
                0x00, 0x00, 0x05,
                0x02,
                0x00,
                0x00, 0x00, 0x00, 0x05,
                0x00, 0x00, 0x00, 0x00, 0x64, 
                // ==========================                
                0x00, 0x00, 0x05,
                0x02,
                0x00,
                0x00, 0x00, 0x00, 0x07,
                0x00, 0x00, 0x00, 0x00, 0x00, 
                // ==========================                
                0x00, 0x00, 0x05,
                0x02,
                0x00,
                0x00, 0x00, 0x00, 0x09,
                0x00, 0x00, 0x00, 0x07, 0x00, 
                // ==========================                
                0x00, 0x00, 0x05,
                0x02,
                0x00,
                0x00, 0x00, 0x00, 0x0B,
                0x00, 0x00, 0x00, 0x03, 0x00, 
                // ==========================                
                0x00, 0x00, 0x05,
                0x02,
                0x00,
                0x00, 0x00, 0x00, 0x0D,
                0x00, 0x00, 0x00, 0x00, 0xF0, 
                // ==========================
                //Frame Headers
                0x00, 0x01, 0x21,       // length 289
                0x01,
                0x25,                   // Flags 37: IsPrioritySet; IsEndHeadersSet; IsEndStreamSet
                0x00, 0x00, 0x00, 0x0F, 

                //Payload
                0x00, 0x00, 0x00, 0x07, //Stream Dependency
                0x15,                   //Weight
                //Field Block Fragment
                //+++++++++++++++++++++++++++++++
                0x82,                   //indexed header field; index=2; :method=get
                //+++++++++++++++++++++++++++++++
                0x05,                   //Literal Header Field without Indexing; Index=5; [:path]; NO name string; value string presented; 
                0xC6,                   // isHuffman; Length = 70
                0x60, 0x93, 0x8C, 0xD5, 0x21, 0x8A, 0x8F, 0x65, 0x23, 0xAA,         /// chains/normandy.content-signature.mozilla.org-2023-10-09-11-30-13.chain?cachebust=2017-06-13-21-06
                0x4F, 0x4B, 0x90, 0xF5, 0x24, 0xB5, 0x25, 0x64, 0x1A, 0x6A,
                0x86, 0x9B, 0x6C, 0x2A, 0xF4, 0x9F, 0xD9, 0xA8, 0xA0, 0x6B,
                0x9E, 0xC9, 0x96, 0x10, 0x04, 0xCA, 0xC1, 0x02, 0xC0, 0x7D,
                0x60, 0x85, 0x66, 0x40, 0xB0, 0x59, 0x5C, 0x93, 0x8C, 0xD5,

                0x7F, 0x84, 0x19, 0x27, 0x2C, 0x76, 0xA1, 0x30, 0x08, 0x01,
                0x75, 0x60, 0x38, 0xB0, 0x59, 0x58, 0x41, 0x58, 0x0E, 0x7F, 
                //+++++++++++++++++++++++++++++++                              
                0x41,                   // Literal Header Field with Incremental Indexing; index=1; [:authority]
                0x99,                   // isHuffman; length value = 25
                0x21, 0xEA, 0x49, 0x6A, 0x4A, 0xC8, 0x34, 0xD5, 0x0D, 0x36,         //content-signature-2.cdn.mozilla.net
                0xD8, 0x55, 0x84, 0xB9, 0x24, 0xA9, 0x7A, 0x4F, 0xEC, 0xD4,
                0x50, 0x35, 0xEA, 0x2A, 0x7F, 
                //+++++++++++++++++++++++++++++++
                0x87,                   // Indexed Header Field Representation; index = 7;  :scheme:https
                //+++++++++++++++++++++++++++++++
                0x7A,                   // Literal Header Field with Incremental Indexing; index=58;
                0xBC,                   // isHuffman; length = 60
                0xD0, 0x7F, 0x66, 0xA2, 0x81, 0xB0, 0xDA, 0xE0, 0x53, 0xFA,
                0xE4, 0x6A, 0xA4, 0x3F, 0x84, 0x29, 0xA7, 0x7A, 0x81, 0x02,
                0xE0, 0xFB, 0x53, 0x91, 0xAA, 0x71, 0xAF, 0xB5, 0x3C, 0xB8,
                0xD7, 0xDA, 0x96, 0x77, 0xB8, 0x10, 0x3E, 0xB8, 0x3F, 0xB5,
                0x31, 0x14, 0x9D, 0x4E, 0xC0, 0x80, 0x10, 0x00, 0x20, 0x0A,

                0x98, 0x4D, 0x61, 0x65, 0x3F, 0x96, 0x02, 0x17, 0xD7, 0x07,
                //+++++++++++++++++++++++++++++++
                0x53,                   // Literal Header Field with Incremental Indexing; index 19; accept 
                0x8B,                   // isHuffman; length=11
                0x1D, 0x75, 0xD0, 0x62, 0x0D, 0x26, 0x3D, 0x4C, 0x74, 0x41, 0xEA, // application/json                
                //+++++++++++++++++++++++++++++++                
                0x51,                   // Literal Header Field with Incremental Indexing; index=17; accept-language
                0x9C,                   // // isHuffman; length=28;
                0xB2, 0xD5, 0xB6, 0xF0, 0xFA, 0xB2, 0xDF, 0xBE, 0xD0, 0x01,         // ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3
                0x77, 0xBE, 0x8B, 0x52, 0xDC, 0x37, 0x7D, 0xF6, 0x80, 0x0B,
                0xB7, 0xF4, 0x5A, 0xBE, 0xFB, 0x40, 0x05, 0xD9, 
                //+++++++++++++++++++++++++++++++
                0x50,                   // Literal Header Field with Incremental Indexing; index=16; accept-encoding
                0x8D,                   // isHuffman; length=13
                0x9B, 0xD9, 0xAB, 0xFA, 0x52, 0x42, 0xCB, 0x40, 0xD2, 0x5F, //gzip, deflate, br
                0xA5, 0x23, 0xB3, 
                //+++++++++++++++++++++++++++++++                
                0x68,                   // Literal Header Field with Incremental Indexing; index=40; if-modified-since 
                0x96,                   // isHuffman; 22;
                0xDD, 0x6D, 0x5F, 0x4A, 0x08, 0x0A, 0x43, 0x6C, 0xCA, 0x08, // Sun, 20 Aug 2023 11:30:14 GMT
                0x02, 0x65, 0x40, 0x86, 0xE3, 0x20, 0xB8, 0x16, 0x94, 0xC5,
                0xA3, 0x7F, 
                //+++++++++++++++++++++++++++++++                
                0x69,                   // Literal Header Field with Incremental Indexing; index = 41;if-none-match
                0x9A,                   // isHuffman; 26
                0xFE, 0x5B, 0x13, 0x21, 0x69, 0xE6, 0x9E, 0x78, 0x31, 0xB2, // "5231484881b355d096afbe9bb55e85bd"
                0xDB, 0x72, 0x01, 0xF7, 0x07, 0x2C, 0x65, 0x7E, 0x38, 0xDB,
                0x6C, 0xAF, 0x37, 0x1C, 0x9F, 0xCF, 
                //+++++++++++++++++++++++++++++++                
                0x40,                   // Literal Header Field with Incremental Indexing; index=0
                0x82,                   // isHuffman; length = 2
                0x49, 0x7F,             // te
                0x86,                   // isHuffman; length = 6
                0x4D, 0x83, 0x35, 0x05, 0xB1, 0x1F, 
                // ==========================
                0x00, 0x00, 0x04,   // length = 4
                0x08,               // Type: Window Update
                0x00,
                0x00, 0x00, 0x00, 0x0F,
                0x00, 0xBE, 0x00, 0x00 };

            ReadOnlyMemory<byte> mem = input.AsMemory();
            if (mem.Length >= ConstantsForHttp20.FRAME_SIZE)
            {
                var preface = mem.Slice(0, ConstantsForHttp20.CLIENT_PREFACE_SIZE);
                if (preface.CompareWith(ConstantsForHttp20.clientPrefaceBytes))
                {
                    Console.WriteLine("Preface Passed");
                }
                mem = mem.Slice(ConstantsForHttp20.CLIENT_PREFACE_SIZE);
                // first frame after preface must be SETTINGS type
                int SETTINGS_HEADER_TABLE_SIZE = default;

                while (mem.Length > 0)
                {
                    var frame = BufferReader.ToStruct<FrameHttp20>(mem.Span);
                    int length = frame.length;
                    Console.WriteLine($"Passed. Length:{length}. Type:{frame.GetTypeName()}. flags:{frame.flags}. stream ID:{frame.streamIdentifier}");
                    mem = mem.Slice(ConstantsForHttp20.FRAME_SIZE);
                    var payload = mem.Slice(0, length);

                    switch (frame.type)
                    {
                        case FrameTypes.SETTINGS:
                            {
                                int countSettings = length / ConstantsForHttp20.FRAME_DATA_SIZE_SETTINGS;
                                var slice = payload;
                                for (int i = 0; i < countSettings; i++)
                                {
                                    var span = slice.Slice(0, ConstantsForHttp20.FRAME_DATA_SIZE_SETTINGS).Span;
                                    FrameDataSettingHttp20 dataSettings = BufferReader.ToStruct<FrameDataSettingHttp20>(span);
                                    Console.WriteLine($"FrameDataSettingHttp20: identifier {dataSettings.GetIdentifierVarName()}. value:{dataSettings.ValueBE}");
                                    slice = slice.Slice(ConstantsForHttp20.FRAME_DATA_SIZE_SETTINGS);

                                    if (dataSettings.IsHeaderTableSize())
                                    {
                                        SETTINGS_HEADER_TABLE_SIZE = dataSettings.ValueBE;
                                    }
                                }
                                break;
                            }
                        case FrameTypes.RST_STREAM:
                            {
                                break;
                            }
                        case FrameTypes.HEADERS:
                            {
                                var dynamicTable = new DynamicTable();
                                var headersSlice = payload;
                                if (Flags.IsPaddedSet(frame.flags))
                                {
                                    Console.WriteLine($"Headers Flags IsPadded");
                                }
                                if (Flags.IsPrioritySet(frame.flags))
                                {
                                    Console.WriteLine($"Headers Flags IsPriority");
                                }
                                if (Flags.IsEndHeadersSet(frame.flags))
                                {
                                    Console.WriteLine($"Headers Flags IsEndHeadersSet");
                                    ReadOnlySpan<byte> streamDependency = headersSlice.Slice(0, 4).Span;
                                    byte weigth = headersSlice.Span[4];
                                    headersSlice = headersSlice.Slice(5);
                                }
                                if (Flags.IsEndHeadersSet(frame.flags))
                                {
                                    Console.WriteLine($"Headers Flags IsEndStreamSet");
                                }


                                var stream = StreamHttp20.OpenStream();
                                Console.WriteLine($"Heders:{Environment.NewLine}{Encoding.UTF8.GetString(headersSlice.Span)}");

                                while (headersSlice.Length > 0)
                                {
                                    ReadOnlySpan<byte> bodySpan = headersSlice.Span;

                                    if (bodySpan[0] > ConstantsForHttp20.HPACK.MASK_INDEX_REVERSE) //indexed header field
                                    {
                                        int index = bodySpan[0] & ConstantsForHttp20.HPACK.MASK_INDEX_REVERSE;
                                        if (index == 0) throw new FormatException("The index value of 0 is not used. It MUST be treated as a decoding error if found in an indexed header field representation.");

                                        HeaderField20 entry = default;
                                        if (index < HPACK.LENGTH_STATIC_TABLE)
                                        {
                                            entry = StaticTable.GetEntry(index);
                                        }
                                        else
                                        {
                                            entry = dynamicTable.GetEntry(index);
                                        }
                                        Console.WriteLine($"STatic TABLE. NAME:{entry.NameAsString} Value:{entry.ValueAsString}");
                                        headersSlice = headersSlice.Slice(1);
                                    }
                                    // Literal Header Field with Incremental Indexing
                                    else if (bodySpan[0] >= ConstantsForHttp20.HPACK.PATTERN_INDEXED_HEADER_FIELD_INCREMENTAL)
                                    {
                                        int index = bodySpan[0] & ConstantsForHttp20.HPACK.MASK_INDEXED_HEADER_FIELD_INCREMENTAL;
                                        var pair = DecodeLength(bodySpan[1]);
                                        var isHuffman = pair.Item1;
                                        int lengthValue = pair.Item2;

                                        if (index != 0)
                                        {
                                            HeaderField20 entry = dynamicTable.GetEntry(index);
                                            ReadOnlySpan<byte> valueString = bodySpan.Slice(2, lengthValue);
                                            if (isHuffman)
                                            {
                                                var value = HuffmanCoding.decoder.Decode(valueString);
                                                Console.WriteLine($"{nameof(HPACK.MASK_INDEXED_HEADER_FIELD_INCREMENTAL)}. field name:[{entry.NameAsString}]; {Encoding.UTF8.GetString(value)}");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"{nameof(HPACK.MASK_INDEXED_HEADER_FIELD_INCREMENTAL)}. field name:[{entry.NameAsString}]; {Encoding.UTF8.GetString(valueString)}");
                                            }
                                            headersSlice = headersSlice.Slice(2 + lengthValue);
                                        }
                                        else
                                        {
                                            ReadOnlySpan<byte> fieldName = bodySpan.Slice(2, lengthValue);
                                            string name = default;
                                            string value = default;
                                            if (isHuffman)
                                            {
                                                name = Encoding.UTF8.GetString(HuffmanCoding.decoder.Decode(fieldName));
                                            }
                                            else
                                            {
                                                name = Encoding.UTF8.GetString(fieldName);
                                            }
                                            headersSlice = headersSlice.Slice(2 + lengthValue);
                                            var pairValue = DecodeLength(headersSlice.Span[0]);
                                            ReadOnlySpan<byte> valueSpan = headersSlice.Span.Slice(1, pairValue.Item2);
                                            if (pairValue.Item1)
                                            {
                                                value = Encoding.UTF8.GetString(HuffmanCoding.decoder.Decode(valueSpan));
                                            }
                                            else
                                            {
                                                value = Encoding.UTF8.GetString(valueSpan);
                                            }
                                            Console.WriteLine($"{nameof(HPACK.MASK_INDEXED_HEADER_FIELD_INCREMENTAL)}. field name:[{name}]; {value}");
                                            headersSlice = headersSlice.Slice(1 + pairValue.Item2);
                                        }

                                    }
                                    // Literal Header Field Never Indexed
                                    else if (bodySpan[0] >= HPACK.PATTERN_INDEXED_HEADER_FIELD_NEVER_INDEX)
                                    {
                                        throw new System.NotImplementedException();
                                    }
                                    else if ((bodySpan[0] & HPACK.PATTERN_INDEXED_HEADER_FIELD_NO_INDEX) == 0)
                                    {
                                        int index = bodySpan[0] & 0b_0000_1111;
                                        if (index != 0)
                                        {
                                            var entry = dynamicTable.GetEntry(index);
                                            int lengthValue = bodySpan[1] & 0b_0111_1111;
                                            ReadOnlySpan<byte> valueString = bodySpan.Slice(2, lengthValue);
                                            bool isHuffman = (bodySpan[1] & 0b_1000_0000) != default;
                                            if (isHuffman)
                                            {
                                                var bytes = HuffmanCoding.decoder.Decode(valueString);
                                                Console.WriteLine($"{nameof(HPACK.PATTERN_INDEXED_HEADER_FIELD_NO_INDEX)}: FieldName:[{entry.NameAsString}]; FieldValue:[{Encoding.UTF8.GetString(bytes)}]");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"{nameof(HPACK.PATTERN_INDEXED_HEADER_FIELD_NO_INDEX)}: FieldName:[{entry.NameAsString}]; FieldValue:[{Encoding.UTF8.GetString(valueString)}]");
                                            }
                                            headersSlice = headersSlice.Slice(2 + lengthValue);
                                        }
                                        else
                                        {
                                            var slice = bodySpan.Slice(1);
                                            int lengthName = slice[0] & 0b_0111_1111;
                                            ReadOnlySpan<byte> nameString = slice.Slice(1, lengthName);
                                            slice = slice.Slice(1 + lengthName);
                                            int lengthValue = slice[0] & 0b_0111_1111;
                                            ReadOnlySpan<byte> valueString = slice.Slice(1, lengthValue);
                                            Console.WriteLine($"{nameof(HPACK.PATTERN_INDEXED_HEADER_FIELD_NO_INDEX)}:[{Encoding.UTF8.GetString(nameString)}={Encoding.UTF8.GetString(valueString)}]");
                                            throw new System.NotImplementedException();
                                        }
                                    }
                                    Console.WriteLine($"Heders length left:{headersSlice.Length}");
                                }
                                break;
                            }
                        case FrameTypes.WINDOW_UPDATE:
                            {
                                break;
                            }
                        default: break;
                    }
                    mem = mem.Slice(length);
                }
                throw new NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException();
            }

            (bool, byte) DecodeLength(byte value)
            {
                bool isHuffman = (value & 0b_1000_0000) != default;
                byte length = (byte)(value & 0b_0111_1111);
                return (isHuffman, length);
            }
        }
    }
}
