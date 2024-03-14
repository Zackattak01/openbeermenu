using Microsoft.AspNetCore.StaticFiles;

namespace OpenBeerMenu.Services
{
    public class ImageService : ServiceBase, IHostedService
    {
        private const string ImagesDirectory = "images";
        private const string LogoDirectoryPath = "logo";
        private const string ThumbnailDirectoryPath = "beers";

        private readonly string _contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        private readonly TimeSpan _imageCleanupInterval = TimeSpan.FromHours(3);

        private readonly BeerInfoService _beerInfoService;
        private readonly SettingsService _settingsService;

        public ImageService(ILogger<ImageService> logger, BeerInfoService beerInfoService, SettingsService settingsService) : base(logger)
        {
            _beerInfoService = beerInfoService;
            _settingsService = settingsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting image cleanup background task.");
            
            EnsureDirectoriesExist();
            _ = StartImageCleanupAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StartImageCleanupAsync()
        {
            while (true)
            {
                await CleanupImagesAsync();
                await Task.Delay(_imageCleanupInterval);
            }
        }

        public async Task CleanupImagesAsync()
        {
            try
            {
                Logger.LogInformation("Searching for abandoned images...");
                
                var referencedImages = new HashSet<string>();

                var beerThumbnails = (await _beerInfoService.GetBeersAsync()).Select(x => x.ThumbnailUrl).Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (var beerThumbnail in beerThumbnails)
                    referencedImages.Add(beerThumbnail);

                var logo = (await _settingsService.GetSettingsAsync())?.LogoUrl;
                
                if (!string.IsNullOrWhiteSpace(logo))
                    referencedImages.Add(logo);

                var allImages = new HashSet<string>();
                
                var beerImages = EnumerateBeerImages().Select(x => x.Name);
                foreach (var beerImage in beerImages)
                {
                    var urlPath = Path.Combine(ImagesDirectory, ThumbnailDirectoryPath, beerImage);
                    allImages.Add(urlPath);
                }
                
                var logoImages = EnumerateLogoImages().Select(x => x.Name);
                foreach (var logoImage in logoImages)
                {
                    var urlPath = Path.Combine(ImagesDirectory, LogoDirectoryPath, logoImage);
                    allImages.Add(urlPath);
                }

                var deletedImageCount = 0;
                foreach (var image in allImages)
                {
                    if (!referencedImages.Contains(image))
                    {
                        DeleteImage(image);
                        deletedImageCount++;
                    }
                }
                
                Logger.LogInformation("Found and deleted {0} abandoned images.", deletedImageCount);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occurred during the image cleanup task's execution.");
            }
        }

        public async Task<string> UploadLogoAsync(string oldLogoUrl, string newImageName, Stream fileStream)
        {
            return await UploadImageAsync(oldLogoUrl, LogoDirectoryPath, newImageName, fileStream);
        }

        public async Task<string> UploadThumbnailAsync(string oldThumbnailUrl, string newImageName, Stream fileStream)
        {
            return await UploadImageAsync(oldThumbnailUrl, ThumbnailDirectoryPath, newImageName, fileStream);
        }

        public void DeleteImage(string imageUrl)
        {
            var logoPath = Path.Combine(_contentRoot, imageUrl);
            if (File.Exists(logoPath))
                File.Delete(logoPath);
        }

        public IEnumerable<FileInfo> EnumerateBeerImages()
            => new DirectoryInfo(Path.Combine(_contentRoot, ImagesDirectory, ThumbnailDirectoryPath)).EnumerateFiles();

        public IEnumerable<FileInfo> EnumerateLogoImages()
            => new DirectoryInfo(Path.Combine(_contentRoot, ImagesDirectory, LogoDirectoryPath)).EnumerateFiles();

        private async Task<string> UploadImageAsync(string oldUrl, string subPath, string newImageName, Stream fileStream)
        {
            if (!string.IsNullOrWhiteSpace(oldUrl))
                DeleteImage(oldUrl);

            var index = newImageName.LastIndexOf('.');
            var newImageExtension = newImageName[index..];
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            
            // if the extracted extension is unrecognized then we dont want to create the image
            if (!contentTypeProvider.Mappings.TryGetValue(newImageExtension, out _))
                return null;
            
            // we dont want the random extension so grab everything except the last 4 chars
            var randomName = Path.GetRandomFileName()[..^4] + newImageExtension;
            var path = Path.Combine(_contentRoot, ImagesDirectory, subPath, randomName);
            
            await using var fs = new FileStream(path, FileMode.CreateNew);
            await fileStream.CopyToAsync(fs);
            
            // return a relative url
            return Path.Combine(ImagesDirectory, subPath, randomName);
        }


        private void EnsureDirectoriesExist()
        {
            var imagesPath = Path.Combine(_contentRoot, ImagesDirectory);
            if (!Directory.Exists(imagesPath))
                Directory.CreateDirectory(imagesPath);

            var logoPath = Path.Combine(imagesPath, LogoDirectoryPath);
            if (!Directory.Exists(logoPath))
                Directory.CreateDirectory(logoPath);

            var beersPath = Path.Combine(imagesPath, ThumbnailDirectoryPath);
            if (!Directory.Exists(beersPath))
                Directory.CreateDirectory(beersPath);
        }
    }
}
