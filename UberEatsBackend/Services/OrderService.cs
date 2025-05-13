using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
  public class OrderService : IOrderService
  {
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        IRepository<Address> addressRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Payment> paymentRepository,
        IMapper mapper)
    {
      _orderRepository = orderRepository;
      _productRepository = productRepository;
      _addressRepository = addressRepository;
      _restaurantRepository = restaurantRepository;
      _paymentRepository = paymentRepository;
      _mapper = mapper;
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
      // 1. Validar que existe la dirección de entrega
      var deliveryAddress = await _addressRepository.GetByIdAsync(createOrderDto.DeliveryAddressId);
      if (deliveryAddress == null)
      {
        throw new KeyNotFoundException($"No se encontró la dirección con ID {createOrderDto.DeliveryAddressId}");
      }

      // 2. Validar que existe el restaurante
      var restaurant = await _restaurantRepository.GetByIdAsync(createOrderDto.RestaurantId);
      if (restaurant == null)
      {
        throw new KeyNotFoundException($"No se encontró el restaurante con ID {createOrderDto.RestaurantId}");
      }

      // 3. Obtener todos los productos de una vez para calcular el total
      var productIds = createOrderDto.Items.Select(i => i.ProductId).ToList();
      var products = await _productRepository.Entities
          .Where(p => productIds.Contains(p.Id))
          .ToListAsync();

      if (products.Count != productIds.Count)
      {
        throw new KeyNotFoundException("Uno o más productos no fueron encontrados");
      }

      // 4. Crear los ítems del pedido con sus precios y subtotales
      var orderItems = new List<OrderItem>();
      decimal subtotal = 0;

      foreach (var item in createOrderDto.Items)
      {
        var product = products.First(p => p.Id == item.ProductId);
        var orderItem = new OrderItem
        {
          ProductId = item.ProductId,
          Quantity = item.Quantity,
          UnitPrice = product.Price,
          Subtotal = product.Price * item.Quantity
        };

        orderItems.Add(orderItem);
        subtotal += orderItem.Subtotal;
      }

      // 5. Calcular impuestos y total final
      decimal deliveryFee = restaurant.DeliveryFee;
      decimal tax = Math.Round(subtotal * 0.21m, 2); // IVA del 21%
      decimal total = subtotal + deliveryFee + tax;

      // 6. Crear el pedido
      var order = new Order
      {
        UserId = userId,
        RestaurantId = createOrderDto.RestaurantId,
        DeliveryAddressId = createOrderDto.DeliveryAddressId,
        Subtotal = subtotal,
        DeliveryFee = deliveryFee,
        Tax = tax,
        Total = total,
        Status = "Pending",
        // Estimar tiempo de entrega (30 minutos + tiempo estimado del restaurante)
        EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30 + restaurant.EstimatedDeliveryTime),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      // 7. Guardar el pedido y sus items en la base de datos
      var savedOrder = await _orderRepository.CreateOrderAsync(order, orderItems);

      // 8. Crear el registro de pago asociado al pedido
      var payment = new Payment
      {
        OrderId = savedOrder.Id,
        PaymentMethod = createOrderDto.PaymentMethod,
        Status = "Completed", // En un caso real, esto dependería del procesamiento del pago
        TransactionId = Guid.NewGuid().ToString(), // En un caso real, esto vendría del procesador de pagos
        Amount = total,
        PaymentDate = DateTime.UtcNow
      };

      await _paymentRepository.AddAsync(payment);

      // 9. Obtener la orden completa con todos sus detalles para el DTO
      var completeOrder = await _orderRepository.GetOrderWithDetailsAsync(savedOrder.Id);
      return _mapper.Map<OrderDto>(completeOrder);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int orderId, OrderStatusDto orderStatusDto, int? deliveryPersonId = null)
    {
      // Validar el estado del pedido
      if (!IsValidOrderStatus(orderStatusDto.Status))
      {
        throw new ArgumentException($"Estado de pedido no válido: {orderStatusDto.Status}");
      }

      // Actualizar el estado
      await _orderRepository.UpdateOrderStatusAsync(orderId, orderStatusDto.Status, deliveryPersonId);

      // Obtener el pedido actualizado
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

      await _orderRepository.DeleteAsync(order);
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