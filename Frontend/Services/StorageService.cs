using Microsoft.JSInterop;

namespace Frontend.Services;

public class StorageService
{
    private readonly IJSRuntime _jsRuntime;

    public StorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveItemToLocalStorageAsync(string name, string value)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.saveItemToLocal", name, value);
    }

    public async Task<string?> GetItemFromLocalStorageAsync(string name)
    {
        string? token = await _jsRuntime.InvokeAsync<string?>("loginHelpers.getItemFromLocal", name);
        return token;
    }

    public async Task RemoveItemFromLocalStorageAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("loginHelpers.deleteItemFromLocal", name);
    }
}