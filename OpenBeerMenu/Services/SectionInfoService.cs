using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Services
{
    public class SectionInfoService : ServiceBase
    {
        private readonly IServiceProvider _serviceProvider;

        public event Func<Task> SectionsUpdated; 

        public SectionInfoService(IServiceProvider serviceProvider, ILogger<SectionInfoService> logger) : base(logger)
        {
            _serviceProvider = serviceProvider;

            var beerInfoService = _serviceProvider.GetRequiredService<BeerInfoService>();
            beerInfoService.BeersUpdated += OnSectionsUpdatedAsync;
        }
        
        public async Task<IReadOnlyList<SectionInfo>> GetSectionsAsync()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var sections = await dbContext.Sections.Include(x => x.SectionBeers).ThenInclude(x => x.Beer).AsNoTracking().ToListAsync();

            return sections;
        }

        public async Task CreateSectionAsync(SectionInfo section)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(section).State = EntityState.Added;
            
            foreach (var sectionBeer in section.SectionBeers) 
                dbContext.Entry(sectionBeer).State = EntityState.Added;

            MarkBeersUnchanged(section, dbContext);
            
            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        public async Task UpdateSectionAsync(SectionInfo section)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(section).State = EntityState.Modified;

            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        public async Task RemoveSectionAsync(SectionInfo section)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            dbContext.Remove(section);

            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        public async Task AddBeerToSectionAsync(SectionInfo section, BeerInfo beer)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            var newSectionBeer = new SectionBeer
            {
                Section = section,
                Beer = beer,
                SectionId = section.Id,
                BeerId = beer.Id,
                Position = section.SectionBeers.Count
            };
            
            section.SectionBeers.Add(newSectionBeer);
            dbContext.Entry(newSectionBeer).State = EntityState.Added;

            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        public async Task RemoveBeerFromSectionAsync(SectionInfo section, BeerInfo beer)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var foundBeerToRemove = false;
            foreach (var sectionBeer in section.SectionBeers)
            {
                if (!foundBeerToRemove && sectionBeer.BeerId == beer.Id)
                {
                    foundBeerToRemove = true;
                    dbContext.Entry(sectionBeer).State = EntityState.Deleted;
                }
                else if (foundBeerToRemove)
                {
                    sectionBeer.Position--;
                    dbContext.Entry(sectionBeer).State = EntityState.Modified;
                }
            }

            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        public async Task ReorderBeersAsync(SectionInfo section, IList<BeerInfo> beers)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            for (var i = 0; i < beers.Count; i++)
            {
                var beer = beers[i];
                var sectionBeer = section.SectionBeers.Find(x => x.BeerId == beer.Id);
        
                if (sectionBeer.Position != i)
                {
                    sectionBeer.Position = i;
                    dbContext.Entry(sectionBeer).State = EntityState.Modified;
                }
            }

            await dbContext.SaveChangesAsync();
            await OnSectionsUpdatedAsync();
        }

        private void MarkBeersUnchanged(SectionInfo section, OpenBeerMenuDbContext dbContext)
        {
            // ensures we dont try and recreate any beers
            if (section.Beers is not null)
                foreach (var beer in section.Beers)
                    dbContext.Entry(beer).State = EntityState.Unchanged;
        
            if (section.SectionBeers is not null)
                foreach (var sectionBeer in section.SectionBeers)
                    dbContext.Entry(sectionBeer.Beer).State = EntityState.Unchanged;
        }

        private async Task OnSectionsUpdatedAsync()
        {
            if (SectionsUpdated is not null)
                await SectionsUpdated.Invoke();
        }
    }
}