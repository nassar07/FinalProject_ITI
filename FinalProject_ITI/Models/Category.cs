namespace FinalProject_ITI.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Brand> Brands { get; set; }
    }
}
