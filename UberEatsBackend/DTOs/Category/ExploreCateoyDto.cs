namespace UberEatsBackend.DTOs.Category
{
  public class ExploreCategoryDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RestaurantCount { get; set; }
  }
}
