using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FlowRight.Core.Serialization;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Program class for integration testing host setup.
/// This is required for WebApplicationFactory to work properly with .NET 6+ hosting model.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Run();
    }

    public static WebApplication CreateHostBuilder(string[] args)
    {
        WebApplicationOptions options = new()
        {
            ApplicationName = typeof(Program).Assembly.FullName,
            ContentRootPath = Directory.GetCurrentDirectory(),
            Args = args
        };
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
        
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                
                // Register Result JSON converters
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<int>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<string>());
            });
            
        builder.Services.AddProblemDetails();

        WebApplication app = builder.Build();

        app.UseRouting();
        app.MapControllers();

        return app;
    }
}