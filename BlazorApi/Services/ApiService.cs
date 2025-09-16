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

    public async Task<string> UploadMemeAsync(IBrowserFile file, List<string> tags, string name, string jwtToken)
    {
        if (tags.Count < 1) return "No tags provided!";

        try
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream(maxAllowedSize: 15_000_000);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "File", file.Name);
            content.Add(new StringContent(name), "Name");

            foreach (var tag in tags)
                content.Add(new StringContent(tag), "Tags");

            using var request = new HttpRequestMessage(HttpMethod.Post, "memes/upload");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode) return "New meme uploaded!";

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "Error uploading meme: " + ex.Message;
        }
    }

    public async Task<string> DeleteMemeAsync(int id, string jwtToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"memes/delete?id={id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return "Deleting meme was not successful: " + response.ReasonPhrase +
                       await response.Content.ReadAsStringAsync();
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
                return "Registering user was not successful: " + response.ReasonPhrase +
                       response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error registering user: " + ex.Message);
            return "Error registering user: " + ex.Message;
        }

        return "User created!";
    }

    public async Task<(string msg, string? token)> LoginUserAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Users/login", new
            {
                email,
                password
            });

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<User.LoginResponseDto>();
                if (loginResponse == null) return ("Error logging in user: Login response is null", null);
                return (loginResponse.message, loginResponse.token);
            }

            return ("Error logging in user: " + response.ReasonPhrase, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ("Error logging in user: " + ex.Message, null);
        }
    }

    public async Task<(string msg, MemesStatsDto? memeStats)> GetMemeStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("memes/stats");
            if (response.IsSuccessStatusCode)
            {
                var memeStats = await response.Content.ReadFromJsonAsync<MemesStatsDto>();
                return ("Successfully gotten meme stats!", memeStats);
            }

            return ("Error getting meme stats: " + response.ReasonPhrase + response.Content.ReadAsStringAsync().Result,
                null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ("Exception caught getting meme stats", null);
        }
    }

    public async Task<(string msg, TagsStatsDto? tagStats)> GetTagsStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("tags/stats");
            if (response.IsSuccessStatusCode)
            {
                var tagsStats = await response.Content.ReadFromJsonAsync<TagsStatsDto>();
                return ("Successfully gotten meme stats!", tagsStats);
            }

            return ("Error getting tags stats: " + response.ReasonPhrase, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ("Generel exception caught getting tags stats", null);
        }
    }

    public async Task<(string msg, List<Meme>? memes)> GetMemesPageAsync(int page)
    {
        try
        {
            var response = await _httpClient.GetAsync($"memes/page/{page}");
            if (response.IsSuccessStatusCode)
            {
                var memes = await response.Content.ReadFromJsonAsync<List<Meme>>();
                return ("Successfully gotten memes!", memes);
            }

            return ("Error getting memes: " + response.ReasonPhrase + response.Content.ReadAsStringAsync().Result,
                null);
        }
        catch (Exception ex)
        {
            return ("Error getting memes: " + ex.Message, null);
        }
    }
}