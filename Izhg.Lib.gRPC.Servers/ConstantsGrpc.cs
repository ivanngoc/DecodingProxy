namespace IziHardGames.Libs.gRPC.InterprocessCommunication
{
    public static class ConstantsGrpc
    {
        public const int TYPE_SUCCEED = 1;


        public static class Headers
        {
            public const int TYPE_ERROR = 2;
        }

        public static class Properties
        {
            public const string MESSAGE_TYPE = "messageType";
            public const string CLIENT_ID = "clientId";
        }

        public static class Message
        {
            public const int TYPE_AUTH = 451189;
            public const int TYPE_DATA_FOR_SERVER = 3;
        }
    }
}
