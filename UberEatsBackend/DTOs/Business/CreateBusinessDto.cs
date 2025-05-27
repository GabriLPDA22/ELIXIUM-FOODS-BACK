namespace UberEatsBackend.DTOs.Business
{
  public class CreateBusinessDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string BusinessType { get; set; } = "Restaurant";
    public int? UserId { get; set; }
  }
}
