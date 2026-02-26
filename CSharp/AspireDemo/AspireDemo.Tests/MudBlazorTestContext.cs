using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace AspireDemo.Tests;

/// <summary>
/// Base class that registers MudBlazor services and wires up async teardown.
///
/// Why IAsyncLifetime?
/// MudBlazor registers KeyInterceptorService which only implements IAsyncDisposable.
/// xUnit v2 calls the synchronous Dispose() path on test classes, which throws when
/// it encounters async-only services. Implementing IAsyncLifetime makes xUnit call
/// DisposeAsync() first; overriding Dispose(bool) to a no-op prevents the sync path
/// from running afterwards and throwing.
/// </summary>
public abstract class MudBlazorTestContext : BunitContext, IAsyncLifetime
{
    protected MudBlazorTestContext()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await ((IAsyncDisposable)this).DisposeAsync();

    protected override void Dispose(bool disposing) { /* async path handles cleanup */ }
}
