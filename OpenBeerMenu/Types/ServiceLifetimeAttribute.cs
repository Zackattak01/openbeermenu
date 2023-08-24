namespace OpenBeerMenu.Types
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceLifetimeAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public ServiceLifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}