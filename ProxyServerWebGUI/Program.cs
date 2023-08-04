using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyServerWebGUI.Hubs;
using ProxyServerWebGUI.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddHostedService<SignalRInfoService>();
builder.Services.AddSingleton<SignalRInfoService>();
builder.Services.AddSingleton<GrpcConnector>();
builder.Services.AddSingleton<ProxyChangeReflector>();
builder.Services.AddSingleton<SignalRInfoHub>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.UseEndpoints(conf =>
{
    conf.MapHub<SignalRInfoHub>("/hubs/info");
});

app.Run();
