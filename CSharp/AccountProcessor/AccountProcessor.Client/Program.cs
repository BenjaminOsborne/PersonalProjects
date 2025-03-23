using AccountProcessor.Client.ClientServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.ConfigureClientServices(builder.Configuration);

builder.Services.AddMudServices();

//builder.RootComponents.Add<Routes>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();
