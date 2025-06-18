namespace FinalProject_ITI.Models
{
    public class Bazar
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<BazarBrand> BazarBrands { get; set; }
    }
}
