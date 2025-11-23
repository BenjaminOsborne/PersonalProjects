using BibleApp.Client.ClientServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.ConfigureClientServices(builder.Configuration);

builder.Services.AddMudServices();

await builder.Build().RunAsync();
