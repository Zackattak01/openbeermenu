using Newtonsoft.Json;

namespace OpenBeerMenu.Data.Sync
{
    public class SyncModel
    {
        [JsonProperty("default_menu_id")]
        public Guid DefaultMenuId { get; set; }

        [JsonProperty("menus")]
        public IEnumerable<MenuModel> Menus { get; set; }

        [JsonProperty("sections")]
        public IEnumerable<SectionModel> Sections { get; set; }

        [JsonProperty("beers")]
        public IEnumerable<BeerModel> Beers { get; set; }
    }
}
