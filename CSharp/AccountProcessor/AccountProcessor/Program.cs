using AccountProcessor.ClientServices;
using AccountProcessor.Components;
using AccountProcessor.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IMatchModelService, MatchModelService>();
builder.Services.AddScoped<IExcelFileHandler, ExcelFileHandler>();
builder.Services.AddScoped<ITransactionCategoriser, TransactionCategoriser>();

builder.Services.AddScoped<IClientExcelFileService, ClientExcelFileService>();
builder.Services.AddScoped<IClientMatchModelService, ClientMatchModelService>();
builder.Services.AddScoped<IClientTransactionCategoriser, ClientTransactionCategoriser>();

builder.Services.AddControllers();

builder.Services.AddSingleton(_ =>
{
    var uri = builder.Configuration["BackendUrl"];
    return new HttpClient { BaseAddress = new Uri(uri!) };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
