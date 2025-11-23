using BibleApp.Client.ClientServices;
using Microsoft.AspNetCore.Components;

namespace BibleApp.Client.Pages;

public partial class Home
{
    [Inject] private IBibleService BibleService { get; init; } = null!;
        
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var books = await BibleService.GetBooksAsync();
    }
}