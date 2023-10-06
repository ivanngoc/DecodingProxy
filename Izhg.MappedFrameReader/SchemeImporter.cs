using System;
using System.Text;
using System.Text.RegularExpressions;
using IziHardGames.Libs.RegexExpressions;
using static IziHardGames.MappedFrameReader.Scheme;
using Func = System.Func<System.ReadOnlyMemory<byte>, int>;

namespace IziHardGames.MappedFrameReader
{
    public class SchemeImporter
    {
        public List<Variables> variables = new List<Variables>();
        //Common
        private static readonly Regex balancedGroupe = new Regex(@"(?>(?>(?'open'{)[^{}]*)+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))");
        //Special
        private static readonly Regex regexEntry = new Regex(@"(?'meta'[^{}]*){(?'body'(?>[^{}]+|(?<Open>{)|(?<Close-Open>})))*}");
        private static readonly Regex regexFrame = new Regex(@"Frame[\s]*:[\s]*(?<meta>[\s\w=\[\]]+)*[\s\r\n]*{[\s\r\n]*(?<body>[\s\W\S]*)}");
        private static readonly Regex regexItem = new Regex(@"Item:(?<item_meta>.*)[\r\n\s]*{[\r\n\s]*(?<item_body>[\s\W\S]*)}");
        private static readonly Regex regexSwitch = new Regex(@"Switch[\s\r\n]*\((?<switch_value>.*)\)[\r\n\s]*{[\r\n\s]*(?<switch_cases>[\W\S\s\r\n]*)}");
        private static readonly Regex regexSwitchCases = new Regex(@"(?'case_item'[^{}(]*\((?'case_meta'[^{}]+)\)[^{})]*{[\s\t\r\n]*(?'case_body'(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*)})+");

#if DEBUG
        public static async Task Test()
        {
            SchemeImporter importer = new SchemeImporter();
            Scheme scheme = await importer.FromFileAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Examples\\SchemeSsl.txt");
            byte[] testData = await File.ReadAllBytesAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Test\\1 8e571345-6153-46e4-80ca-386f673073f6.reader");

            Reader reader = new Reader(scheme);
            reader.RegistHandlers("HttpConnect", (x) => { Console.WriteLine("call"); return 0; });
            await reader.ReadAsync(testData);
        }
#endif



        public async Task<Scheme> FromFileAsync(string path)
        {
            byte[] bytes = await File.ReadAllBytesAsync(path);
            string str = Encoding.UTF8.GetString(bytes);
            string substr = str;

            ReadFrame head = new ReadFrame();
            head.name = ConstantsForMappedReader.RESERVED_ID_HEAD;
            ReadOperation roHead = new ReadOperation();
            roHead.AsHead();
            head.SetHead(roHead);

            ReadFrame rfCurrent = default;

            while (true)
            {
                var matchMain = regexEntry.Match(substr);
                var entry = matchMain.Value;

                if (matchMain.Success)
                {
                    if (entry.StartsWith("Scheme"))
                    {
                        substr = substr.Substring(entry.Length).Trim();
                        head = ParseScheme(matchMain, head);
                        rfCurrent = head;
                    }

                    if (entry.StartsWith("Frame"))
                    {
                        substr = substr.Substring(entry.Length).Trim();
                        var matchMeta = regexFrame.Match(entry);

                        if (matchMeta.Success)
                        {
                            var grMeta = matchMeta.Groups["meta"];
                            Meta meta = ParseFrameMeta(grMeta.Value);
                            ReadFrame rf = rfCurrent.Find(meta.Name);
                            ReadOperation ro = new ReadOperation();
                            ro.SetReadLength(0);
                            ro.idFrame = ConstantsForMappedReader.RESERVED_ID_HEAD;
                            rf.SetHead(ro);

                            var body = matchMeta.Groups["body"];
                            string frameBody = body.Value;
                            ReadItem(frameBody, ref ro);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return FinishBuild(head);
        }

        private Scheme FinishBuild(ReadFrame head)
        {
            Scheme scheme = new Scheme();
            scheme.SetHeadReader(head);
            ReadOperation headRo = head.head;
            ReadOperation currentRo = headRo;

            foreach (var rf in head)
            {
                ReadOperation firestRo = rf.head;
                foreach (var ro in firestRo)
                {
                    currentRo.SetNext(ro);
                }
            }
            throw new System.NotImplementedException();
            return scheme;
        }

        private ReadFrame ParseScheme(Match matchMain, ReadFrame head)
        {
            ReadFrame currentRf = head;

            var metaStr = matchMain.Groups["meta"].Value.Trim();
            var bodyStr = matchMain.Groups["body"].Value.Trim();
            var metaSplit = metaStr.Split(':');
            string keyword = metaSplit[0].Trim();
            if (string.Compare(keyword, "Scheme", StringComparison.InvariantCultureIgnoreCase) != 0) throw new FormatException();

            if (metaSplit.Length > 0)
            {
                string name = metaSplit[1];
            }
            else
            {
                throw new System.InvalidOperationException();
            }

            var instructions = bodyStr.Split(';');

            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                var instrSplit = instr.Split(' ');
                var operation = instrSplit[0].Trim();

                if (operation == "Read")
                {
                    ReadFrame readFrame = new ReadFrame();
                    currentRf.SetNext(readFrame);
                    currentRf = readFrame;
                    ReadOperation roRead = new ReadOperation();
                    readFrame.SetHead(roRead);
                    roRead.SetReadLength(0);

                    for (int j = 1; j < instrSplit.Length; j++)
                    {
                        var trim = instrSplit[j].Trim();
                        var atrPair = trim.Substring(1, trim.Length - 2);
                        var splitPair = atrPair.Split('=');
                        var atrName = splitPair[0].Trim();
                        var atrValue = splitPair[1].Trim();
                        var atr = Enum.Parse<EAttribute>(atrName);

                        switch (atr)
                        {
                            case EAttribute.None: throw new System.NotImplementedException();
                            case EAttribute.Size: break;
                            case EAttribute.LengthType: break;
                            case EAttribute.Name: break;
                            case EAttribute.SourceType:
                                {
                                    readFrame.sourceType = Enum.Parse<ESourceType>(atrValue);
                                    break;
                                }
                            case EAttribute.SourceName:
                                {
                                    readFrame.name = atrValue;
                                    break;
                                }
                            case EAttribute.Repeat:
                                {
                                    readFrame.mods |= EMods.Repeat;
                                    break;
                                }
                            case EAttribute.ConditionType: break;
                            case EAttribute.ConditionValue: break;
                            case EAttribute.Algo: break;
                            default: break;
                        }
                    }
                }
            }
            return head;
        }

        private void ReadItem(string str, ref ReadOperation parent)
        {
            var match = regexItem.Match(str);
            if (match.Success)
            {
                var itemMeta = match.Groups["item_meta"];
                var itemBody = match.Groups["item_body"];
                var meta = ParseItemMeta(itemMeta.Value);
                ReadOperation readOperation = new ReadOperation();
                readOperation.SetReadLength(GetSize(meta));
                parent.SetNext(readOperation);
                ReadFields(itemBody.Value, readOperation);
            }
        }

        private int GetSize(Meta meta)
        {
            if (meta.sizeType == ESizeType.Defined) return meta.size;
            if (meta.sizeType == ESizeType.Linked)
            {
                throw new System.NotImplementedException();
            }
            throw new NotImplementedException();
        }

        private void ReadFields(string input, ReadOperation parent)
        {
            if (input.StartsWith("Switch"))
            {
                string switchBody = input;
                ReadSwitch(switchBody, parent);
            }
            else if (input.StartsWith(nameof(EKeyWord.Item)))
            {
                string item = FindItem(input);
                ReadItem(item, ref parent);
            }
            else
            {
                var split = input.Split(' ');
                var typeName = split[0];
                var fieldName = split[1];
                var sizeNote = split[2];
                var size = 0;
                bool isLinkedValue = sizeNote.Contains('$');
                bool isLinkedSource = fieldName.StartsWith('$');

                if (isLinkedSource)
                {
                    variables.Add(new Variables(typeName, fieldName));
                }

                ReadOperation readOperation = new ReadOperation();
                if (isLinkedValue)
                {
                    readOperation.FromLinked();
                }
                else
                {
                    readOperation.SetReadLength(size);
                }
                parent.SetNext(readOperation);

                // Read Attributes
                for (int i = 3; i < split.Length; i++)
                {
                    var result = ReadAttribute(split[i]);
                    var atr = result.Item1;
                    var value = result.Item2;

                }
            }
        }

        private (EAttribute, string) ReadAttribute(string v)
        {
            throw new NotImplementedException();
        }

        private void ReadSwitch(string switchBody, ReadOperation parent)
        {
            var match = regexSwitch.Match(switchBody);
            ValuePromise valuePromise = default;
            if (match.Success)
            {
                var value = match.Groups["switch_value"].Value;
                if (value.Contains('$'))
                {
                    valuePromise = Variables.GetValue(value);
                }
                var cases = match.Groups["switch_cases"].Value;
                var matchCases = regexSwitchCases.Match(cases);
                int caseCount = matchCases.Groups["case_item"].Captures.Count;
                for (int i = 0; i < caseCount; i++)
                {
                    var item = matchCases.Groups["case_item"].Captures[i].Value;
                    var meta = matchCases.Groups["case_meta"].Captures[i].Value;
                    var body = matchCases.Groups["case_body"].Captures[i].Value;

                    if (meta.Contains("$"))
                    {
                        throw new System.NotImplementedException();
                    }
                    else
                    {
                        parent.AddCondition(valuePromise.IfEqual(valuePromise.ParseValue(meta)));
                    }
                    ReadFields(body, parent);
                }
            }
            else
                throw new NotImplementedException();
        }




        private string FindItem(string value)
        {
            throw new System.NotImplementedException();
        }

        private static Meta ParseItemMeta(string value)
        {
            var splits = value.Split(' ');
            var name = splits[0];
            int size = default;
            ESizeType sizeType = ESizeType.None;

            for (int i = 1; i < splits.Length; i++)
            {
                Attribute atr = ParseAttribute(splits[i]);
                switch (atr.type)
                {
                    case EAttribute.None: throw new System.NotImplementedException();
                    case EAttribute.Size: size = atr.length; break;
                    case EAttribute.LengthType:
                        {
                            sizeType = atr.sizeType;
                            if (atr.sizeType == ESizeType.Defined)
                            {

                            }
                            else if (atr.sizeType == ESizeType.Dynamic)
                            {

                            }
                            break;
                        }
                    default: throw new System.NotImplementedException();
                }
            }
            return new Meta()
            {
                Name = name,
                size = size,
                sizeType = sizeType,
            };
        }
        private static Meta ParseFrameMeta(string value)
        {
            var splits = value.Split(' ');
            var name = splits[0];
            int size = default;
            ESizeType sizeType = ESizeType.None;

            for (int i = 1; i < splits.Length; i++)
            {
                Attribute atr = ParseAttribute(splits[i]);
                switch (atr.type)
                {
                    case EAttribute.None: throw new ArgumentOutOfRangeException();
                    case EAttribute.Size: size = atr.length; break;
                    case EAttribute.LengthType:
                        {
                            sizeType = atr.sizeType;
                            if (atr.sizeType == ESizeType.Defined)
                            {

                            }
                            else if (atr.sizeType == ESizeType.Dynamic)
                            {

                            }
                            break;
                        }
                    default: throw new System.NotImplementedException();
                }
            }
            return new Meta()
            {
                Name = name,
                size = size,
                sizeType = sizeType,
            };
        }
        private static Attribute ParseAttribute(string value)
        {
            value = value.Trim();
            var split = value.Substring(1, value.Length - 2).Split('=');
            var atr = new Attribute()
            {
                type = Enum.Parse<EAttribute>(split[0]),
            };
            switch (atr.type)
            {
                case EAttribute.Size: atr.length = int.Parse(split[1]); break;
                case EAttribute.LengthType: atr.sizeType = Enum.Parse<ESizeType>(split[1]); break;
                case EAttribute.None: throw new System.NotImplementedException();
                default: throw new System.NotImplementedException();
            }
            return atr;
        }
        private struct Attribute
        {
            public EAttribute type;
            public int length;
            public Scheme.ESizeType sizeType;
        }

        private struct Meta
        {
            public string Name;
            public int size;
            public Scheme.ESizeType sizeType;
        }


    }

    public enum ESourceType
    {
        None,
        Frame,
        String,
    }
    [Flags]
    public enum EMods
    {
        None = 0,
        Repeat = 1 << 0,
    }

    public enum EAttribute
    {
        None,
        Size,
        LengthType,
        SourceType,
        /// <summary>
        /// ID. Defined Name as StringID
        /// </summary>
        Name,
        /// <summary>
        /// ID of Source
        /// </summary>
        SourceName,
        /// <summary>
        /// Read Mode - Repeat
        /// </summary>
        Repeat,
        ConditionType,
        ConditionValue,
        Algo,
    }
}