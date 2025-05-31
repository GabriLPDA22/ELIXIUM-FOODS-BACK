// UberEatsBackend/Services/PaymentMethodService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UberEatsBackend.Data; // ← IMPORTANTE: Agregar este using
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

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createDto)
    {
      try
      {
        // Validar tipo de pago
        if (!Enum.TryParse<PaymentType>(createDto.Type, true, out var paymentType))
        {
          throw new ArgumentException("Tipo de pago no válido");
        }

        var paymentMethod = new PaymentMethod
        {
          UserId = userId,
          Nickname = createDto.Nickname,
          Type = paymentType,
          IsDefault = createDto.IsDefault,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        };

        // Procesar según el tipo
        if (paymentType == PaymentType.PayPal)
        {
          if (string.IsNullOrEmpty(createDto.PayPalEmail))
            throw new ArgumentException("Email de PayPal requerido");

          paymentMethod.PayPalEmail = createDto.PayPalEmail;
        }
        else
        {
          // Procesar tarjeta
          if (string.IsNullOrEmpty(createDto.CardNumber) ||
              string.IsNullOrEmpty(createDto.ExpiryDate) ||
              string.IsNullOrEmpty(createDto.CVV))
            throw new ArgumentException("Datos de tarjeta incompletos");

          // Validar número de tarjeta
          var cleanCardNumber = createDto.CardNumber.Replace(" ", "");
          if (!IsValidCardNumber(cleanCardNumber))
            throw new ArgumentException("Número de tarjeta no válido");

          // SEGURIDAD: Solo almacenar últimos 4 dígitos
          paymentMethod.LastFourDigits = cleanCardNumber.Substring(cleanCardNumber.Length - 4);
          paymentMethod.CardholderName = createDto.CardholderName;

          // Procesar fecha de expiración
          var expiryParts = createDto.ExpiryDate.Split('/');
          if (expiryParts.Length == 2 &&
              int.TryParse(expiryParts[0], out var month) &&
              int.TryParse(expiryParts[1], out var year))
          {
            paymentMethod.ExpiryMonth = month;
            paymentMethod.ExpiryYear = year < 100 ? 2000 + year : year; // Manejar YY y YYYY
          }
        }

        // Si es el primer método o se marca como predeterminado
        if (createDto.IsDefault)
        {
          await UnsetDefaultPaymentMethodsAsync(userId);
        }
        else
        {
          var hasDefault = await _context.PaymentMethods
              .AnyAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive);
          if (!hasDefault)
          {
            paymentMethod.IsDefault = true;
          }
        }

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Método de pago creado para usuario {UserId}", userId);

        return _mapper.Map<PaymentMethodDto>(paymentMethod);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creando método de pago para usuario {UserId}", userId);
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

        paymentMethod.Nickname = updateDto.Nickname;
        paymentMethod.CardholderName = updateDto.CardholderName;
        paymentMethod.PayPalEmail = updateDto.PayPalEmail;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

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

        // Soft delete
        paymentMethod.IsActive = false;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Si era el predeterminado, establecer otro como predeterminado
        if (wasDefault)
        {
          var firstActive = await _context.PaymentMethods
              .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsActive);

          if (firstActive != null)
          {
            firstActive.IsDefault = true;
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

        // Simular procesamiento
        await Task.Delay(1000);

        // Retornar ID de transacción simulado
        return $"txn_{DateTime.UtcNow.Ticks}";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error procesando pago con método {PaymentMethodId}", paymentMethodId);
        throw;
      }
    }

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

      await _context.SaveChangesAsync();
    }

    // Validación básica de tarjeta usando algoritmo de Luhn
    private bool IsValidCardNumber(string cardNumber)
    {
      if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 13 || cardNumber.Length > 19)
        return false;

      int sum = 0;
      bool alternate = false;

      for (int i = cardNumber.Length - 1; i >= 0; i--)
      {
        if (!char.IsDigit(cardNumber[i]))
          return false;

        int digit = cardNumber[i] - '0';

        if (alternate)
        {
          digit *= 2;
          if (digit > 9)
            digit = (digit % 10) + 1;
        }

        sum += digit;
        alternate = !alternate;
      }

      return sum % 10 == 0;
    }
  }
}
