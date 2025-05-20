// UberEatsBackend/DTOs/Business/BusinessHourDto.cs
namespace UberEatsBackend.DTOs.Business
{
  public class BusinessHourDto
  {
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public int BusinessId { get; set; }
  }

  public class CreateBusinessHourDto
  {
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
  }

  public class UpdateBusinessHourDto
  {
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
  }
}
