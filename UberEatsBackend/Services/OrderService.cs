using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.DTOs.Offers;
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
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Servicio opcional de ofertas - si no está registrado, no se usa
    private readonly IProductOfferService? _productOfferService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        IRepository<Address> addressRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Payment> paymentRepository,
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<OrderService> logger,
        IProductOfferService? productOfferService = null) // Parámetro opcional
    {
      _orderRepository = orderRepository;
      _productRepository = productRepository;
      _addressRepository = addressRepository;
      _restaurantRepository = restaurantRepository;
      _paymentRepository = paymentRepository;
      _context = context;
      _mapper = mapper;
      _logger = logger;
      _productOfferService = productOfferService; // Puede ser null
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
      // 1. Validar que existe la dirección de entrega (igual que antes)
      var deliveryAddress = await _addressRepository.GetByIdAsync(createOrderDto.DeliveryAddressId);
      if (deliveryAddress == null)
      {
        throw new KeyNotFoundException($"No se encontró la dirección con ID {createOrderDto.DeliveryAddressId}");
      }

      // 2. Validar que existe el restaurante (igual que antes)
      var restaurant = await _restaurantRepository.GetByIdAsync(createOrderDto.RestaurantId);
      if (restaurant == null)
      {
        throw new KeyNotFoundException($"No se encontró el restaurante con ID {createOrderDto.RestaurantId}");
      }

      // 3. Obtener productos - MEJORADO: Priorizar RestaurantProduct si existe
      var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();

      // Intentar obtener productos específicos del restaurante primero
      var restaurantProducts = await _context.RestaurantProducts
          .Include(rp => rp.Product)
          .Where(rp => rp.RestaurantId == createOrderDto.RestaurantId &&
                      productIds.Contains(rp.ProductId) &&
                      rp.IsAvailable)
          .ToListAsync();

      // Para productos que no están en RestaurantProduct, usar el producto base
      var foundProductIds = restaurantProducts.Select(rp => rp.ProductId).ToList();
      var missingProductIds = productIds.Where(pid => !foundProductIds.Contains(pid)).ToList();

      var baseProducts = await _context.Products
          .Where(p => missingProductIds.Contains(p.Id))
          .ToListAsync();

      // Verificar que todos los productos existen
      if (restaurantProducts.Count + baseProducts.Count != productIds.Count)
      {
        throw new KeyNotFoundException("Uno o más productos no fueron encontrados");
      }

      // 4. Calcular precios iniciales y crear estructura de datos
      var orderItemsData = new List<(CreateOrderItemDto item, decimal unitPrice, decimal subtotal)>();
      decimal initialSubtotal = 0;

      foreach (var item in createOrderDto.Items)
      {
        decimal unitPrice;

        // Buscar en RestaurantProduct primero
        var restaurantProduct = restaurantProducts.FirstOrDefault(rp => rp.ProductId == item.ProductId);
        if (restaurantProduct != null)
        {
          unitPrice = restaurantProduct.Price;
        }
        else
        {
          // Usar precio base del producto
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
      }

      // 5. NUEVO: Aplicar ofertas si el servicio está disponible
      decimal finalSubtotal = initialSubtotal;
      var appliedOffers = new List<(int productId, List<ProductOfferSummaryDto> offers)>();

      if (_productOfferService != null)
      {
        try
        {
          _logger.LogInformation("Aplicando ofertas automáticas para restaurante {RestaurantId}", createOrderDto.RestaurantId);

          // Preparar datos para el cálculo de ofertas
          var productsForOffers = orderItemsData.Select(x =>
              (x.item.ProductId, x.item.Quantity, x.unitPrice)).ToList();

          // Calcular ofertas aplicables
          var offerCalculations = await _productOfferService.CalculateOffersForProducts(
              createOrderDto.RestaurantId, productsForOffers, initialSubtotal);

          // Aplicar ofertas y recalcular precios
          finalSubtotal = 0;
          var updatedOrderItemsData = new List<(CreateOrderItemDto item, decimal finalUnitPrice, decimal finalSubtotal)>();

          foreach (var itemData in orderItemsData)
          {
            // Buscar ofertas aplicables para este producto
            var productOffers = offerCalculations
                .Where(oc => oc.Applied &&
                           productsForOffers.Any(p => p.Item1 == itemData.item.ProductId))
                .ToList();

            // Aplicar la mejor oferta (mayor descuento)
            var bestOffer = productOffers.OrderByDescending(o => o.CalculatedDiscount).FirstOrDefault();
            var finalUnitPrice = bestOffer?.FinalPrice ?? itemData.unitPrice;
            var finalItemSubtotal = finalUnitPrice * itemData.item.Quantity;

            updatedOrderItemsData.Add((itemData.item, finalUnitPrice, finalItemSubtotal));
            finalSubtotal += finalItemSubtotal;

            // Registrar ofertas aplicadas para logging
            if (productOffers.Any())
            {
              appliedOffers.Add((itemData.item.ProductId, productOffers));
            }

            // Log del descuento aplicado
            if (bestOffer != null)
            {
              var savings = (itemData.unitPrice - finalUnitPrice) * itemData.item.Quantity;
              _logger.LogInformation(
                  "Oferta aplicada - Producto {ProductId}: ${OriginalPrice} → ${FinalPrice} (Ahorro: ${Savings})",
                  itemData.item.ProductId, itemData.unitPrice, finalUnitPrice, savings);
            }
          }

          // Actualizar datos con precios finales
          orderItemsData = updatedOrderItemsData.Select(x => (x.item, x.finalUnitPrice, x.finalSubtotal)).ToList();

          var offerSavings = initialSubtotal - finalSubtotal;
          if (offerSavings > 0)
          {
            _logger.LogInformation("Ahorros totales por ofertas: ${TotalSavings}", offerSavings);
          }
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Error aplicando ofertas, continuando con precios normales");
          // Si hay error, continuar con precios normales
          finalSubtotal = initialSubtotal;
          appliedOffers.Clear();
        }
      }
      else
      {
        _logger.LogDebug("Servicio de ofertas no disponible, usando precios normales");
      }

      // 6. Crear los ítems del pedido con precios finales
      var orderItems = new List<OrderItem>();

      foreach (var itemData in orderItemsData)
      {
        var orderItem = new OrderItem
        {
          ProductId = itemData.item.ProductId,
          Quantity = itemData.item.Quantity,
          UnitPrice = itemData.unitPrice,
          Subtotal = itemData.subtotal
        };

        orderItems.Add(orderItem);
      }

      // 7. Calcular impuestos y total final (usando subtotal con descuentos)
      decimal deliveryFee = restaurant.DeliveryFee;
      decimal tax = Math.Round(finalSubtotal * 0.21m, 2); // IVA del 21%
      decimal total = finalSubtotal + deliveryFee + tax;

      // 8. Crear el pedido (igual que antes)
      var order = new Order
      {
        UserId = userId,
        RestaurantId = createOrderDto.RestaurantId,
        DeliveryAddressId = createOrderDto.DeliveryAddressId,
        Subtotal = finalSubtotal, // Usar subtotal con descuentos
        DeliveryFee = deliveryFee,
        Tax = tax,
        Total = total,
        Status = "Pending",
        // Estimar tiempo de entrega (30 minutos + tiempo estimado del restaurante)
        EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30 + restaurant.EstimatedDeliveryTime),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      // 9. Guardar el pedido y sus items en la base de datos (igual que antes)
      var savedOrder = await _orderRepository.CreateOrderAsync(order, orderItems);

      // 10. NUEVO: Incrementar contadores de ofertas aplicadas
      if (_productOfferService != null && appliedOffers.Any())
      {
        foreach (var appliedOffer in appliedOffers)
        {
          foreach (var offer in appliedOffer.offers)
          {
            try
            {
              await _productOfferService.ApplyOfferUsage(offer.OfferId);
              _logger.LogInformation("Contador de oferta {OfferId} incrementado", offer.OfferId);
            }
            catch (Exception ex)
            {
              _logger.LogWarning(ex, "Error incrementando contador de oferta {OfferId}", offer.OfferId);
            }
          }
        }
      }

      // 11. Crear el registro de pago asociado al pedido (igual que antes)
      var payment = new Payment
      {
        OrderId = savedOrder.Id,
        PaymentMethod = createOrderDto.PaymentMethod,
        Status = "Completed", // En un caso real, esto dependería del procesamiento del pago
        TransactionId = Guid.NewGuid().ToString(), // En un caso real, esto vendría del procesador de pagos
        Amount = total,
        PaymentDate = DateTime.UtcNow
      };

      await _paymentRepository.CreateAsync(payment);

      // 12. Log de resumen del pedido
      var finalTotalSavings = initialSubtotal - finalSubtotal;
      _logger.LogInformation(
          "Pedido {OrderId} creado - Subtotal original: ${Original}, Subtotal final: ${Final}, Ahorros: ${Savings}, Total: ${Total}",
          savedOrder.Id, initialSubtotal, finalSubtotal, finalTotalSavings, total);

      // 13. Obtener la orden completa con todos sus detalles para el DTO (igual que antes)
      var completeOrder = await _orderRepository.GetOrderWithDetailsAsync(savedOrder.Id);
      return _mapper.Map<OrderDto>(completeOrder);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int orderId, OrderStatusDto orderStatusDto, int? deliveryPersonId = null)
    {
      // Validar el estado del pedido (igual que antes)
      if (!IsValidOrderStatus(orderStatusDto.Status))
      {
        throw new ArgumentException($"Estado de pedido no válido: {orderStatusDto.Status}");
      }

      // Actualizar el estado (igual que antes)
      await _orderRepository.UpdateOrderStatusAsync(orderId, orderStatusDto.Status, deliveryPersonId);

      // Obtener el pedido actualizado (igual que antes)
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
