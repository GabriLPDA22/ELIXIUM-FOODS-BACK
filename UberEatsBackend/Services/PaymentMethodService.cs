// UberEatsBackend/Services/PaymentMethodService.cs - SUPER SIMPLE Y PERMISIVO
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.PaymentMethod;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<PaymentMethodService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(int userId)
        {
            try
            {
                var paymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.UserId == userId && pm.IsActive)
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenBy(pm => pm.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<PaymentMethodDto>>(paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métodos de pago para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId && pm.IsActive);

                return paymentMethod != null ? _mapper.Map<PaymentMethodDto>(paymentMethod) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo método de pago {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<PaymentMethod?> GetPaymentMethodEntityAsync(int id, int userId)
        {
            try
            {
                return await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entidad método de pago {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createDto)
        {
            try
            {
                _logger.LogInformation("=== INICIO SERVICE CREATE ===");
                _logger.LogInformation("UserId: {UserId}", userId);
                _logger.LogInformation("CreateDto: {@CreateDto}", createDto);

                // Validaciones MÍNIMAS
                if (string.IsNullOrWhiteSpace(createDto.Nickname))
                {
                    _logger.LogWarning("Nickname vacío");
                    throw new ArgumentException("Nombre del método es requerido");
                }

                if (string.IsNullOrWhiteSpace(createDto.Type))
                {
                    _logger.LogWarning("Type vacío");
                    throw new ArgumentException("Tipo de método es requerido");
                }

                _logger.LogInformation("Validaciones básicas pasadas");

                // Mapear tipo SIN validación estricta
                var paymentType = MapStringToPaymentType(createDto.Type);
                _logger.LogInformation("PaymentType mapeado: {PaymentType}", paymentType);

                var paymentMethod = new PaymentMethod
                {
                    UserId = userId,
                    Nickname = createDto.Nickname.Trim(),
                    Type = paymentType,
                    IsDefault = createDto.IsDefault,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("PaymentMethod object creado");

                // Procesar según el tipo SIN validaciones estrictas
                if (paymentType == PaymentType.PayPal)
                {
                    _logger.LogInformation("Procesando PayPal");

                    if (!string.IsNullOrWhiteSpace(createDto.PayPalEmail))
                    {
                        paymentMethod.PayPalEmail = createDto.PayPalEmail.Trim();
                        _logger.LogInformation("PayPal email asignado: {Email}", paymentMethod.PayPalEmail);
                    }
                    else
                    {
                        _logger.LogWarning("PayPal email vacío");
                        throw new ArgumentException("Email de PayPal es requerido");
                    }
                }
                else
                {
                    _logger.LogInformation("Procesando tarjeta");

                    // VALIDACIONES MUY PERMISIVAS para tarjetas
                    if (!string.IsNullOrWhiteSpace(createDto.CardNumber))
                    {
                        var cleanCardNumber = createDto.CardNumber.Replace(" ", "").Replace("-", "");
                        _logger.LogInformation("Card number limpio: {Length} caracteres", cleanCardNumber.Length);

                        if (cleanCardNumber.Length >= 13)
                        {
                            paymentMethod.LastFourDigits = cleanCardNumber.Substring(cleanCardNumber.Length - 4);
                            _logger.LogInformation("LastFourDigits: {LastFour}", paymentMethod.LastFourDigits);
                        }
                        else
                        {
                            throw new ArgumentException("Número de tarjeta muy corto");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Número de tarjeta es requerido");
                    }

                    if (!string.IsNullOrWhiteSpace(createDto.CardholderName))
                    {
                        paymentMethod.CardholderName = createDto.CardholderName.Trim();
                        _logger.LogInformation("CardholderName: {Name}", paymentMethod.CardholderName);
                    }
                    else
                    {
                        throw new ArgumentException("Nombre del titular es requerido");
                    }

                    // Procesar fecha de expiración PERMISIVAMENTE
                    if (!string.IsNullOrWhiteSpace(createDto.ExpiryDate))
                    {
                        try
                        {
                            var parts = createDto.ExpiryDate.Trim().Split('/');
                            if (parts.Length == 2 &&
                                int.TryParse(parts[0], out var month) &&
                                int.TryParse(parts[1], out var year))
                            {
                                if (month >= 1 && month <= 12)
                                {
                                    if (year < 100) year += 2000; // Convertir YY a YYYY

                                    paymentMethod.ExpiryMonth = month;
                                    paymentMethod.ExpiryYear = year;

                                    _logger.LogInformation("Fecha procesada: {Month}/{Year}", month, year);
                                }
                                else
                                {
                                    throw new ArgumentException("Mes inválido en fecha de vencimiento");
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Formato de fecha inválido. Use MM/AA");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error procesando fecha: {Error}", ex.Message);
                            throw new ArgumentException($"Error en fecha de vencimiento: {ex.Message}");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Fecha de vencimiento es requerida");
                    }
                }

                _logger.LogInformation("Datos procesados correctamente");

                // Manejar método predeterminado
                if (createDto.IsDefault)
                {
                    await UnsetDefaultPaymentMethodsAsync(userId);
                    _logger.LogInformation("Métodos predeterminados anteriores desactivados");
                }
                else
                {
                    var hasDefault = await _context.PaymentMethods
                        .AnyAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive);

                    if (!hasDefault)
                    {
                        paymentMethod.IsDefault = true;
                        _logger.LogInformation("Establecido como predeterminado (primer método)");
                    }
                }

                _logger.LogInformation("Guardando en base de datos...");

                // Guardar en base de datos
                _context.PaymentMethods.Add(paymentMethod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Método de pago guardado con ID {Id}", paymentMethod.Id);
                _logger.LogInformation("=== FIN SERVICE CREATE ===");

                return _mapper.Map<PaymentMethodDto>(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en CreatePaymentMethodAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<PaymentMethodDto> UpdatePaymentMethodAsync(int id, int userId, UpdatePaymentMethodDto updateDto)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId && pm.IsActive);

                if (paymentMethod == null)
                    throw new ArgumentException("Método de pago no encontrado");

                paymentMethod.Nickname = updateDto.Nickname.Trim();
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(updateDto.CardholderName))
                    paymentMethod.CardholderName = updateDto.CardholderName.Trim();

                if (!string.IsNullOrWhiteSpace(updateDto.PayPalEmail))
                    paymentMethod.PayPalEmail = updateDto.PayPalEmail.Trim();

                if (updateDto.IsDefault && !paymentMethod.IsDefault)
                {
                    await UnsetDefaultPaymentMethodsAsync(userId);
                    paymentMethod.IsDefault = true;
                }

                await _context.SaveChangesAsync();
                return _mapper.Map<PaymentMethodDto>(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando método de pago {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeletePaymentMethodAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId && pm.IsActive);

                if (paymentMethod == null) return false;

                var wasDefault = paymentMethod.IsDefault;

                paymentMethod.IsActive = false;
                paymentMethod.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (wasDefault)
                {
                    var firstActive = await _context.PaymentMethods
                        .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsActive);

                    if (firstActive != null)
                    {
                        firstActive.IsDefault = true;
                        firstActive.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando método de pago {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> HardDeletePaymentMethodAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId);

                if (paymentMethod == null) return false;

                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando permanentemente método de pago {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> SetAsDefaultAsync(int id, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId && pm.IsActive);

                if (paymentMethod == null) return false;

                await UnsetDefaultPaymentMethodsAsync(userId);

                paymentMethod.IsDefault = true;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo método predeterminado {Id} para usuario {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> ValidatePaymentMethodAsync(int paymentMethodId, decimal amount)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.IsActive);
                return paymentMethod != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ProcessPaymentAsync(int paymentMethodId, decimal amount, string description)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.IsActive);

                if (paymentMethod == null)
                    throw new ArgumentException("Método de pago no válido");

                await Task.Delay(1000);
                return $"txn_{DateTime.UtcNow.Ticks}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando pago con método {PaymentMethodId}", paymentMethodId);
                throw;
            }
        }

        // ===== MÉTODOS PRIVADOS =====

        private async Task UnsetDefaultPaymentMethodsAsync(int userId)
        {
            var defaultMethods = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive)
                .ToListAsync();

            foreach (var method in defaultMethods)
            {
                method.IsDefault = false;
                method.UpdatedAt = DateTime.UtcNow;
            }

            if (defaultMethods.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private PaymentType MapStringToPaymentType(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
            {
                _logger.LogWarning("TypeString está vacío, usando 'other'");
                return PaymentType.Other;
            }

            var result = typeString.Trim().ToLower() switch
            {
                "visa" => PaymentType.Visa,
                "mastercard" => PaymentType.Mastercard,
                "paypal" => PaymentType.PayPal,
                "other" => PaymentType.Other,
                _ => PaymentType.Other // MUY PERMISIVO
            };

            _logger.LogInformation("Mapeado '{TypeString}' a {Result}", typeString, result);
            return result;
        }
    }
}
