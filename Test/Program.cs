using System;
using System.Net;
using System.Threading.Tasks;
using IziHardGames.MappedFrameReader;
using IziHardGames.Libs.ForHttp20;
using Test;



await SchemeImporter.Test();
Console.WriteLine($"Scheme Imported");
await TestQuic.Test();
//await AsyncFlowControl.Test();
ReaderHttp2.Test();
//TlcHelloFromClient.Test12();
//Console.ReadLine();
//await TestHttp2.RunTlsParse();
//await TestHttp2.ShowIp();
await TestHttp2.UpgradeConnect2();
//await TestHttp2.RunProxy();
//await TestHttp2.RunProxySsl();
//await TestHttp2.RunHttp2Ssl();
//await TestHttp2.RunHttp2();
await DoAsync();

async Task DoAsync()
{
    var adressed = await Dns.GetHostAddressesAsync("zeroscans.com");

    foreach (var item in adressed)
    {
        Console.WriteLine(item.ToString());
    }

    Console.WriteLine($"Begin");

    //var t1 = TcpInfo.Run(80);
    //var t1 = TcpInfo.RunHex(443);
    var t1 = TcpInfo.Run(49702);
    //var t1 = TcpInfo.Run(60122);
    //var t1 = TcpInfo.RunUdp(60122);
    //var t2 = TcpInfo.Run(61255);
    await Task.WhenAll(t1);
    //await Task.WhenAll(t1,t2);
}
