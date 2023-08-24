using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBeerMenu.Data.Entities
{
    public class BeerInfo : IEquatable<BeerInfo>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Style { get; set; }
        
        public string ThumbnailUrl { get; set; }

        public double Abv { get; set; }

        public IList<SectionInfo> Sections { get; set; }
        public List<SectionBeer> SectionBeers { get; set; }
        
        private BeerInfo(Guid id, string name, string description, string style, string thumbnailUrl, double abv, IList<SectionInfo> sections)
        {
            Id = id;
            Name = name;
            Description = description;
            Style = style;
            ThumbnailUrl = thumbnailUrl;
            Abv = abv;
            Sections = sections;
        }

        public BeerInfo(string name, string description, string style, string thumbnailUrl, double abv)
            : this(Guid.NewGuid(), name, description, style, thumbnailUrl, abv, new List<SectionInfo>()) { }

        public bool Equals(BeerInfo other)
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
            return Equals((BeerInfo)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}