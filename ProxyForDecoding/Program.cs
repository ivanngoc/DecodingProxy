﻿using IziHardGames.Proxy;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Recoreder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

Console.OutputEncoding = System.Text.Encoding.Unicode;
Console.InputEncoding = System.Text.Encoding.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the co ntainer.
var grpcBuilder = builder.Services.AddGrpc();

builder.Services.AddHostedService<ProxyService>();
builder.Services.AddSingleton((x) => ProxyFactory<HttpSpyProxy>.Create(EProxyBehaviour.MitmSpy, () => new HttpSpyProxy(x.GetService<ILogger<HttpSpyProxy>>()!, x.GetService<MonitorForConnections>()!)));
builder.Services.AddSingleton<MonitorForConnections>();
builder.Services.AddSingleton<GrpcServiceServer>();
builder.Services.AddSingleton<HttpRecoreder>();
builder.Services.AddSingleton<GrpcProxyPublisherService>();

var app = builder.Build();

app.MapGrpcService<GrpcProxyPublisherService>(); //.RequireHost("*:7042");
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// C:\Users\ngoc\Documents\[Projects] C#\IziHardGamesProxy\ProxyForDecoding\Properties\launchSettings.json
// "applicationUrl": "http://localhost:5104;https://localhost:7256", при запуске через Visual Studio используются эти порты

// appsettings.Development.json Kestrel.EndPoints.Http и Kestrel.EndPoints.Https устанавливают адрес и порты прослушки для приложения запущенного в Visual Studio 
// Также перезаписывает launchSettings.json


// appsettings.json устанавливает тоже что и appsettings.Development.json но применяется к процессам запущенным вне Visual Studio например через файл exe

// необходимо обязательно устанавливать опцию         "Protocols": "Http2" так как по нему работает gRPC


app.Run();