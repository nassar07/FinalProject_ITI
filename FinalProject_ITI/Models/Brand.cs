namespace FinalProject_ITI.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }

        public int CategoryID { get; set; }
        public string OwnerID { get; set; }
        public int SubscribeID { get; set; }

        public Category Category { get; set; }
        public ApplicationUser Owner { get; set; }
        public Subscribe Subscribe { get; set; }

        public ICollection<BazarBrand> BazarBrands { get; set; }
    }
}
