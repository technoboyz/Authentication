using Microsoft.Extensions.DependencyInjection;
using Techno.Authentication.Interfaces;

namespace Techno.Authentication
{
    internal class JwtAuthenticationBuilder : IJwtAuthenticationBuilder
    {
        public IServiceCollection Services { get; }

        public JwtAuthenticationBuilder(IServiceCollection services)
        {
            Services = services;

        }
    }
}
