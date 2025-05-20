using Microsoft.Extensions.DependencyInjection;
using Techno.Authentication.Interfaces;

namespace Techno.Authentication
{
    internal class TechnoAuthenticationBuilder : ITechnoAuthenticationBuilder
    {
        public IServiceCollection Services { get; }

        public TechnoAuthenticationBuilder(IServiceCollection services)
        {
            this.Services = services;
        }
    }
}
