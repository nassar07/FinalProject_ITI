namespace FinalProject_ITI.DTO
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }

        public string Category { get; set; }           // جديد
        public int ProductCount { get; set; }          // جديد
        public double AverageRating { get; set; }      // جديد
    }


}
