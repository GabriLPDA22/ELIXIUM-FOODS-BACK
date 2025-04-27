namespace UberEatsBackend.DTOs.Menu
{
    public class CreateMenuDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int RestaurantId { get; set; }
    }
}