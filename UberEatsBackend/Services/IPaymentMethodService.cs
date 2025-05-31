// UberEatsBackend/Services/IPaymentMethodService.cs
using UberEatsBackend.DTOs.PaymentMethod;

namespace UberEatsBackend.Services
{
    public interface IPaymentMethodService
    {
        Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(int userId);
        Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id, int userId);
        Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createDto);
        Task<PaymentMethodDto> UpdatePaymentMethodAsync(int id, int userId, UpdatePaymentMethodDto updateDto);
        Task<bool> DeletePaymentMethodAsync(int id, int userId);
        Task<bool> SetAsDefaultAsync(int id, int userId);

        // Para procesamiento de pagos
    Task<bool> ValidatePaymentMethodAsync(int paymentMethodId, decimal amount);
        Task<string> ProcessPaymentAsync(int paymentMethodId, decimal amount, string description);
    }
}
