namespace FinalProject_ITI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionReference { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; }
    }
}
