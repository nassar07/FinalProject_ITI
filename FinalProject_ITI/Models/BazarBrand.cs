namespace FinalProject_ITI.Models;

public class BazarBrand
{
    public int Id { get; set; }
    //public DateTime OrderDate { get; set; }
    //public string Status { get; set; }
    public int BazarID { get; set; }
    public int BrandID { get; set; }
    public Bazar Bazar { get; set; }
    public Brand Brand { get; set; }
}
