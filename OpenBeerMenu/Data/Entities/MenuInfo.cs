using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBeerMenu.Data.Entities
{
    public class MenuInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<MenuSection> MenuSections { get; set; }
        
        [NotMapped]
        public List<SectionInfo> Sections => MenuSections.OrderBy(x => x.Position).Select(x => x.Section).ToList();

        // EF ctor
        private MenuInfo(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        private MenuInfo(Guid id, string name, List<SectionInfo> sections)
        {
            Id = id;
            Name = name;
            MenuSections = new List<MenuSection>(sections.Count);
            
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                MenuSections.Add(new MenuSection
                {
                    Position = i,
                    SectionId = section.Id,
                    Section = section,
                    MenuId = Id,
                    Menu = this
                });
            }
        }
        
        public MenuInfo(string name, List<SectionInfo> sections)
            : this(Guid.NewGuid(), name, sections) { }
    }
}