using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton(sp =>
{
    var uri = builder.Configuration["BackendUrl"]!;
    return new HttpClient { BaseAddress = new Uri(uri) };
});

builder.Services.AddMudServices();

//builder.RootComponents.Add<Routes>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();
