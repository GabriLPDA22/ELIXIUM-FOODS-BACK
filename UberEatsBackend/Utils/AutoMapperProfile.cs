using AutoMapper;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.DTOs.Category;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.DTOs.RestaurantProduct;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.DTOs.Address;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.DTOs.Offers;
using UberEatsBackend.DTOs.PaymentMethod;
using UberEatsBackend.Models;
using System;

namespace UberEatsBackend.Utils
{
  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<User, UserDto>()
          .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses))
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
          .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src =>
              src.Birthdate.HasValue ? src.Birthdate.Value.ToString("yyyy-MM-dd") : null))
          .ReverseMap()
          .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src =>
              !string.IsNullOrEmpty(src.Birthdate) ? DateTime.Parse(src.Birthdate) : (DateTime?)null));

      CreateMap<RegisterRequestDto, User>()
          .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

      CreateMap<CreateUserDto, User>()
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
          .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

      CreateMap<UpdateUserDto, User>()
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.Email, opt => opt.Ignore())
          .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

      CreateMap<UpdateProfileDto, User>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.Email, opt => opt.Ignore())
          .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
          .ForMember(dest => dest.Role, opt => opt.Ignore())
          .ForMember(dest => dest.IsActive, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.DietaryPreferencesJson, opt => opt.Ignore())
          .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src =>
              !string.IsNullOrEmpty(src.Birthdate) ? DateTime.Parse(src.Birthdate) : (DateTime?)null));

      CreateMap<Address, AddressDto>();
      CreateMap<Address, ExtendedAddressDto>();
      CreateMap<CreateAddressDto, Address>();
      CreateMap<CreateExtendedAddressDto, Address>();
      CreateMap<UpdateAddressDto, Address>();

      CreateMap<Business, BusinessDto>()
          .ForMember(dest => dest.Restaurants, opt => opt.MapFrom(src => src.Restaurants))
          .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
          .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));
      CreateMap<CreateBusinessDto, Business>();
      CreateMap<UpdateBusinessDto, Business>();

      CreateMap<Restaurant, RestaurantDto>()
          .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));

      CreateMap<Restaurant, RestaurantCardDto>()
          .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders != null ? src.Orders.Count : 0))
          .ForMember(dest => dest.Cuisine, opt => opt.Ignore())
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));

      CreateMap<CreateRestaurantDto, Restaurant>();
      CreateMap<UpdateRestaurantDto, Restaurant>();

      CreateMap<RestaurantHour, RestaurantHourDto>()
          .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => src.OpenTime.ToString(@"hh\:mm")))
          .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => src.CloseTime.ToString(@"hh\:mm")));

      CreateMap<CreateRestaurantHourDto, RestaurantHour>()
          .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.OpenTime)))
          .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => TimeSpan.Parse(src.CloseTime)))
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore());

      CreateMap<UpdateRestaurantHourDto, RestaurantHour>()
          .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => src.OpenTime))
          .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => src.CloseTime))
         .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore())
          .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

      CreateMap<Category, CategoryDto>()
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : null))
          .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
      CreateMap<CreateCategoryDto, Category>();
      CreateMap<UpdateCategoryDto, Category>();

      CreateMap<Product, ProductDto>()
          .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
          .ForMember(dest => dest.BusinessId, opt => opt.MapFrom(src => src.Category != null ? src.Category.BusinessId : 0))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Category != null && src.Category.Business != null ? src.Category.Business.Name : null))
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantName, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantPrice, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantProductIsAvailable, opt => opt.Ignore())
          .ForMember(dest => dest.StockQuantity, opt => opt.Ignore());

      CreateMap<CreateProductDto, Product>();
      CreateMap<UpdateProductDto, Product>();

      CreateMap<RestaurantProduct, RestaurantProductDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : null))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
          .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product != null ? src.Product.Description : null))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null))
          .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.BasePrice : 0));

      CreateMap<CreateRestaurantProductDto, RestaurantProduct>();
      CreateMap<UpdateRestaurantProductDto, RestaurantProduct>();

      CreateMap<RestaurantProduct, RestaurantProductOfferingDto>()
          .ForMember(dest => dest.RestaurantId, opt => opt.MapFrom(src => src.RestaurantId))
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : null))
          .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
          .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
          .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
          .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

      CreateMap<Order, OrderDto>()
          .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : null))
          .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => src.DeliveryAddress != null ? $"{src.DeliveryAddress.Street}, {src.DeliveryAddress.City}" : null))
          .ForMember(dest => dest.DeliveryPersonName, opt => opt.MapFrom(src => src.DeliveryPerson != null ? src.DeliveryPerson.FullName : null))
          .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
          .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment));

      CreateMap<OrderItem, OrderItemDto>()
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
          .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product != null ? src.Product.Description : null))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null));
      CreateMap<Payment, PaymentDto>();

      CreateMap<ProductOffer, ProductOfferDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : null))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null))
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive()))
          .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.EndDate < DateTime.UtcNow))
          .ForMember(dest => dest.RemainingUses, opt => opt.MapFrom(src =>
              src.UsageLimit > 0 ? Math.Max(0, src.UsageLimit - src.UsageCount) : -1));

      CreateMap<CreateProductOfferDto, ProductOffer>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => 0))
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "active"))
          .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
          .ForMember(dest => dest.Product, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore());

      CreateMap<UpdateProductOfferDto, ProductOffer>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
          .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
          .ForMember(dest => dest.Product, opt => opt.Ignore())
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore())
          .ForMember(dest => dest.ProductId, opt => opt.Ignore())
          .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

      CreateMap<PaymentMethod, PaymentMethodDto>()
          .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString().ToLower()));

      CreateMap<CreatePaymentMethodDto, PaymentMethod>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.UserId, opt => opt.Ignore())
          .ForMember(dest => dest.LastFourDigits, opt => opt.Ignore())
          .ForMember(dest => dest.ExpiryMonth, opt => opt.Ignore())
          .ForMember(dest => dest.ExpiryYear, opt => opt.Ignore())
          .ForMember(dest => dest.PaymentToken, opt => opt.Ignore())
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.User, opt => opt.Ignore());

      CreateMap<UpdatePaymentMethodDto, PaymentMethod>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.UserId, opt => opt.Ignore())
          .ForMember(dest => dest.Type, opt => opt.Ignore())
          .ForMember(dest => dest.LastFourDigits, opt => opt.Ignore())
          .ForMember(dest => dest.ExpiryMonth, opt => opt.Ignore())
          .ForMember(dest => dest.ExpiryYear, opt => opt.Ignore())
          .ForMember(dest => dest.PaymentToken, opt => opt.Ignore())
          .ForMember(dest => dest.IsActive, opt => opt.Ignore())
          .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
          .ForMember(dest => dest.User, opt => opt.Ignore())
          .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

      CreateMap<OrderItemOffer, AppliedOfferDto>()
          .ForMember(dest => dest.OfferId, opt => opt.MapFrom(src => src.OfferId))
          .ForMember(dest => dest.OfferName, opt => opt.MapFrom(src => src.OfferName));
    }
  }
}
