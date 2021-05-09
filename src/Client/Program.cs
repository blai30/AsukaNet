using System;
using Asuka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Extensions;
using Serilog;

Console.WriteLine(DateTime.UtcNow.ToString("R"));
Console.WriteLine(Environment.ProcessId);

// Typical ASP.NET host builder pattern but for console app without the web.
// TODO: Official support planned for .NET 6.0.
var host = Host
    .CreateDefaultBuilder(args)
    .UseSerilog()
    .UseStartup<Startup>()
    .Build();

// Configure services.
// using var scope = host.Services.CreateScope();
// var services = scope.ServiceProvider;

await host.RunAsync();
