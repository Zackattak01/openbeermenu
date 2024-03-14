using OpenBeerMenu.Data.Entities;
using OpenBeerMenu.Data.Sync;
using OpenBeerMenu.Types;
using OpenBeerMenu.Types.Sync;

namespace OpenBeerMenu.Services
{
    public class ExternalSyncService : ServiceBase, IHostedService
    {
        private int TwoMinutesSpan = (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
        private DelayedTask _syncTask;

        private readonly MenuInfoService _menuInfoService;
        private readonly ImageService _imageService;
        private readonly SettingsService _settingsService;
        private readonly ExternalSyncClientFactory _syncClientFactory;

        public SyncStatus Status { get; private set; }

        public ExternalSyncService(MenuInfoService menuInfoService, ImageService imageService, SettingsService settingsService, ExternalSyncClientFactory syncClientFactory, ILogger<ExternalSyncService> logger) : base(logger)
        {
            _menuInfoService = menuInfoService;
            _imageService = imageService;
            _syncClientFactory = syncClientFactory;
            _settingsService = settingsService;

            Status = SyncStatus.Waiting;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _menuInfoService.MenusUpdated += OnMenusUpdated;
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _menuInfoService.MenusUpdated -= OnMenusUpdated;
            return Task.CompletedTask;
        }

        public async Task ForceSyncAsync()
        {
            if (_syncTask != null && !_syncTask.IsComplete)
                _syncTask.Cancel();

            await SyncToExternalServicesAsync();
        }

        private async Task OnMenusUpdated()
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (!settings.SyncEnabled)
                return;

            if (_syncTask == null || _syncTask.IsComplete)
            {
                Logger.LogInformation("Creating sync task");
                _syncTask = new DelayedTask(SyncToExternalServicesAsync, TwoMinutesSpan);
            }
            else
            {
                Logger.LogInformation("Postponing sync task");
                _syncTask.Postpone();
            }

            Status = SyncStatus.Scheduled;
        }

        private async Task SyncToExternalServicesAsync()
        {
            Logger.LogInformation("Starting external sync");
            await _imageService.CleanupImagesAsync();

            var settings = await _settingsService.GetSettingsAsync();
            var menuInfos = await _menuInfoService.GetMenusAsync();
            var (beers, sections, menus) = MapToTransferModels(menuInfos);

            var syncModel = new SyncModel
            {
                DefaultMenuId = settings.DefaultMenu.Id,
                Beers = beers,
                Sections = sections,
                Menus = menus,
            };

            var client = await _syncClientFactory.CreateAsync();
            var syncResp = await client.SendSyncAsync(syncModel);

            if (!syncResp.IsSuccess)
            {
                Status = SyncStatus.Failed(syncResp.Message);
                return;
            }

            var respModel = syncResp.Value;

            if (respModel.RequestedImages.Length == 0)
            {
                Logger.LogInformation("External sync complete. Server requested no images");
                Status = SyncStatus.Success;
                return;
            }

            Logger.LogInformation("Server requested the following images: {0}", string.Join(", ", respModel?.RequestedImages));
            var allImages = _imageService.EnumerateBeerImages();
            var imageSyncModel = new ImageSyncModel
            {
                ImagePaths = allImages.IntersectBy(respModel.RequestedImages, x => x.Name).Select(x => x.FullName)
            };

            var imageResp = await client.UploadImagesAsync(imageSyncModel);
            if (!imageResp.IsSuccess)
            {
                Status = SyncStatus.Failed(syncResp.Message);
                return;
            }

            Status = SyncStatus.Success;
            Logger.LogInformation("Finished external sync");
        }

        private (IEnumerable<BeerModel> Beers, IEnumerable<SectionModel> Sections, IEnumerable<MenuModel> Menus) MapToTransferModels(IEnumerable<MenuInfo> menuInfos)
        {
            var sectionInfos = menuInfos.SelectMany(x => x.Sections).DistinctBy(x => x.Id);
            var beerInfos = sectionInfos.SelectMany(x => x.Beers).DistinctBy(x => x.Id);

            var menuModels = menuInfos.Select(MenuModel.MapFromInfo);
            var sectionModels = sectionInfos.Select(SectionModel.MapFromInfo);
            var beerModels = beerInfos.Select(BeerModel.MapFromInfo);

            return (beerModels, sectionModels, menuModels);
        }

    }
}
