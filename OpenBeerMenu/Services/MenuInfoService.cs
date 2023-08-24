using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Services
{
    public class MenuInfoService : ServiceBase
    {
        private readonly IServiceProvider _serviceProvider;

        public event Func<Task> MenusUpdated;

        public MenuInfoService(IServiceProvider serviceProvider, ILogger<MenuInfoService> logger) : base(logger)
        {
            _serviceProvider = serviceProvider;

            var sectionInfoService = _serviceProvider.GetRequiredService<SectionInfoService>();
            sectionInfoService.SectionsUpdated += OnMenusUpdatedAsync;
        }
        
        public async Task<IReadOnlyList<MenuInfo>> GetMenusAsync()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var menus = await dbContext.Menus.AsNoTracking()
                .Include(x => x.MenuSections)
                .ThenInclude(x => x.Section)
                .ThenInclude(x => x.SectionBeers)
                .ThenInclude(x => x.Beer)
                .ToListAsync();

            return menus;
        }

        public async Task CreateMenuAsync(MenuInfo menu)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(menu).State = EntityState.Added;
            
            foreach (var menuSection in menu.MenuSections)
            {
                dbContext.Entry(menuSection).State = EntityState.Added;
            }
            
            MarkSectionsUnchanged(menu, dbContext); 

           
            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        public async Task UpdateMenuAsync(MenuInfo menu)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            dbContext.Entry(menu).State = EntityState.Modified;

            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        public async Task RemoveMenuAsync(MenuInfo menu)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var settings = await dbContext.Settings.Include(x => x.DefaultMenu).SingleOrDefaultAsync();
            if (settings.DefaultMenu.Id == menu.Id)
            {
                settings.DefaultMenu = null;
                await dbContext.SaveChangesAsync();
            }
            
            dbContext.ChangeTracker.Clear();
            dbContext.Remove(menu);

            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        public async Task AddSectionToMenuAsync(MenuInfo menu, SectionInfo section)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            var newMenuSection = new MenuSection
            {
                Menu = menu,
                Section = section,
                MenuId = menu.Id,
                SectionId = section.Id,
                Position = menu.MenuSections.Count
            };
            
            menu.MenuSections.Add(newMenuSection);
            dbContext.Entry(newMenuSection).State = EntityState.Added;

            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        public async Task RemoveSectionFromMenuAsync(MenuInfo menu, SectionInfo section)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();

            var foundSectionToRemove = false;
            foreach (var menuSection in menu.MenuSections)
            {
                if (!foundSectionToRemove && menuSection.SectionId == section.Id)
                {
                    foundSectionToRemove = true;
                    dbContext.Entry(menuSection).State = EntityState.Deleted;
                }
                else if (foundSectionToRemove)
                {
                    menuSection.Position--;
                    dbContext.Entry(menuSection).State = EntityState.Modified;
                }
            }

            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        public async Task ReorderSectionsAsync(MenuInfo menu, IList<SectionInfo> sections)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpenBeerMenuDbContext>();
            
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                var menuSection = menu.MenuSections.Find(x => x.SectionId == section.Id);
        
                if (menuSection.Position != i)
                {
                    menuSection.Position = i;
                    dbContext.Entry(menuSection).State = EntityState.Modified;
                }
            }

            await dbContext.SaveChangesAsync();
            await OnMenusUpdatedAsync();
        }

        private void MarkSectionsUnchanged(MenuInfo menu, OpenBeerMenuDbContext dbContext)
        {
            // ensures we dont try and recreate any sections
            if (menu.Sections is not null)
                foreach (var section in menu.Sections)
                    dbContext.Entry(section).State = EntityState.Unchanged;
        
            if (menu.MenuSections is not null)
                foreach (var menuSection in menu.MenuSections)
                    dbContext.Entry(menuSection.Section).State = EntityState.Unchanged;
        }

        public async Task OnMenusUpdatedAsync()
        {
            if (MenusUpdated is not null) 
                await MenusUpdated.Invoke();
        }
    }
}
