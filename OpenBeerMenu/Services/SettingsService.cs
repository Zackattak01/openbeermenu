using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Services
{
    public class SettingsService : ServiceBase
    {
        private const int SettingsId = 0;

        private readonly IServiceProvider _serviceProvider;

        public event Func<Task> SettingsUpdated;
        
        public SettingsService(IServiceProvider serviceProvider, ILogger<SettingsService> logger) : base(logger)
        {
            _serviceProvider = serviceProvider;
            
            var menuInfoService = serviceProvider.GetRequiredService<MenuInfoService>();
            menuInfoService.MenusUpdated += OnSettingsUpdatedAsync;
        }

        public async Task<Settings> GetSettingsAsync()
        {
            return await FetchSettingsAsync();
        }

        public async Task<bool> CheckSettingsExistAsync()
        {
            var settings = await FetchSettingsAsync();

            return settings is not null;
        }

        public async Task CreateSettingsAsync(Settings settings)
        {
            settings.Id = SettingsId;
            
            var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(settings).State = EntityState.Added;

            await dbContext.SaveChangesAsync(); 
            await OnSettingsUpdatedAsync();
        }

        public async Task UpdateSettingsAsync(Settings settings)
        {
            var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(settings).State = EntityState.Modified;
            dbContext.Entry(settings.DefaultMenu).State = EntityState.Modified;

            await dbContext.SaveChangesAsync();

            await OnSettingsUpdatedAsync();
        }

        private async Task<Settings> FetchSettingsAsync()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var settings = await dbContext.Settings
                .Include(x => x.DefaultMenu)
                .ThenInclude(x => x.MenuSections)
                .ThenInclude(x => x.Section)
                .ThenInclude(x => x.SectionBeers)
                .ThenInclude(x => x.Beer)
                .SingleOrDefaultAsync();
            
            return settings;
        }

        private async Task OnSettingsUpdatedAsync()
        {
            if (SettingsUpdated is not null)
                await SettingsUpdated.Invoke();
        }
    }
}