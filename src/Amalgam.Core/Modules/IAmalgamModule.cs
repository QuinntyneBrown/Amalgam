using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Amalgam.Core.Modules;

public interface IAmalgamModule
{
    string ServiceName { get; }
    void ConfigureServices(IServiceCollection services);
    void ConfigureApp(IApplicationBuilder app);
}
