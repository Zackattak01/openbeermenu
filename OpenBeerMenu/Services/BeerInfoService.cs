using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Services
{
    public class BeerInfoService : ServiceBase
    {
        private readonly IServiceProvider _serviceProvider;

        public event Func<Task> BeersUpdated; 

        public BeerInfoService(IServiceProvider serviceProvider, ILogger<BeerInfoService> logger) 
            : base(logger)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IReadOnlyList<BeerInfo>> GetBeersAsync()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var beers = await dbContext.Beers.ToListAsync();

            return beers;
        }

        public async Task CreateBeerAsync(BeerInfo beer)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            dbContext.Beers.Add(beer);
            await dbContext.SaveChangesAsync();
            await OnBeersUpdated();
        }

        public async Task UpdateBeerAsync(BeerInfo beer)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            dbContext.Update(beer);
            await dbContext.SaveChangesAsync();
            await OnBeersUpdated();
        }

        public async Task RemoveBeerAsync(BeerInfo beer)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            dbContext.Remove(beer);
            
            await dbContext.SaveChangesAsync();
            await OnBeersUpdated();
        }

        private async Task OnBeersUpdated()
        {
            if (BeersUpdated is not null)
                await BeersUpdated.Invoke();
        }
    }
}