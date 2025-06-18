namespace FinalProject_ITI.Models
{
    public class OrderType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
