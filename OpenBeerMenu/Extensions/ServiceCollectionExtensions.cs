using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenBeerMenu.Services;
using OpenBeerMenu.Types;

namespace OpenBeerMenu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenBeerServices(this IServiceCollection services)
        {
            var serviceBaseType = typeof(ServiceBase);
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(serviceBaseType) && !x.IsAbstract);
            
            foreach (var type in types)
            {
                var lifetimeAttribute = type.GetCustomAttributes().OfType<ServiceLifetimeAttribute>().SingleOrDefault();

                if (type.IsAssignableTo(typeof(IHostedService)))
                    services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), type));

                services.Add(new ServiceDescriptor(type, type, lifetimeAttribute?.Lifetime ?? ServiceLifetime.Singleton));
            }
            
            return services;
        }
    }
}