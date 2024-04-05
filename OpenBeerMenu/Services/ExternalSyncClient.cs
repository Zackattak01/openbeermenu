using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using OpenBeerMenu.Data.Sync;
using OpenBeerMenu.Sync;

namespace OpenBeerMenu.Services
{
    public class ExternalSyncClient
    { 
        private const string SyncEndpoint = "openbeermenu/sync";
        private const string ImageUploadEndpoint = "openbeermenu/sync/images";

        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalSyncClient> _logger;

        private string _baseUrl;
        private string _key;


        public ExternalSyncClient(HttpClient httpClient, string baseUrl, string key, ILogger<ExternalSyncClient> logger)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _key = key;
            _logger = logger;
        }


        public async Task<SyncRequestResult<SyncResponseModel>> SendSyncAsync(SyncModel model)
        {
            var json = SerializeModel(model);
            
            var request = new HttpRequestMessage(HttpMethod.Post, Path.Join(_baseUrl, SyncEndpoint));
            request.Headers.TryAddWithoutValidation("Authorization", _key);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            HttpResponseMessage resp;
            try
            {
                resp = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Encountered an error sending a sync request");
                return new SyncRequestResult<SyncResponseModel>(false, e.Message);
            }

            if (!resp.IsSuccessStatusCode)
            {
                var message = await resp.Content.ReadAsStringAsync();
                _logger.LogError("Encountered an error while sending sync manifest: {0} - {1}", resp.StatusCode, message);
                return new SyncRequestResult<SyncResponseModel>(false, HttpStatusToMessage(resp.StatusCode));
            }
            return new SyncRequestResult<SyncResponseModel>(DeserializeModel<SyncResponseModel>(await resp.Content.ReadAsStringAsync()));
        }

        public async Task<SyncRequestResult> UploadImagesAsync(ImageSyncModel model)
        {
            var content = new MultipartFormDataContent();
            
            foreach (var imgPath in model.ImagePaths)
            {
                var fs = File.OpenRead(imgPath);
                var imageContent = new StreamContent(fs);
                
                content.Add(imageContent, "images[]", imgPath);
            }
            
            var request = new HttpRequestMessage(HttpMethod.Post, Path.Join(_baseUrl, ImageUploadEndpoint));
            request.Headers.TryAddWithoutValidation("Authorization", _key);
            request.Content = content;

            HttpResponseMessage resp;
            try
            {
                resp = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Encountered an error sending a sync request");
                return new SyncRequestResult(false, e.Message);
            }

            if (!resp.IsSuccessStatusCode)
            {
                var message = await resp.Content.ReadAsStringAsync();
                _logger.LogError("Encountered an error while uploading images: {0} - {1}", resp.StatusCode, message);
                return new SyncRequestResult(false, HttpStatusToMessage(resp.StatusCode));
            }

            return SyncRequestResult.Success;
        }

        private string SerializeModel(object model)
        {
            return JsonConvert.SerializeObject(model);
        }

        private T DeserializeModel<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        private string HttpStatusToMessage(HttpStatusCode status) => status switch
        {
            HttpStatusCode.BadRequest => "Bad request. Data could be mismatched",
            HttpStatusCode.Unauthorized => "Unathorized. Sync key was rejected by the server",
            HttpStatusCode.NotFound => "Not found. Could not find a sync server at supplied url",
            _ => "Unknown"
        };
    }

}
