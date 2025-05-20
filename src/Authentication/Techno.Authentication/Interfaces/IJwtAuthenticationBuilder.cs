using Microsoft.Extensions.DependencyInjection;

namespace Techno.Authentication.Interfaces
{
    public interface IJwtAuthenticationBuilder
    {
        IServiceCollection Services { get; }
    }
}
