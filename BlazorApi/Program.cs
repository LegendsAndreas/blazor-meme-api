using System.Text;
using BlazorApi.Components;
using BlazorApi.Data;
using Microsoft.EntityFrameworkCore;
using BlazorApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace BlazorApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration Configuration = builder.Configuration;

        builder.Services.AddScoped<StorageService>(sp => new StorageService(sp.GetRequiredService<IJSRuntime>()));

        // JWT: 
        // Packages: System.IdentityModel.Tokens.Jwt, Microsoft.IdentityModel.JsonWebTokens, Microsoft.AspNetCore.Authentication.JwtBearer
        builder.Services.AddScoped<JwtService>();
        string jwtSecretKey = (Configuration["Jwt:SecretKey"]
                               ?? Environment.GetEnvironmentVariable("JwtSecretKey"))!;

        string jwtIssuer = (Configuration["Jwt:Issuer"]
                            ?? Environment.GetEnvironmentVariable("JwtIssuer"))!;

        string jwtAudience = (Configuration["Jwt:Audience"]
                              ?? Environment.GetEnvironmentVariable("JwtAudience"))!;

        string connectionString = (Configuration.GetConnectionString("DefaultConnection")
                                   ?? Environment.GetEnvironmentVariable("DefaultConnection"))!;

        if (string.IsNullOrEmpty(jwtSecretKey) || string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Secretkey, issuer, audience or connectionstring is not set");
        }

        // Boilerplate for JWT auth
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                };
            });
        builder.Services.AddAuthorization();

        builder.Services.AddDbContext<AppDBContext>(options =>
            options.UseNpgsql(connectionString));

        // CORS boilerplate
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowSpecificOrigins",
                policyBuilder =>
                {
                    policyBuilder
                        .WithOrigins(
                            Configuration["Origin"],
                            "https://localhost:5273",
                            "https://localhost:8080",
                            "https://localhost:7231"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition");
                }
            );
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddControllers();
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:5273/api/");
            Console.WriteLine($"APIService BaseAddress: {client.BaseAddress}");
        });

        var app = builder.Build();
        
        app.UseCors("AllowSpecificOrigins");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.MapControllers();

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        // Authentication FIRST and then Authorization.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}