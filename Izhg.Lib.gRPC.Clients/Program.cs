// See https://aka.ms/new-console-template for more information
using IziHardGames.Libs.gRPC.InterprocessCommunication;

Console.WriteLine("Hello, World!");
await EchoClient.Test();