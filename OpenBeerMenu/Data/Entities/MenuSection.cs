namespace OpenBeerMenu.Data.Entities
{
    public class MenuSection
    {
        public int Position { get; set; }
        
        public Guid MenuId { get; set; }
        public MenuInfo Menu { get; set; }
        
        public Guid SectionId { get; set; }
        public SectionInfo Section { get; set; }
    }
}