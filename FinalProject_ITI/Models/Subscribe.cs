namespace FinalProject_ITI.Models
{
    public class Subscribe
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public ICollection<Brand> Brands { get; set; }
    }
}
