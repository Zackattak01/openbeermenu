using Newtonsoft.Json;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Data.Sync
{
    public class SectionModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("show_header")]
        public bool ShowHeader { get; set; }

        [JsonProperty("beer_ids")]
        public IEnumerable<Guid> BeerIds { get; set; }

        public static SectionModel MapFromInfo(SectionInfo info)
        {
            var model = new SectionModel();
            model.Id = info.Id;
            model.Name = info.Name;
            model.ShowHeader = info.ShowHeader;
            model.BeerIds = info.Beers.Select(x => x.Id);

            return model;
        }
    }
}
