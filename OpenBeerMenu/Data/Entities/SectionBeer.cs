namespace OpenBeerMenu.Data.Entities
{
    public class SectionBeer
    {
        public int Position { get; set; }
        
        public Guid SectionId { get; set; }
        public SectionInfo Section { get; set; }
        
        public Guid BeerId { get; set; }
        public BeerInfo Beer { get; set; }
    }
}