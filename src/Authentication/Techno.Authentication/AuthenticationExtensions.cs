using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Techno.Authentication.Interfaces;

namespace Techno.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddTechnoAuthentication(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication", bool addAuthorizationServices = true)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(sectionName);

            var defaultAuthenticationScheme = configuration.GetValue<string>($"{sectionName}:DefaultScheme");

            var builder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = defaultAuthenticationScheme;
                options.DefaultChallengeScheme = defaultAuthenticationScheme;
            });

            return builder.AddTechnoAuthentication(configuration, sectionName, addAuthorizationServices);
        }

        public static AuthenticationBuilder AddTechnoAuthentication(this AuthenticationBuilder builder, IConfiguration configuration, string sectionName = "Authentication", bool addAuthorizationServices = true)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(sectionName);

            if (addAuthorizationServices)
            {
                builder.Services.AddAuthorization();
            }

            CheckAddJwtBearer(builder, configuration.GetSection($"{sectionName}:JwtBearer"));

            return builder;

            static void CheckAddJwtBearer(AuthenticationBuilder builder, IConfigurationSection section)
            {
                var settings = section.Get<JwtBearerSettings>();
                if (settings is null)
                {
                    return;
                }

                ArgumentNullException.ThrowIfNull(settings.SchemeName, nameof(JwtBearerSettings.SchemeName));
                ArgumentNullException.ThrowIfNull(settings.SecurityKey, nameof(JwtBearerSettings.SecurityKey));
                ArgumentNullException.ThrowIfNull(settings.Algorithm, nameof(JwtBearerSettings.Algorithm));
                ArgumentNullException.ThrowIfNull(settings.NameClaimType, nameof(JwtBearerSettings.NameClaimType));
                ArgumentNullException.ThrowIfNull(settings.RoleClaimType, nameof(JwtBearerSettings.RoleClaimType));

                builder.Services.Configure<JwtBearerSettings>(section);

                builder.AddJwtBearer(settings.SchemeName, options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        AuthenticationType = settings.SchemeName,
                        NameClaimType = settings.NameClaimType,
                        RoleClaimType = settings.RoleClaimType,
                        ValidateIssuer = settings.Issuers?.Length > 0,
                        ValidIssuers = settings.Issuers,
                        ValidateAudience = settings.Audiences?.Length > 0,
                        ValidAudiences = settings.Audiences,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecurityKey)),
                        RequireExpirationTime = true,
                        ValidateLifetime = settings.ExpirationTime.GetValueOrDefault() > TimeSpan.Zero,
                        ClockSkew = settings.ClockSkew
                    };
                });

                if (settings.EnableJwtBearerService)
                {
                    builder.Services.TryAddSingleton<IJwtBearerService, JwtBearerService>();
                }
            }
        }

        public static IJwtAuthenticationBuilder WithJwtBearer(this ITechnoAuthenticationBuilder builder, IConfiguration configuration)
        {
            var section = configuration.GetSection(nameof(JwtBearerSettings)) ??
                throw new NullReferenceException("JwtSettins values not found");
            var jwtBearerSettings = section.Get<JwtBearerSettings>()!;
            builder.Services.Configure<JwtBearerSettings>(section);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = jwtBearerSettings.Issuers?.Any() ?? false,
                    ValidIssuers = jwtBearerSettings.Issuers,
                    ValidateAudience = jwtBearerSettings.Audiences?.Any() ?? false,
                    ValidAudiences = jwtBearerSettings.Audiences,
                    ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(jwtBearerSettings.SecurityKey),
                    IssuerSigningKey = string.IsNullOrWhiteSpace(jwtBearerSettings.SecurityKey) ?
                    null :
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerSettings.SecurityKey)),
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            return new JwtAuthenticationBuilder(builder.Services);
        }

        public static IJwtAuthenticationBuilder AddTokenGenerator(this IJwtAuthenticationBuilder builder)
        {
            return builder;
        }

        public static IApplicationBuilder UseTechnoAuthentication(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static void AddTechnoAuthentication(this OpenApiOptions options, IConfiguration configuration, string sectionName = "Authentication")
            => options.AddTechnoAuthentication(configuration, sectionName, Array.Empty<OpenApiSecurityRequirement>());

        public static void AddTechnoAuthentication(this OpenApiOptions options, IConfiguration configuration, params IEnumerable<string> additionalSecurityDefinitionNames)
            => options.AddTechnoAuthentication(configuration, "Authentication", additionalSecurityDefinitionNames);

        public static void AddTechnoAuthentication(this OpenApiOptions options, IConfiguration configuration, string sectionName, params IEnumerable<string> additionalSecurityDefinitionNames)
        {
            var securityRequirements = additionalSecurityDefinitionNames?.Select(OpenApiHelpers.CreateSecurityRequirement).ToArray();
            options.AddTechnoAuthentication(configuration, sectionName, securityRequirements ?? []);
        }

        public static void AddTechnoAuthentication(this OpenApiOptions options, IConfiguration configuration, params IEnumerable<OpenApiSecurityRequirement> securityRequirements)
            => options.AddTechnoAuthentication(configuration, "Authentication", securityRequirements);

        public static void AddTechnoAuthentication(this OpenApiOptions options, IConfiguration configuration, string sectionName, params IEnumerable<OpenApiSecurityRequirement> additionalSecurityRequirements)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(sectionName);

            options.AddDocumentTransformer(new AuthenticationDocumentTransformer(configuration, sectionName, additionalSecurityRequirements));
            options.AddDocumentTransformer<DefaultResponseDocumentTransformer>();
            options.AddOperationTransformer<AuthenticationOperationTransformer>();
        }
    }
}
