using OpenBeerMenu.Services;

namespace OpenBeerMenu
{
    public class ExternalSyncClientFactory : ServiceBase
    {
        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalSyncClient> _clientLogger;

        public ExternalSyncClientFactory(IServiceProvider serviceProvider, ILogger<ExternalSyncClientFactory> logger) : base(logger)
        {
            _settingsService = serviceProvider.GetRequiredService<SettingsService>();
            _httpClient = serviceProvider.GetRequiredService<HttpClient>();
            _clientLogger = serviceProvider.GetRequiredService<ILogger<ExternalSyncClient>>();
        }

        public async Task<ExternalSyncClient> CreateAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();
            
            return new ExternalSyncClient(_httpClient, settings.SyncUrl, settings.SyncKey, _clientLogger);
        }
    }
}
