using System.Security.Claims;
using Techno.Authentication.Interfaces;

namespace Techno.Authentication.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddProblemDetails();


            builder.Services.AddTechnoAuthentication(builder.Configuration);

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();
            builder.Services.AddOpenApi(options =>
            {
                options.AddTechnoAuthentication(builder.Configuration);
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", builder.Environment.ApplicationName);
                });
            }

            app.UseHttpsRedirection();

            app.UseTechnoAuthentication();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .RequireAuthorization();

            app.MapGet("/users/login", async (IJwtBearerService service) =>
            {
                List<Claim> claims = [ new Claim(ClaimTypes.Role, "Admin")];
                string token = await service.CreateTokenAsync("giovanni", claims);
                return new { Token = token };
            });

            app.Run();
        }
    }
}
