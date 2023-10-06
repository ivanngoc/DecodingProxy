namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Mapping Sceme for separation
    /// </summary>
    public class Scheme
    {
        /*
        Принципы:
        - Водопадная модель. Обычно любой формат предусматривает движение от начала до конца в одну сторону. То есть объект нажнего уровеня вложенности не обращается к верхнему
        - Навигация. Должна быть возможна "навигация через точку". Синтаксис должен быть построен чтобы селекторы 
         
        */
        /// <summary>
        /// Flat list of all id's</br>
        /// Inner id is index of this array issued by this class.<br/>
        /// Inner ID - int32 isued by this class, index of array. <br/>
        /// ID - string, custom defined<br/>
        /// Reccomended Use xPath likes syntax for id.  for example: Header.InnerObject.ValueField
        /// </summary>        
        private string[] ids;
        private List<string> tempIds;
        private ReadFrame head;

        public SchemeBinary SchemeBinary => new SchemeBinary(this);


        public FrameScheme MapFrame(int length, string identifier, EFlags flags = EFlags.None)
        {
            throw new System.NotImplementedException();
        }
        public object MapRange(string identifier, int offset, int length, string identifierRange)
        {
            throw new System.NotImplementedException();
        }
        public object MapRange<T>(string identifier, int offset, T lengthSource, string identifierRange) where T : ILengthSource
        {
            throw new System.NotImplementedException();
        }

        private int AddSeparator(CharSequenceSeparator separator, string v)
        {
            throw new NotImplementedException();
        }

        private void BuildReaders()
        {
            ReadOperation operation = default;
            throw new System.NotImplementedException();
        }

        public void Begin()
        {

        }
        public void End()
        {

        }
        public static Scheme LoadFromFile(string path)
        {
            throw new System.NotImplementedException();
        }

#if DEBUG
        #region Develop
        public static void ExampleStunTurn()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Case of non-determined length. 
        /// </summary>
        public static void ExampleHttp11()
        {
            Scheme scheme = new Scheme();
            var frame = scheme.MapFrame(-1, "Http Message");

            var separator = new CharSequenceSeparator("\r\n\r\n".AsMemory());
            var key = scheme.AddSeparator(separator, "Header");
            // Based on header analyz there can be empty/length defined/chuncked content.
        }


        public static void ExampleTls()
        {
            Scheme scheme = new Scheme();
            scheme.Begin();

            var frame = scheme.MapFrame(5, "TlsRecord");
            int key0 = frame.AddMapLengthSource(1, "TlsRecordType", EMapType.TypeSelector); //0x16 - Handshake, 0x17 CipherChange
            int key1 = frame.AddMapLengthSource(2, "ProtocolVersion", EMapType.Value);
            int key2 = frame.AddMapLengthSource(2, "LengthFollowed", EMapType.LengthSource);

            var selector = new ByteSelector(0x16, 0x17);
            int keySwitch0 = selector.Case(0x16, () => { Console.WriteLine("This is handshake"); });
            int keySwitch1 = selector.Case(0x17, () => { Console.WriteLine("This is CipherChange"); });
            frame.AddSelector(key0, selector);

            throw new System.NotImplementedException();
            scheme.End();
        }
        public static void UsageConcept()
        {
            Scheme scheme = new Scheme();
            scheme.Begin();

            var frame = scheme.MapFrame(4, "Header");
            var key1 = scheme.MapRange("Header", 0, 2, "[1] Length Followed After");
            var key2 = scheme.MapRange("Header", 2, (ILengthSource)key1, "[2] Payload ofter [1]");

            throw new System.NotImplementedException();
            scheme.End();
        }

        internal void SetHeadReader(ReadFrame rf)
        {
            this.head = rf;
        }
        #endregion
#endif
        public enum ESizeType
        {
            None,
            Defined,
            Linked,
            Dynamic
        }
    }
}