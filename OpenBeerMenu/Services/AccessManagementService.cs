using OpenBeerMenu.Types;

namespace OpenBeerMenu.Services
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public class AccessManagementService : ServiceBase
    {
        private readonly SettingsService _settingsService;
        
        public bool IsAuthorized { get; private set; } = false;
        
        public AccessManagementService(SettingsService settingsService, ILogger<AccessManagementService> logger) : base(logger)
        {
            _settingsService = settingsService;
        }

        public async Task<bool> TryAuthorizeAsync(string accessCode)
        {
            var settings = await _settingsService.GetSettingsAsync();

            if (settings.AccessCode == accessCode)
            {
                IsAuthorized = true;
                return IsAuthorized;
            }

            return false;
        }
    }
}