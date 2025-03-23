using AccountProcessor.Client.ClientServices;
using AccountProcessor.Components;
using AccountProcessor.Core.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IMatchModelService, MatchModelService>();
builder.Services.AddScoped<IExcelFileHandler, ExcelFileHandler>();
builder.Services.AddScoped<ITransactionCategoriser, TransactionCategoriser>();

builder.Services.ConfigureClientServices(builder.Configuration);

builder.Services.AddControllers();

// Add a CORS policy for the client
// Add .AllowCredentials() for apps that use an Identity Provider for authn/z
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"]!, builder.Configuration["FrontendUrl"]!])
            .AllowAnyMethod()
            .AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapControllers();

// Activate the CORS policy
app.UseCors("wasm");

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AccountProcessor.Client._Imports).Assembly);

app.Run();
