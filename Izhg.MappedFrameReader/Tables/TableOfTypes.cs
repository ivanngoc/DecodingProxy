using static IziHardGames.MappedFrameReader.SchemeImporter;

namespace IziHardGames.MappedFrameReader
{
    internal class TableOfTypes
    {
        private readonly Dictionary<string, DefinedType> types = new Dictionary<string, DefinedType>();

        public TableOfTypes()
        {
            var v1 = new DefinedType() { name = nameof(EValueTypes.Byte).ToLower(), size = 1, sizeType = Scheme.ESizeType.Defined, valueParser = new ByteParser() };
            var v2 = new DefinedType() { name = nameof(EValueTypes.Uint16).ToLower(), size = 2, sizeType = Scheme.ESizeType.Defined };
            var v21 = new DefinedType() { name = "ushort", size = 2, sizeType = Scheme.ESizeType.Defined };
            var v3 = new DefinedType() { name = nameof(EValueTypes.Uint32).ToLower(), size = 4, sizeType = Scheme.ESizeType.Defined };
            var v31 = new DefinedType() { name = "uint", size = 4, sizeType = Scheme.ESizeType.Defined };
            var v5 = new DefinedType() { name = nameof(EValueTypes.String).ToLower(), size = 0, sizeType = Scheme.ESizeType.Dynamic, valueParser = new StringParser() };
            var v6 = new DefinedType() { name = nameof(EValueTypes.Slice).ToLower(), size = 0, sizeType = Scheme.ESizeType.Dynamic, valueParser = new SliceParser() };

            types.Add(v1.name, v1);
            types.Add(v2.name, v2);
            types.Add(v21.name, v21);
            types.Add(v3.name, v3);
            types.Add(v31.name, v31);
            types.Add(v5.name, v5);
            types.Add(v6.name, v6);
        }
        internal DefinedType CreateNew(Meta meta)
        {
            var dt = new DefinedType();
            dt.name = meta.Name;
            dt.sizeType = meta.sizeType;
            dt.size = meta.size;
            types.Add(dt.name, dt);
            return dt;
        }

        internal DefinedType Get(string typeName)
        {
            Console.WriteLine($"GetDefinedType: {typeName}");
            return types[typeName.ToLower()];
        }

        internal bool StartWithTypeName(string capture, out DefinedType? type)
        {
            string str = capture.TrimStart();
            int index = -1;

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]))
                {
                    index = i; break;
                }
            }
            string keyword = str.Substring(0, index);
            if (types.TryGetValue(keyword, out type!))
            {
                return true;
            }
            type = default;
            return false;
        }
    }
}