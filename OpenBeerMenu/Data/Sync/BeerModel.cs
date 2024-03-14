using Newtonsoft.Json;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Data.Sync
{
    public class BeerModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("style")]
        public string Style { get; set; }

        [JsonProperty("thumbnail_name")]
        public string ThumbnailName { get; set; }

        [JsonProperty("abv")]
        public double Abv { get; set; }

        public static BeerModel MapFromInfo(BeerInfo info)
        {
            var model = new BeerModel();
            model.Id = info.Id;
            model.Name = info.Name;
            model.Description = info.Description;
            model.Style = info.Style;
            model.ThumbnailName = info.ThumbnailUrl.Split('/').Last(); // grab just the name of the file
            model.Abv = info.Abv;

            return model;
        }
    }
}
