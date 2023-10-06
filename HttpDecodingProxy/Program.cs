using System;
using IziHardGames.Libs.HttpCommon.Recording;

HttpRecordAnalyzer analyzer = new HttpRecordAnalyzer();
await analyzer.Run().ConfigureAwait(false);

Console.WriteLine("Hello, World!");