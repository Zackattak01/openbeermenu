using Newtonsoft.Json;

namespace OpenBeerMenu.Data.Sync
{
    public class SyncResponseModel
    {
        [JsonProperty("requested_images")]
        public string[] RequestedImages { get; set; }
    }
}
