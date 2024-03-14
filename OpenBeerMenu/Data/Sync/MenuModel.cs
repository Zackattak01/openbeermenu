using Newtonsoft.Json;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Data.Sync
{
    public class MenuModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("section_ids")]
        public IEnumerable<Guid> SectionIds { get; set; }

        public static MenuModel MapFromInfo(MenuInfo info)
        {
            var model = new MenuModel();
            model.Id = info.Id;
            model.Name = info.Name;
            model.SectionIds = info.Sections.Select(x => x.Id);

            return model;
        }
    }
}
