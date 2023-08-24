using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;

namespace OpenBeerMenu.Services
{
    public class BeerStyleService : ServiceBase
    {
        private readonly IServiceProvider _serviceProvider;
        
        public event Func<Task> StylesUpdated;

        public BeerStyleService(IServiceProvider serviceProvider, ILogger<BeerStyleService> logger) : base(logger)
        {
            _serviceProvider = serviceProvider;

            var beerInfoService = _serviceProvider.GetRequiredService<BeerInfoService>();
            beerInfoService.BeersUpdated += OnStylesUpdated;
        }

        public async Task<IReadOnlySet<string>> GetStylesAsync()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            var styles = (await dbContext.Beers.Select(x => x.Style).Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToListAsync()).ToHashSet();

            return styles;
        }

        private async Task OnStylesUpdated()
        {
            if (StylesUpdated is not null)
                await StylesUpdated.Invoke();
        }
    }
}