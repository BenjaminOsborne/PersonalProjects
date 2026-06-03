using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MudBlazorAuthTemplate.Components.Account;

/// <summary>
/// An <see cref="AuthenticationStateProvider"/> that always returns an authenticated state.
/// Used alongside <see cref="PassThroughAuthHandler"/> when Auth:Enabled is false.
/// </summary>
internal sealed class PassThroughAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AuthenticatedState = new(
        new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "Local User")],
                PassThroughAuthHandler.SchemeName)));

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(AuthenticatedState);
}
