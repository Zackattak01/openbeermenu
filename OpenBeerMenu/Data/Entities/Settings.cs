using System.ComponentModel.DataAnnotations;

namespace OpenBeerMenu.Data.Entities
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string LogoUrl { get; set; }
        
        public string AccessCode { get; set; }
        
        public bool ShowCompanyHeader { get; set; }
        
        public MenuInfo DefaultMenu { get; set; }
        
    }
}