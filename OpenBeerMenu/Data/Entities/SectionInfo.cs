using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBeerMenu.Data.Entities
{
    public class SectionInfo : IEquatable<SectionInfo>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public bool ShowHeader { get; set; }
        
        public List<MenuSection> MenuSections { get; set; }

        public List<SectionBeer> SectionBeers { get; set; }
        
        [NotMapped]
        public List<BeerInfo> Beers => SectionBeers.OrderBy(x => x.Position).Select(x => x.Beer).ToList();

        
        
        // EF ctor
        private SectionInfo(Guid id, string name, bool showHeader)
        {
            Id = id;
            Name = name;
            ShowHeader = showHeader;
        }

        private SectionInfo(Guid id, string name, bool showHeader, List<BeerInfo> beers)
        {
            Id = id;
            Name = name;
            ShowHeader = showHeader;
            MenuSections = new List<MenuSection>();
            SectionBeers = new List<SectionBeer>(beers.Count);
            
            for (var i = 0; i < beers.Count; i++)
            {
                var beer = beers[i];
                SectionBeers.Add(new SectionBeer
                {
                    Position = i,
                    BeerId = beer.Id,
                    Beer = beer,
                    SectionId = Id,
                    Section = this
                });
            }
        }
        
        public SectionInfo(string name, bool showHeader, List<BeerInfo> beers)
            : this (Guid.NewGuid(), name, showHeader, beers) { }

        public bool Equals(SectionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SectionInfo)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
    
}