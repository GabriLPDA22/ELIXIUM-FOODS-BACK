namespace UberEatsBackend.DTOs.Category
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MenuId { get; set; }
    }
}