using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using BlazorApi.Models;

namespace BlazorApi.Services;

public class ApiService
{
    public HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> UploadMemeAsync(IBrowserFile file, string name)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            // Read file content to stream
            var stream = file.OpenReadStream(maxAllowedSize: 10_000_000); // 10 MB max
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            // Create file content
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync($"memes/upload?name={name}", content);


            if (response.IsSuccessStatusCode)
            {
                return "New meme uploaded!";
            }

            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "Error uploading meme: " + ex.Message;
        }
    }

    public async Task<string> DeleteMemeAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"memes/delete?id={id}");
            if (!response.IsSuccessStatusCode)
            {
                return "Deleting meme was not successful: " + response.ReasonPhrase + response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting message: " + ex.Message);
            return "Error deleting meme: " + ex.Message;
        }
        
        return "Meme deleted!";
    }

    public async Task<string> RegisterUser(User.RegisterDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Users/register", new
            {
                email = dto.Email,
                password = dto.Password,
                confirmPassword = dto.ConfirmPassword
            });

            if (!response.IsSuccessStatusCode)
            {
                return "Registering user was not successful: " + response.ReasonPhrase + response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error registering user: " + ex.Message);
            return "Error registering user: " + ex.Message;
        }
        
        return "User created!";
    }
}