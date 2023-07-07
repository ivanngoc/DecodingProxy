using IziHardGames.Proxy;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Recoreder;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the co ntainer.
var grpcBuilder = builder.Services.AddGrpc();

builder.Services.AddHostedService<ProxyService>();
builder.Services.AddSingleton<DecodingProxyServerAPI>();
builder.Services.AddSingleton((x) => ProxyFactory<HttpSpyProxy>.Create(EProxyBehaviour.MitmSpy, () => new HttpSpyProxy()));
builder.Services.AddSingleton<HttpRecoreder>();

var app = builder.Build();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// C:\Users\ngoc\Documents\[Projects] C#\IziHardGamesProxy\ProxyForDecoding\Properties\launchSettings.json
// "applicationUrl": "http://localhost:5104;https://localhost:7256", при запуске через Visual Studio используются эти порты

// appsettings.Development.json Kestrel.EndPoints.Http и Kestrel.EndPoints.Https устанавливают адрес и порты прослушки для приложения запущенного в Visual Studio 
// Также перезаписывает launchSettings.json


// appsettings.json устанавливает тоже что и appsettings.Development.json но применяется к процессам запущенным вне Visual Studio например через файл exe

// необходимо обязательно устанавливать опцию         "Protocols": "Http2" так как по нему работает gRPC

app.MapGrpcService<DecodingProxyServerAPI>(); //.RequireHost("*:7042");

app.Run();