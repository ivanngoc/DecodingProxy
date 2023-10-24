using System;
using System.Text;
using System.Text.RegularExpressions;
using IziHardGames.Libs.RegexExpressions;
using IziHardGames.Libs.Text;
using static IziHardGames.MappedFrameReader.Scheme;
using Func = System.Func<System.ReadOnlyMemory<byte>, int>;
using Constants = IziHardGames.MappedFrameReader.ConstantsForMappedReader;
using Microsoft.VisualBasic;
using System.Linq;

namespace IziHardGames.MappedFrameReader
{
    public class SchemeImporter
    {
        //Common
        private static readonly Regex balancedGroupe = new Regex(@"(?>(?>(?'open'{)[^{}]*)+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))");
        //Special
        private static readonly Regex regexEntry = new Regex(@"(?'meta'[^{}]*){(?'body'(?>[^{}]+|(?<Open>{)|(?<Close-Open>})))*}");
        private static readonly Regex regexFrame = new Regex(@"Frame:(?<meta>[^{]*){(?<body>(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*)}");
        private static readonly Regex regexItem = new Regex(@"Item:(?<item_meta>[^{]*){(?<item_body>(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*)}");
        private static readonly Regex regexSwitch = new Regex(@"Switch(?<switch_value>[^{]*){(?<switch_cases>(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*)}");
        private static readonly Regex regexSwitchCases = new Regex(@"(?'case_item'[^{}(]*\((?'case_meta'[^{}]+)\)[^{})]*{[\s\t\r\n]*(?'case_body'(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*)})+");


        internal TableOfPromises tableOfPromises = new TableOfPromises();
        internal TableOfVariables tableOfVariables = new TableOfVariables();
        internal TableOfTypes tableOfTypes = new TableOfTypes();
        internal TableOfFrames tableOfFrames = new TableOfFrames();
        internal TableOfResults tableOfResults = new TableOfResults();
        public TableOfFuncs tableOfFuncs = new TableOfFuncs();

#if DEBUG
        public static async Task Test()
        {
            SchemeImporter importer = new SchemeImporter();
            importer.tableOfFuncs.AddAdvancingFunc($"ReadBodyHttp11", PopularFuncs.ReadBodyHttp11);

            Scheme scheme = await importer.FromFileAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Examples\\SchemeSsl.txt");
            byte[] testData = await File.ReadAllBytesAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Test\\1 8e571345-6153-46e4-80ca-386f673073f6.reader.clear");

            Reader reader = new Reader(scheme);
            reader.scheme.RegistHandlers("HttpConnect.StartLine", (x) => { Console.WriteLine(Encoding.UTF8.GetString(x.Span)); return 0; });
            reader.scheme.RegistHandlers("HttpConnect.Headers", (x) => { Console.WriteLine(Encoding.UTF8.GetString(x.Span)); return 0; });
            reader.scheme.RegistHandlers("SslFrame", (x) => { Console.WriteLine(Encoding.UTF8.GetString(x.Span)); return 0; });
            reader.scheme.RegistHandlers("SslFrame.TlsRecord.RecordType", (x) => { Console.WriteLine(Convert.ToHexString(x.Span)); return 0; });

            await reader.ReadAllAsync(testData);
        }
#endif

        public SchemeImporter()
        {
            tableOfPromises.tableOfVariables = tableOfVariables;
            tableOfFuncs.tableOfResults = tableOfResults;
        }

        public async Task<Scheme> FromFileAsync(string path)
        {
            byte[] bytes = await File.ReadAllBytesAsync(path);
            string str = Encoding.UTF8.GetString(bytes);
            string substr = str;

            NodeBegin nodeBegin = new NodeBegin();
            Node nodeCurrent = nodeBegin;

            while (true)
            {
                var matchMain = regexEntry.Match(substr);
                var entry = matchMain.Value;

                if (matchMain.Success)
                {
                    if (entry.StartsWith("Scheme"))
                    {
                        ParseScheme(ref substr, ref nodeCurrent, matchMain, entry);
                    }
                    else if (entry.StartsWith("Frame"))
                    {
                        ParseFrame(entry, ref substr, ref nodeCurrent);
                    }
                }
                else
                {
                    break;
                }
            }
            NodeEnd nodeEnd = new NodeEnd();
            nodeEnd.SetThisAfter(ref nodeCurrent);
            return FinishBuild(nodeBegin);
        }

        private void ParseScheme(ref string substr, ref Node nodeCurrent, Match matchMain, string entry)
        {
            NodeSchemeBegin nodeBeginScheme = new NodeSchemeBegin();
            NodeSchemeEnd nodeSchemeEnd = new NodeSchemeEnd();

            nodeBeginScheme.SetThisAfter(ref nodeCurrent!);
            substr = substr.Substring(entry.Length).Trim();

            var metaStr = matchMain.Groups["meta"].Value.Trim();
            var bodyStr = matchMain.Groups["body"].Value.Trim();
            var metaSplit = metaStr.Split(':');
            string keyword = metaSplit[0].Trim();
            string name = metaSplit[1].Trim();

            nodeBeginScheme.SetPath($"Begin Scheme:{name}");
            nodeSchemeEnd.SetPath($"End Scheme:{name}");

            if (string.Compare(keyword, "Scheme", StringComparison.InvariantCultureIgnoreCase) != 0) throw new FormatException();

            var instructions = bodyStr.Split(';');

            for (int i = 0; i < instructions.Length - 1; i++)
            {
                var instr = instructions[i];
                var instrSplit = instr.Split(' ');
                var operation = instrSplit[0].Trim();
                // Fill meta data
                if (operation == "Read")
                {
                    Attribute[] attributes = ParseAttributes(instrSplit);
                    var sourceType = attributes.FirstOrDefault(x => x.type == EAttribute.SourceType);
                    if (sourceType != default)
                    {
                        if (sourceType.valueString == "Frame")
                        {
                            var atrName = attributes.First(x => x.type == EAttribute.SourceName);
                            NodeFrameBegin nbf = GetOrCreateNodeBeginFrame(atrName.valueString);

                            var atrRepeat = attributes.FirstOrDefault(x => x.type == EAttribute.Repeat);
                            if (atrRepeat != default)
                            {
                                nbf.SetRepeat();
                            }
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            nodeSchemeEnd.SetThisAfter(ref nodeCurrent!);
        }

        private Scheme FinishBuild(Node begin)
        {
            var graph = begin.ToGraph();
            Scheme scheme = new Scheme(graph);
            scheme.TableOfPromises = tableOfPromises;
            scheme.TableOfTypes = tableOfTypes;
            scheme.TableOfVariables = tableOfVariables;
            scheme.TableOfFrames = tableOfFrames;
            return scheme;
        }


        private NodeFrameBegin GetOrCreateNodeBeginFrame(string valueString)
        {
            throw new NotImplementedException();
        }

        private int ParseItem(string str, ref Node node)
        {
            var match = regexItem.Match(str);
            if (match.Success)
            {
                //Console.WriteLine($"{nameof(ParseItem)}: {match.Value}");
                var itemMeta = match.Groups["item_meta"].Value;
                var itemBody = match.Groups["item_body"].Value;
                var meta = ParseItemMeta(itemMeta, ref node);
                var path = meta.Name;

                var definedType = tableOfTypes.CreateNew(meta);

                NodeItemBegin begin = new NodeItemBegin();
                NodeItemEnd end = new NodeItemEnd();
                NodeResult nodeResult = new NodeResult();
                begin.SetEnd(end);
                begin.SetPath(path);
                end.SetPath(path);
                nodeResult.SetPath(path);
                nodeResult.For(begin);
                nodeResult.FromTo(begin, end, definedType);

                var v = tableOfVariables.CreateVariable($"${path}", path, definedType, end, nodeResult);

                begin.SetThisAfter(ref node!);
                begin.SetType(definedType);

                ParseFields(itemBody, ref node!);
                end.SetThisAfter(ref node!);
                nodeResult.SetThisAfter(ref node!);
                return match.Value.Length;
            }
            return 0;
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

        private void ParseFields(string input, ref Node node)
        {
            int lengthLeft = input.Length;
            var capture = input;
            while (lengthLeft > 0)
            {
                //Console.WriteLine($"ParseFields.Capture:{Environment.NewLine}{capture}");

                var trimmed = capture.TrimStart();
                if (trimmed.Length != capture.Length)
                {
                    lengthLeft -= capture.Length - trimmed.Length;
                    capture = trimmed;
                    if (lengthLeft == 0) break;
                }
                int lengthConsumed = default;

                if (tableOfTypes.StartWithTypeName(capture, out var type))
                {
                    int length = ParseFieldTyped(capture, type!, ref node);
                    lengthConsumed += length;
                }
                else if (capture.StartsWith("vector"))
                {
                    int length = ParseVector(capture, ref node);
                    lengthConsumed += length + 1;
                }
                else if (capture.StartsWith("Item"))
                {
                    int length = ParseItem(capture, ref node);
                    lengthConsumed += length;
                }
                else if (capture.StartsWith("Switch"))
                {
                    string switchBody = input;
                    int length = ParseSwitch(switchBody, ref node);
                    lengthConsumed += length;
                }
                else if (capture.StartsWith(nameof(EKeyWord.Item)))
                {
                    int length = ParseItem(capture, ref node);
                    lengthConsumed += length;
                }
                else
                {
                    throw new System.NotImplementedException();
                }
                //Console.WriteLine($"ParseFields.Capture Consumed:{Environment.NewLine}{capture.Substring(0, lengthConsumed)}");
                lengthLeft -= lengthConsumed;
                capture = capture.Substring(lengthConsumed);
            }
        }

        /// <summary>
        /// Non Vector
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <param name="node"></param>
        private int ParseFieldTyped(string input, DefinedType type, ref Node node)
        {
            // typeName - Field Name - (Length) - (Attributens)*N
            var indexDelimeter = input.IndexOf(',');
            var capture = input.Substring(0, indexDelimeter);
            var splits = capture.SplitByWhiteSpace();

            bool isLengthSpecified = false;
            int length = default;
            string typeName = type.name;
            string fieldControl = splits[1].Trim();
            string link = fieldControl;
            string path = fieldControl;
            string lengthControl = splits[2].Trim();
            bool isLinkedLength = lengthControl.Contains('$');
            bool isConditionLength = lengthControl.Contains('?');
            bool isFixedLength = !isLinkedLength && !isConditionLength;
            DefinedType definedType = tableOfTypes.Get(typeName);
            NodeField nodeField = new NodeField();
            nodeField.SetThisAfter(ref node!);
            NodeResult nodeResult = new NodeResult();
            nodeResult.For(nodeField);

            if (fieldControl.Contains('$'))
            {
                path = path.Trim('$').TrimStart();
            }
            Variable variable = tableOfVariables.CreateVariable(link, path, definedType, nodeField, nodeResult);

            nodeField.SetPath(path);
            nodeResult.SetPath(path);

            if (isLinkedLength)
            {
                var lengthLink = lengthControl.Substring(1, lengthControl.Length - 2);
                var lengthLinkPath = lengthLink.Trim('$');
                var promise = tableOfPromises.GetValuePromise(lengthLinkPath);
                NodeReadFromLinked nodeLinked = new NodeReadFromLinked();
                nodeLinked.SetPath(path);
                var source = tableOfPromises.GetValuePromise(lengthLinkPath);
                nodeLinked.ScheduleReadFromPromise(source, ref node!);
            }
            else if (isConditionLength)
            {

            }
            else
            {
                string lengthString = lengthControl.TrimStart('(').TrimEnd(')').Trim();
                length = int.Parse(lengthString);
                nodeField.SetReadLength(length, path, definedType);
                isLengthSpecified = true;
            }

            if (splits.Length > 3)
            {
                var attributes = ParseAttributes(splits.Skip(3).ToArray(), ref node);

                if (isConditionLength)
                {
                    var condType = attributes.FirstOrDefault(x => x.type == EAttribute.ConditionType);
                    if (condType != default)
                    {
                        var condValue = attributes.FirstOrDefault(x => x.type == EAttribute.ConditionValue);
                        if (condValue != default)
                        {
                            var algo = attributes.FirstOrDefault(x => x.type == EAttribute.AdvanceMode);

                            if (algo != default)
                            {
                                if (algo.valueString == "StringCompare")
                                {
                                    if (condValue.valueType == Constants.TYPE_STRING)
                                    {
                                        if (string.Compare(condType.valueString.Trim(), "ClosedBy", StringComparison.InvariantCultureIgnoreCase) == 0)
                                        {
                                            if (condValue.valueType == Constants.TYPE_STRING)
                                            {
                                                var value = condValue.valueString.Substring(1, condValue.valueString.Length - 2); // Trim '"';
                                                NodeReadWithCondition nodeCondition = new NodeReadWithCondition();
                                                nodeCondition.path = nodeField.path;
                                                nodeCondition.SetThisAfter(ref node!);
                                                nodeCondition.SetCondition(ECondition.SearchUntilExclusive, Encoding.UTF8.GetBytes(Regex.Unescape(value)));
                                                nodeResult.From(nodeCondition, definedType);
                                            }
                                            else
                                            {
                                                throw new System.NotImplementedException();
                                            }
                                        }
                                        else if (string.Compare(condType.valueString.Trim(), "ClosedAt", StringComparison.InvariantCultureIgnoreCase) == 0)
                                        {
                                            var v = ECondition.SearchUntilInclusive;
                                            throw new System.NotImplementedException();
                                        }
                                        else if (string.Compare(condType.valueString.Trim(), "Func", StringComparison.InvariantCultureIgnoreCase) == 0)
                                        {
                                            var func = tableOfFuncs.GetFunc(condValue.valueString);
                                        }
                                    }
                                    else
                                    {
                                        throw new System.NotImplementedException();
                                    }
                                }
                                else if (algo.valueString == "Func")
                                {
                                    string funcId = condValue.valueString;
                                    var func = tableOfFuncs.GetFunc(funcId);
                                    nodeResult.FromFunc(func, definedType);
                                }
                            }
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                }
                else if (isFixedLength)// fixed length
                {
                    SetReadFromSource(type, ref node, length, path, nodeResult);

                    var atrCast = attributes.FirstOrDefault(x => x.type == EAttribute.Cast);
                    if (atrCast != default)
                    {
                        var isCast = atrCast.GetValueBoolean();
                        if (isCast)
                        {
                            if (isLengthSpecified)
                            {
                                nodeField.SetCast();
                            }
                            else
                            {
                                throw new InvalidOperationException("Cast specified. Length Must be presented");
                            }
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }
            else
            {
                SetReadFromSource(type, ref node, length, path, nodeResult);
            }
#if DEBUG
            if (nodeResult.Mode == EResultCollectingMode.None) throw new System.NotImplementedException();
#endif
            nodeResult.SetThisAfter(ref node!);
            return indexDelimeter + 1;
        }

        private static void SetReadFromSource(DefinedType type, ref Node node, int length, string path, NodeResult nodeResult)
        {
            NodeReadFromSource nodeReadFromSource = new NodeReadFromSource();
            nodeReadFromSource.SetReadLength(length, path, type);
            nodeReadFromSource.SetThisAfter(ref node!);
            nodeReadFromSource.SetPath(path);
            nodeResult.From(nodeReadFromSource, type);
        }

        private int ParseVector(string substr, ref Node node)
        {
            int indexDelimeter = substr.IndexOf(',');
            var capture = substr.TrimStart().Substring(0, indexDelimeter).TrimEnd();
            capture = capture.Substring(Constants.KEYWORD_VECTOR.Length).TrimStart();

            NodeRepeat nodeRepeat = new NodeRepeat(tableOfResults);
            nodeRepeat.SetThisAfter(ref node!);
            NodeResult nodeResult = new NodeResult();
            nodeResult.SetThisAfter(ref node!);
            nodeResult.For(nodeRepeat);


            var splits = capture.SplitByWhiteSpace();
            var typeName = splits[0].Trim();
            var fieldControl = splits[1].Trim();
            var path = fieldControl;
            var link = fieldControl;

            DefinedType type = tableOfTypes.Get(typeName);
            nodeResult.From(nodeRepeat, type);

            if (fieldControl.Contains('$'))
            {
                path = path.TrimStart('$').TrimStart();
                tableOfVariables.CreateVariable(fieldControl, path, type, nodeRepeat, nodeResult);
            }
            nodeRepeat.path = path;
            nodeResult.path = path;

            var lengthControl = splits[2];
            bool isCondition = lengthControl.Contains('?');
            bool isLinkedLength = lengthControl.Contains('$');
            var atrs = ParseAttributes(splits.Skip(3).ToArray(), ref node!);
            AttributesUtility.ValidateForRepeat(atrs);
            string lengthValue = lengthControl.TrimStart('(').TrimEnd(')');

            if (isLinkedLength)
            {
                string lengthPath = lengthValue.Trim('$');
                var promise = tableOfPromises.GetValuePromise(lengthPath);
                NodeReadFromLinked nrfl = new NodeReadFromLinked();
                nrfl.ScheduleReadFromPromise(promise, ref node);
            }

            Variable variable = tableOfVariables.CreateVariable(link, path, type, nodeRepeat, nodeResult);


            if (isCondition)
            {
                var atrAlgo = atrs.First(x => x.type == EAttribute.AdvanceMode);
                var atrCondType = atrs.First(x => x.type == EAttribute.ConditionType);
                var atrCondValue = atrs.FirstOrDefault(x => x.type == EAttribute.ConditionValue);
                var atrRepeat = atrs.FirstOrDefault(x => x.type == EAttribute.Repeat);
                var atrSeparator = atrs.FirstOrDefault(x => x.type == EAttribute.Separator);
                var atrEnclose = atrs.FirstOrDefault(x => x.type == EAttribute.Enclose);
                bool isRepeat = atrRepeat.GetValueBoolean();

                if (atrCondType.valueType == Constants.TYPE_STRING)
                {
                    if (atrCondType.valueString == "ClosedBy")
                    {
                        if (atrCondValue != default)
                        {
                            if (atrCondValue.valueType == Constants.TYPE_STRING)
                            {
                                var value = atrCondValue.valueString.Trim('"');
                                NodeReadWithCondition nrfc = new NodeReadWithCondition();
                                nodeRepeat.SetRepeatWithNode(ERepeatMode.WhileCondition, nrfc);
                                byte[] bytes = Encoding.UTF8.GetBytes(value);
                                nrfc.SetCondition(ECondition.SearchUntilExclusive, bytes);
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }
                        }
                        else
                        {
                            var value = atrSeparator.valueString.Trim('"');
                            var valueEnclose = atrEnclose.valueString.Trim('"');
                            if (atrSeparator != default)
                            {
                                nodeRepeat.SetEnclose(Encoding.UTF8.GetBytes(Regex.Unescape(valueEnclose)));
                                nodeRepeat.SetRepeatWithSeparator(Encoding.UTF8.GetBytes(Regex.Unescape(value)));
                            }
                            else
                            {
                                throw new System.NotImplementedException();
                            }
                        }
                    }
                    else if (atrCondType.valueString == Constants.ATR_ALGO_FUNC)
                    {
                        var func = tableOfFuncs.GetFunc(atrCondValue.valueString);
                        nodeRepeat.SetFunc(func);
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            else if (isLinkedLength)
            {
                ValuePromise valuePromise = tableOfPromises.GetValuePromise(path);
                NodeReadFromSource nrfl = new NodeReadFromSource();
                nodeRepeat.SetRepeatWithNode(ERepeatMode.FixedFromSource, nrfl, valuePromise);
                nrfl.SetReadLength(fieldControl, type);
                throw new System.NotImplementedException("Array or N-count elements?");
            }
            else
            {

                throw new System.NotImplementedException();
            }
            return indexDelimeter + 1;
        }



        private int ParseSwitch(string switchBody, ref Node node)
        {
            //Console.WriteLine($"{nameof(ParseSwitch)}: {switchBody}");

            var match = regexSwitch.Match(switchBody);
            ValuePromise? valuePromise = default;
            int length = default;
            if (match.Success)
            {
                NodeSwitch nodeSwitch = new NodeSwitch();
                nodeSwitch.SetThisAfter(ref node!);
                NodeSwitchEnd nodeSwitchEnd = new NodeSwitchEnd();
                nodeSwitchEnd.SetThisAfter(ref node!);

                var valueControl = match.Groups["switch_value"].Value.Trim(); //  ($SslFrame.TlsRecord.RecordType)
                length = valueControl.Length;
                if (valueControl.Contains('$'))
                {
                    var idVar = valueControl.Substring(1, valueControl.Length - 2);
                    var path = idVar.TrimStart('$');
                    nodeSwitch.SetPath(path);
                    nodeSwitchEnd.SetPath(path);
                    valuePromise = tableOfPromises.GetValuePromise(path);
                    nodeSwitch.AddValueSource(valuePromise);
                }
                else
                {
                    throw new NotSupportedException();
                }
                var cases = match.Groups["switch_cases"].Value;
                var matchCases = regexSwitchCases.Match(cases);
                int caseCount = matchCases.Groups["case_item"].Captures.Count;

                for (int i = 0; i < caseCount; i++)
                {
                    NodeSwitchItem nodeSwitchItem = new NodeSwitchItem();
                    Node current = nodeSwitchItem;
                    var item = matchCases.Groups["case_item"].Captures[i].Value;
                    var meta = matchCases.Groups["case_meta"].Captures[i].Value.Trim();
                    var body = matchCases.Groups["case_body"].Captures[i].Value;
                    var caseValue = meta.TrimStart('(').TrimEnd(')').Trim();

                    if (string.Compare(caseValue, "default", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        nodeSwitch.AddCaseDefault(nodeSwitchItem);
                    }
                    else if (meta.Contains("$"))
                    {
                        nodeSwitch.AddValueSourceForCase(null);
                        throw new System.NotImplementedException();
                    }
                    else
                    {
                        var type = nodeSwitch.ValuePromise.variable.type;
                        nodeSwitch.AddCaseValue(type.From(caseValue), nodeSwitchItem);
                        //node.AddCondition(valuePromise.IfEqual(valuePromise.ParseValue(meta)));
                    }
                    ParseFields(body, ref current!);
                    // multiplex

                    NodeSwitchItemEnd nodeSwitchItemEnd = new NodeSwitchItemEnd();
                    nodeSwitchItemEnd.SetThisAfter(ref current!);
                    nodeSwitchEnd.AddEnd(nodeSwitchItemEnd);
                    nodeSwitchItemEnd.SetNext(nodeSwitchEnd);
                }
                return match.Value.Length;
            }
            else
                throw new NotImplementedException();
        }

        private Meta ParseItemMeta(string value, ref Node ro)
        {
            //Console.WriteLine($"{nameof(ParseItemMeta)}: {value}");
            var splits = value.Split(' ');
            var name = splits[0];
            int size = default;
            ESizeType sizeType = ESizeType.None;

            for (int i = 1; i < splits.Length; i++)
            {
                Attribute atr = ParseAttribute(splits[i], ref ro);
                switch (atr.type)
                {
                    case EAttribute.None: throw new System.NotImplementedException();
                    case EAttribute.Size: size = atr.valueInt; break;
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
        private void ParseFrame(string entry, ref string substr, ref Node nodeCurrent)
        {
            NodeFrameBegin nodeFrame = new NodeFrameBegin();
            NodeFrameEnd nodeFrameEnd = new NodeFrameEnd();
            nodeFrame.SetThisAfter(ref nodeCurrent!);
            substr = substr.Substring(entry.Length).TrimStart();
            var matchMeta = regexFrame.Match(entry);
            NodeResult nodeResult = new NodeResult();

            if (matchMeta.Success)
            {
                var grMeta = matchMeta.Groups["meta"];
                Meta meta = ParseFrameMeta(grMeta.Value, ref nodeCurrent!);
                var path = meta.Name.Trim();
                nodeFrame.SetPath(path);
                nodeFrameEnd.SetPath(path);
                nodeResult.SetPath(path);

                DefinedType definedType = tableOfTypes.CreateNew(meta);
                tableOfVariables.CreateVariable($"${path}", path, definedType, nodeFrameEnd, nodeResult);

                var body = matchMeta.Groups["body"];
                string frameBody = body.Value;
                ParseFields(frameBody, ref nodeCurrent);
                nodeFrameEnd.SetThisAfter(ref nodeCurrent!);
                nodeResult.ForFrame(nodeFrame, nodeFrameEnd, definedType);
                nodeResult.SetThisAfter(ref nodeCurrent!);
                nodeFrame.SetEnd(nodeFrameEnd);
            }
            else
            {
                throw new System.FormatException();
            }
        }
        private Meta ParseFrameMeta(string value, ref Node node)
        {
            var splits = value.Split(' ');
            var name = splits[0];
            int size = default;
            ESizeType sizeType = ESizeType.None;

            for (int i = 1; i < splits.Length; i++)
            {
                Attribute atr = ParseAttribute(splits[i], ref node);
                switch (atr.type)
                {
                    case EAttribute.None: throw new ArgumentOutOfRangeException();
                    case EAttribute.Size: size = atr.valueInt; break;
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


        private Attribute[] ParseAttributes(string[] instrSplit)
        {
            Attribute[] attributes = new Attribute[instrSplit.Length - 1];

            for (int j = 1; j < instrSplit.Length; j++)
            {
                var trim = instrSplit[j].Trim();
                var atrPair = trim.Substring(1, trim.Length - 2);
                var splitPair = atrPair.Split('=');
                var atrName = attributes[j - 1].name = splitPair[0].Trim();
                var atrValue = attributes[j - 1].valueString = splitPair[1].Trim();
                var atr = Enum.Parse<EAttribute>(atrName);

                switch (atr)
                {
                    case EAttribute.None: throw new System.NotImplementedException();
                    case EAttribute.Size: break;
                    case EAttribute.LengthType: break;
                    case EAttribute.Name: break;
                    case EAttribute.SourceType:
                        {
                            break;
                        }
                    case EAttribute.SourceName:
                        {
                            break;
                        }
                    case EAttribute.Repeat:
                        {
                            break;
                        }
                    case EAttribute.ConditionType: break;
                    case EAttribute.ConditionValue: break;
                    case EAttribute.AdvanceMode: break;
                    default: break;
                }
            }
            return attributes;
        }

        private Attribute[] ParseAttributes(string[] instrSplit, ref Node node)
        {
            Attribute[] atrs = new Attribute[instrSplit.Length];

            for (int i = 0; i < atrs.Length; i++)
            {
                atrs[i] = ParseAttribute(instrSplit[i], ref node);
            }
            return atrs;
        }


        private Attribute ParseAttribute(string input, ref Node node)
        {
            //Console.WriteLine($"ParseAttribute:[{input}]");
            input = input.Trim();
            var split = input.Substring(1, input.Length - 2).Split('=');

            var value = split[1];
            var atr = new Attribute()
            {
                type = Enum.Parse<EAttribute>(split[0]),
            };
            if (value.Contains('$'))
            {
                string path = value.TrimStart('$');
                ValuePromise promise = tableOfPromises.GetValuePromise(path);
                NodeReadFromLinked ro = new NodeReadFromLinked();
                ro.SetPath(node.path);
                ro.ScheduleReadFromPromise(promise, ref node);
                node = ro;
            }
            else
            {
                switch (atr.type)
                {
                    case EAttribute.None: throw new System.NotImplementedException();
                    case EAttribute.Size:
                        {
                            atr.valueInt = int.Parse(value);
                            atr.valueType = Constants.TYPE_INT;
                            break;
                        }
                    case EAttribute.LengthType:
                        {
                            atr.sizeType = Enum.Parse<ESizeType>(value);
                            atr.valueType = Constants.TYPE_INT;
                            break;
                        }
                    case EAttribute.SourceType: goto default;
                    case EAttribute.Name: goto default;
                    case EAttribute.SourceName: goto default;
                    case EAttribute.Repeat:
                        {
                            atr.valueType = Constants.TYPE_BOOL;
                            atr.valueString = value;
                            break;
                        }
                    case EAttribute.ConditionType: goto default;
                    case EAttribute.ConditionValue: goto default;
                    case EAttribute.AdvanceMode: goto default;
                    case EAttribute.AsignedValue: goto default;
                    case EAttribute.Type: goto default;
                    case EAttribute.Separator: goto default;
                    case EAttribute.Enclose: goto default;
                    case EAttribute.Cast:
                        {
                            goto case EAttribute.Repeat;
                        }
                    default:
                        {
                            atr.valueString = value;
                            atr.valueType = Constants.TYPE_STRING;
                            break;
                        }
                }
            }
            return atr;
        }

        internal struct Attribute
        {
            public EAttribute type;
            public Scheme.ESizeType sizeType;

            public int valueInt;
            public string name;
            public string valueString;
            public string valueType;

            internal void DefineType(Attribute algo)
            {
                if (algo.type == EAttribute.AdvanceMode)
                {
                    if (valueString == Constants.ATR_ALGO_STRING_COMPARE)
                    {
                        valueType = Constants.TYPE_STRING;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                else throw new System.NotImplementedException();
            }

            internal bool GetValueBoolean()
            {
                return bool.Parse(valueString);
            }

            public static bool operator ==(Attribute left, Attribute right)
            {
                return
                    left.type == right.type &&
                    left.sizeType == right.sizeType &&
                    left.valueInt == right.valueInt &&
                    left.valueString == right.valueString &&
                    left.valueType == right.valueType &&
                    left.name == right.name;
            }
            public static bool operator !=(Attribute left, Attribute right)
            {
                return
                     left.type != right.type ||
                     left.sizeType != right.sizeType ||
                     left.valueInt != right.valueInt ||
                     left.valueString != right.valueString ||
                     left.valueType != right.valueType ||
                     left.name != right.name;
            }
        }

        internal struct Meta
        {
            public string Name;
            public int size;
            public Scheme.ESizeType sizeType;
        }
    }
}