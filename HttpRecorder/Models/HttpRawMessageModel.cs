namespace IziHardGames.Libs.HttpRecording.Models
{ 
    public class HttpRawMessageModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Host { get; set; }
        public byte[] Data { get; set; }
    }
}
