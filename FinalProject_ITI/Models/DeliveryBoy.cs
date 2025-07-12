namespace FinalProject_ITI.Models
{
    public class DeliveryBoy
    {
        public int Id { get; set; }

        //public string UserId { get; set; } // Identity FK
        //public ApplicationUser User { get; set; }

        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
