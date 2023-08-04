namespace IziHardGames.Libs.Networking.Contracts
{
    public interface ITrafficMetr
    {
        long TotalBytes { get; set; }
        long Sended { get; set; }
        long Recived { get; set; }
    }
}