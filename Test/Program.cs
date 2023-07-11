using HttpDecodingProxy.ForHttp;
using System.Buffers;

byte[] bytes = new byte[4096];
ArrayPool<byte>.Shared.Return(bytes);

Console.ReadLine();