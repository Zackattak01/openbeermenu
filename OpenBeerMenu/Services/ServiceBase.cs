namespace OpenBeerMenu.Services
{
    public class ServiceBase
    {
        protected ILogger Logger { get; }
        
        public ServiceBase(ILogger logger)
        {
            Logger = logger;
        }
    }
}