using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using UberEatsBackend.Data;

namespace UberEatsBackend.Services
{
  public class OrderService : IOrderService
  {
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly ApplicationDbContext _context; // ✅ CAMBIO: Usar contexto directo para Payment
    private readonly IMapper _mapper;

    // Servicio opcional de ofertas
    private readonly IProductOfferService? _productOfferService;

    // Servicio para validar métodos de pago
    private readonly IPaymentMethodService? _paymentMethodService;

    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        IRepository<Address> addressRepository,
        IRepository<Restaurant> restaurantRepository,
        ApplicationDbContext context, // ✅ CAMBIO: Contexto directo
        IMapper mapper,
        ILogger<OrderService> logger,
        IProductOfferService? productOfferService = null,
        IPaymentMethodService? paymentMethodService = null)
    {
      _orderRepository = orderRepository;
      _productRepository = productRepository;
      _addressRepository = addressRepository;
      _restaurantRepository = restaurantRepository;
      _context = context; // ✅ CAMBIO: Asignar contexto
      _mapper = mapper;
      _logger = logger;
      _productOfferService = productOfferService;
      _paymentMethodService = paymentMethodService;
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
      var orders = await _orderRepository.GetAllAsync();
      return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
      var order = await _orderRepository.GetOrderWithDetailsAsync(id);
      return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
      var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
      return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<List<OrderDto>> GetOrdersByRestaurantIdAsync(int restaurantId)
    {
      var orders = await _orderRepository.GetOrdersByRestaurantIdAsync(restaurantId);
      return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<List<OrderDto>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId)
    {
      var orders = await _orderRepository.GetOrdersByDeliveryPersonIdAsync(deliveryPersonId);
      return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
    {
      // 1. Validar dirección de entrega
      var deliveryAddress = await _addressRepository.GetByIdAsync(createOrderDto.DeliveryAddressId);
      if (deliveryAddress == null)
      {
        throw new KeyNotFoundException($"No se encontró la dirección con ID {createOrderDto.DeliveryAddressId}");
      }

      if (deliveryAddress.UserId != userId)
      {
        throw new UnauthorizedAccessException("La dirección de entrega no pertenece al usuario");
      }

      // 2. ✅ ARREGLO: Validar PaymentMethod para crear Payment
      PaymentMethod? paymentMethod = null;

      if (_paymentMethodService != null)
      {
        var paymentMethodEntity = await _paymentMethodService.GetPaymentMethodEntityAsync(createOrderDto.PaymentMethodId, userId);
        if (paymentMethodEntity == null)
        {
          throw new KeyNotFoundException($"Método de pago con ID {createOrderDto.PaymentMethodId} no encontrado o no pertenece al usuario");
        }
        paymentMethod = paymentMethodEntity;
      }
      else
      {
        paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == createOrderDto.PaymentMethodId &&
                                      pm.UserId == userId &&
                                      pm.IsActive);

        if (paymentMethod == null)
        {
          throw new KeyNotFoundException($"Método de pago con ID {createOrderDto.PaymentMethodId} no encontrado o no pertenece al usuario");
        }
      }

      _logger.LogInformation("✅ Método de pago validado: {PaymentMethodId} - {Type}",
        paymentMethod.Id, paymentMethod.Type);

      // 3. Validar restaurante
      var restaurant = await _restaurantRepository.GetByIdAsync(createOrderDto.RestaurantId);
      if (restaurant == null)
      {
        throw new KeyNotFoundException($"No se encontró el restaurante con ID {createOrderDto.RestaurantId}");
      }

      if (!restaurant.IsOpen)
      {
        throw new InvalidOperationException("El restaurante está cerrado");
      }

      // 4. Obtener productos - MEJORADO: Priorizar RestaurantProduct si existe
      var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();

      var restaurantProducts = await _context.RestaurantProducts
          .Include(rp => rp.Product)
          .Where(rp => rp.RestaurantId == createOrderDto.RestaurantId &&
                      productIds.Contains(rp.ProductId) &&
                      rp.IsAvailable)
          .ToListAsync();

      var foundProductIds = restaurantProducts.Select(rp => rp.ProductId).ToList();
      var missingProductIds = productIds.Where(pid => !foundProductIds.Contains(pid)).ToList();

      var baseProducts = await _context.Products
          .Where(p => missingProductIds.Contains(p.Id))
          .ToListAsync();

      if (restaurantProducts.Count + baseProducts.Count != productIds.Count)
      {
        throw new KeyNotFoundException("Uno o más productos no fueron encontrados");
      }

      // 5. Calcular precios iniciales
      var orderItemsData = new List<(CreateOrderItemDto item, decimal unitPrice, decimal subtotal)>();
      decimal initialSubtotal = 0;

      foreach (var item in createOrderDto.Items)
      {
        decimal unitPrice;

        var restaurantProduct = restaurantProducts.FirstOrDefault(rp => rp.ProductId == item.ProductId);
        if (restaurantProduct != null && restaurantProduct.Price.HasValue)
        {
          unitPrice = restaurantProduct.Price.Value;
        }
        else
        {
          var baseProduct = baseProducts.FirstOrDefault(p => p.Id == item.ProductId);
          if (baseProduct == null)
          {
            throw new KeyNotFoundException($"Producto con ID {item.ProductId} no encontrado");
          }
          unitPrice = baseProduct.BasePrice;
        }

        var itemSubtotal = unitPrice * item.Quantity;
        orderItemsData.Add((item, unitPrice, itemSubtotal));
        initialSubtotal += itemSubtotal;

        _logger.LogInformation("Producto {ProductId}: Precio={Price}, Cantidad={Quantity}, Subtotal={Subtotal}",
          item.ProductId, unitPrice, item.Quantity, itemSubtotal);
      }

      // 6. Aplicar ofertas si disponible
      var appliedOfferIds = new List<int>();
      var finalOrderItemsData = new List<(CreateOrderItemDto item, decimal finalUnitPrice, decimal finalSubtotal)>();

      bool enableOffers = true;

      if (_productOfferService != null && enableOffers)
      {
        try
        {
          _logger.LogInformation("Aplicando ofertas automáticas para restaurante {RestaurantId}", createOrderDto.RestaurantId);

          var productsForOffers = orderItemsData.Select(x =>
              (x.item.ProductId, x.item.Quantity, x.unitPrice)).ToList();

          var offerCalculations = await _productOfferService.CalculateOffersForProducts(
              createOrderDto.RestaurantId, productsForOffers, initialSubtotal);

          _logger.LogInformation("Ofertas encontradas: {OffersCount}", offerCalculations.Count);

          foreach (var itemData in orderItemsData)
          {
            var productOffers = offerCalculations
                .Where(oc => oc.Applied &&
                           productsForOffers.Any(p => p.Item1 == itemData.item.ProductId))
                .ToList();

            var bestOffer = productOffers.OrderByDescending(o => o.CalculatedDiscount).FirstOrDefault();

            var finalUnitPrice = bestOffer?.FinalPrice ?? itemData.unitPrice;

            if (finalUnitPrice < 0.01m)
            {
              _logger.LogWarning("Precio final muy bajo ({FinalPrice}) para producto {ProductId}. Ajustando a $0.01",
                finalUnitPrice, itemData.item.ProductId);
              finalUnitPrice = 0.01m;
            }

            var finalItemSubtotal = finalUnitPrice * itemData.item.Quantity;

            finalOrderItemsData.Add((itemData.item, finalUnitPrice, finalItemSubtotal));

            if (bestOffer != null)
            {
              appliedOfferIds.Add(bestOffer.OfferId);
              var savings = (itemData.unitPrice - finalUnitPrice) * itemData.item.Quantity;
              _logger.LogInformation(
                  "Oferta aplicada - Producto {ProductId}: ${OriginalPrice} → ${FinalPrice} (Ahorro: ${Savings})",
                  itemData.item.ProductId, itemData.unitPrice, finalUnitPrice, savings);
            }
          }

          var totalSavings = initialSubtotal - finalOrderItemsData.Sum(x => x.finalSubtotal);
          if (totalSavings > 0)
          {
            _logger.LogInformation("Ahorros totales por ofertas: ${TotalSavings}", totalSavings);
          }
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Error aplicando ofertas, continuando con precios normales");
          appliedOfferIds.Clear();
          finalOrderItemsData = orderItemsData.Select(x => (x.item, x.unitPrice, x.subtotal)).ToList();
        }
      }
      else
      {
        _logger.LogDebug("Servicio de ofertas no disponible, usando precios normales");
        finalOrderItemsData = orderItemsData.Select(x => (x.item, x.unitPrice, x.subtotal)).ToList();
      }

      decimal finalSubtotal = finalOrderItemsData.Sum(x => x.finalSubtotal);

      // 7. Crear OrderItems
      var orderItems = new List<OrderItem>();

      foreach (var itemData in finalOrderItemsData)
      {
        var orderItem = new OrderItem
        {
          ProductId = itemData.item.ProductId,
          Quantity = itemData.item.Quantity,
          UnitPrice = itemData.finalUnitPrice,
          Subtotal = itemData.finalSubtotal
        };

        orderItems.Add(orderItem);
      }

      // 8. Calcular total final
      decimal deliveryFee = restaurant.DeliveryFee;
      decimal total = finalSubtotal + deliveryFee;

      _logger.LogInformation("Resumen pedido - Subtotal inicial: {InitialSubtotal}, Subtotal final: {FinalSubtotal}, DeliveryFee: {DeliveryFee}, Total: {Total}",
        initialSubtotal, finalSubtotal, deliveryFee, total);

      // 9. ✅ ARREGLO: Crear Payment PRIMERO
      var paymentDescription = GetPaymentMethodDescription(paymentMethod);

      var payment = new Payment
      {
        PaymentMethod = paymentDescription,
        Status = "Completed",
        TransactionId = Guid.NewGuid().ToString(),
        Amount = total,
        PaymentDate = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      // Guardar Payment en la base de datos
      _context.Payments.Add(payment);
      await _context.SaveChangesAsync(); // ✅ IMPORTANTE: Guardar para obtener Payment.Id

      _logger.LogInformation("✅ Payment creado con ID: {PaymentId} - Método: {PaymentMethod} - Monto: ${Amount}",
        payment.Id, paymentDescription, total);

      // 10. ✅ ARREGLO: Crear Order con PaymentId
      var order = new Order
      {
        UserId = userId,
        RestaurantId = createOrderDto.RestaurantId,
        DeliveryAddressId = createOrderDto.DeliveryAddressId,
        PaymentId = payment.Id, // ✅ ASIGNAR PaymentId
        Subtotal = finalSubtotal,
        DeliveryFee = deliveryFee,
        Total = total,
        Status = "Pending",
        EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30 + restaurant.EstimatedDeliveryTime),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      // 11. Guardar Order y OrderItems
      var savedOrder = await _orderRepository.CreateOrderAsync(order, orderItems);

      // 12. Incrementar contadores de ofertas
      if (_productOfferService != null && appliedOfferIds.Any())
      {
        foreach (var offerId in appliedOfferIds.Distinct())
        {
          try
          {
            await _productOfferService.ApplyOfferUsage(offerId);
            _logger.LogInformation("Contador de oferta {OfferId} incrementado", offerId);
          }
          catch (Exception ex)
          {
            _logger.LogWarning(ex, "Error incrementando contador de oferta {OfferId}", offerId);
          }
        }
      }

      // 13. Log final
      var finalTotalSavings = initialSubtotal - finalSubtotal;
      _logger.LogInformation(
          "✅ Pedido {OrderId} creado - PaymentId: {PaymentId} - Subtotal original: ${Original}, Subtotal final: ${Final}, Ahorros: ${Savings}, Total: ${Total}",
          savedOrder.Id, payment.Id, initialSubtotal, finalSubtotal, finalTotalSavings, total);

      // 14. Obtener orden completa con Payment incluido
      var completeOrder = await _orderRepository.GetOrderWithDetailsAsync(savedOrder.Id);
      return _mapper.Map<OrderDto>(completeOrder);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int orderId, OrderStatusDto orderStatusDto, int? deliveryPersonId = null)
    {
      if (!IsValidOrderStatus(orderStatusDto.Status))
      {
        throw new ArgumentException($"Estado de pedido no válido: {orderStatusDto.Status}");
      }

      await _orderRepository.UpdateOrderStatusAsync(orderId, orderStatusDto.Status, deliveryPersonId);

      var updatedOrder = await _orderRepository.GetOrderWithDetailsAsync(orderId);
      return updatedOrder != null ? _mapper.Map<OrderDto>(updatedOrder) : null;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
      var order = await _orderRepository.GetByIdAsync(id);
      if (order == null)
      {
        return false;
      }

      await _orderRepository.DeleteAsync(order.Id);
      return true;
    }

    // ✅ MANTENER: Método helper para descripción del método de pago
    private string GetPaymentMethodDescription(PaymentMethod paymentMethod)
    {
      var type = paymentMethod.Type.ToString().ToLower();

      switch (type)
      {
        case "paypal":
          return $"PayPal ({paymentMethod.PayPalEmail})";
        case "visa":
          return $"Visa •••• {paymentMethod.LastFourDigits}";
        case "mastercard":
          return $"Mastercard •••• {paymentMethod.LastFourDigits}";
        default:
          if (!string.IsNullOrEmpty(paymentMethod.LastFourDigits))
            return $"{paymentMethod.Nickname} •••• {paymentMethod.LastFourDigits}";
          else
            return paymentMethod.Nickname;
      }
    }

    private bool IsValidOrderStatus(string status)
    {
      string[] validStatuses = {
          "Pending",
          "Accepted",
          "Preparing",
          "ReadyForPickup",
          "OnTheWay",
          "Delivered",
          "Cancelled"
      };

      return validStatuses.Contains(status);
    }
  }
}
