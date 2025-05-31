// UberEatsBackend/Services/IPaymentMethodService.cs - INTERFAZ DEFINITIVA
using UberEatsBackend.DTOs.PaymentMethod;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IPaymentMethodService
    {
        // ===== MÉTODOS BÁSICOS CRUD =====
        Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(int userId);
        Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id, int userId);
        Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createDto);
        Task<PaymentMethodDto> UpdatePaymentMethodAsync(int id, int userId, UpdatePaymentMethodDto updateDto);

        // ===== MÉTODOS DE ELIMINACIÓN =====
        Task<bool> DeletePaymentMethodAsync(int id, int userId); // Soft delete
        Task<bool> HardDeletePaymentMethodAsync(int id, int userId); // Hard delete

        // ===== MÉTODOS DE GESTIÓN =====
        Task<bool> SetAsDefaultAsync(int id, int userId);

        // ===== MÉTODOS DE ACCESO A ENTIDADES =====
        Task<PaymentMethod?> GetPaymentMethodEntityAsync(int id, int userId);

        // ===== MÉTODOS DE PROCESAMIENTO DE PAGOS =====
        Task<bool> ValidatePaymentMethodAsync(int paymentMethodId, decimal amount);
        Task<string> ProcessPaymentAsync(int paymentMethodId, decimal amount, string description);
    }
}
