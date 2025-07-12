namespace FinalProject_ITI.Models
{
    public class Bazar
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Location { get; set; }
        public string Entry { get; set; }
        public ICollection<BazarBrand> BazarBrands { get; set; }
    }
}


//EDITABLE