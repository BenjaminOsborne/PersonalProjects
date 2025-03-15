using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

//builder.RootComponents.Add<Routes>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

// set base address for default host
builder.Services.AddScoped(sp =>
{
    var fe = builder.Configuration["FrontendUrl"];
    var client = new HttpClient {BaseAddress = new Uri(fe ?? "https://localhost:5002")};
    return client;
});

await builder.Build().RunAsync();
