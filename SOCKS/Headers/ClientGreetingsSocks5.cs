using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using IziHardGames.Socks5.Enums;

namespace IziHardGames.Socks5.Headers
{
    public class ConstantsForSocks
    {
        public const byte RSV = 0x00;
    }

    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc1928#section-3
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ClientGreetingsSocks5
    {
        [FieldOffset(0)] public byte version;
        /// <summary>
        /// по факту длина списка методов аутентификации которые идут следом. каждый Item = 1 байту.
        /// То есть если это поле = 3, то далее необзодимо считать 3 байта. 
        /// Каждый байт - это метод. Сервер долэжен выбрать 1 из 3 предложенных клиентом методов
        /// </summary>
        [FieldOffset(1)] public byte numberOfAuthMethods;

        public bool IsSocsk5()
        {
            return version == (byte)ESocksType.SOCKS5 && numberOfAuthMethods > 0;
        }

        internal string ToStringInfo()
        {
            return $"Version:{version}\tnumberOfAuthMethods:{numberOfAuthMethods}";
        }
    }

    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc1928#section-3
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ServerChoice
    {
        [FieldOffset(0)] public byte version;
        [FieldOffset(1)] public byte cauth;
        public EAuth Auth => (EAuth)cauth;
    }

    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc1928#section-4
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ClientRequest
    {
        [FieldOffset(0)] private byte ver;
        [FieldOffset(1)] public byte cmd;
        /// <summary>
        /// <see cref="ConstantsForSocks.RSV"/>
        /// </summary>
        [FieldOffset(2)] public byte rsv;
        [FieldOffset(3)] public SOCKS5address atyp;
        public ECmd Cmd => (ECmd)cmd;
        public ESocksType VER { get => (ESocksType)ver; set => ver = (byte)value; }
        public string ToStringInfo() => $"Version:{VER}\tCmd:{Cmd}";
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ServerReply
    {
        [FieldOffset(0)] private byte ver;
        [FieldOffset(1)] private byte rep;
        /// <summary>
        /// <see cref="ConstantsForSocks.RSV"/>
        /// </summary>
        [FieldOffset(2)] private byte rsv;
        [FieldOffset(3)] public SOCKS5address atyp;
        public EReply Reply { get => (EReply)rep; set => rep = (byte)value; }
        public ESocksType VER { get => (ESocksType)ver; set => ver = (byte)value; }
        public string ToStringInfo() => $"Version:{VER}\tReply:{Reply}";
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SOCKS5address
    {
        [FieldOffset(0)] public byte atyp;
        public EAdrType Type { get => (EAdrType)atyp; set => atyp = (byte)value; }
    }

}
