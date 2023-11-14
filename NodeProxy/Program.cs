// See https://aka.ms/new-console-template for more information
using System;
using IziHardGames.NodeProxies.Version1;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.NodeProxies.Nodes.Tls;

Console.WriteLine("Hello, World!");

await NodeTlsHandshakeFromClient.Test();
Console.ReadLine();
var http = NodeProxy.RunSmartTcp(63401);
//var http = Proxy.RunSmartTcp(60121);
//var https = Proxy.StartTcp(60121);

await http.ConfigureAwait(false);
//await https.ConfigureAwait(false);