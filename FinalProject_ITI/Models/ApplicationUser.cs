using Microsoft.AspNetCore.Identity;

namespace FinalProject_ITI.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AccountType { get; set; }

    public ICollection<Review>? Reviews { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<Order>? AssignedOrders { get; set; }
}
