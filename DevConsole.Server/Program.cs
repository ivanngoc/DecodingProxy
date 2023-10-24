using System;
using System.Text;
using DevConsole.Server.Services;
using DevConsole.Shared.Consoles;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddGrpc();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ConsolesManager>();
builder.Services.AddSingleton<ConsolesServer>();
builder.Services.AddSingleton<ConsoleService>();
builder.Services.AddSingleton<ConsoleHub>();
builder.Services.AddHostedService<ConsoleService>();
builder.Services.AddHostedService<ConsolesServerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(conf =>
{
    conf.MapHub<ConsoleHub>("/hubs/updates");
});

app.Run();
