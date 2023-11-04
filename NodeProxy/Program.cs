// See https://aka.ms/new-console-template for more information
using System;
using IziHardGames.NodeProxy;

Console.WriteLine("Hello, World!");


var http = Proxy.StartTcp(49702);
//var https = Proxy.StartTcp(60121);

await http.ConfigureAwait(false);
//await https.ConfigureAwait(false);