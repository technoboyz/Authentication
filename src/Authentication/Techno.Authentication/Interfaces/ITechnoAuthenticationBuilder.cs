using Microsoft.Extensions.DependencyInjection;

namespace Techno.Authentication.Interfaces
{
    public interface ITechnoAuthenticationBuilder
    {
        IServiceCollection Services { get; }
    }
}
