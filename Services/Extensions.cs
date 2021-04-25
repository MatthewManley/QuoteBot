using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Services
{
    public static class Extensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAudioProcessingService, AudioProcessingService>();
            serviceCollection.AddTransient<IUserService, UserService>();
            return serviceCollection;
        }
    }
}
