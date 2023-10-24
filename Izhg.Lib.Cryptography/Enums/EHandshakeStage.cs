using System;

namespace IziHardGames.Libs.Cryptography.Attributes
{
    [Flags]
    public enum EHandshakeStage
    {
        All = -1,
        None = 0,
        /// <summary>
        /// [1] Sender:Client
        /// </summary>
        ClientHello = 1 << 0,
        /// <summary>
        /// [2] Sernder:Server
        /// </summary>
        ServerHello = 1 << 1,
        /// <summary>
        /// [3] Sender:Server
        /// </summary>
        ServerCertificate = 1 << 2,
        /// <summary>
        /// [4] Sender:Server
        /// </summary>
        ServerKeyExchange = 1 << 3,
        /// <summary>
        /// [5] Sernder:Server
        /// </summary>
        ServerHelloDone = 1 << 4,
        /// <summary>
        /// [6] Sender:Client
        /// </summary>
        ClientKeyExchange = 1 << 5,
        /// <summary>
        /// [7] Sender:Client
        /// </summary>
        ClientChangeCipherSpec = 1 << 6,
        /// <summary>
        /// [8] Sender:Client
        /// </summary>
        ClientHandshakeFinished = 1 << 7,
        /// <summary>
        /// [9] Sender:Server
        /// </summary>
        ServerChangeCipherSpec = 1 << 8,
        /// <summary>
        /// [10] Sender:Server
        /// </summary>
        ServerHandshakeFinished = 1 << 9,
        /// <summary>
        /// [11] Sender:Client
        /// </summary>
        ClientApplicationData = 1 << 10,
        /// <summary>
        /// [12] Sender:Server
        /// </summary>
        ServerApplicationData = 1 << 11,
        /// <summary>
        /// [13] Sender:Client
        /// </summary>
        ClientCloseNotify = 1 << 12,
    }
}
