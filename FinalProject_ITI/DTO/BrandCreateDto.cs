using System.ComponentModel.DataAnnotations;

namespace FinalProject_ITI.DTO
{
    public class BrandCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }

        public int CategoryID { get; set; }
        public string OwnerID { get; set; }
    }
}
