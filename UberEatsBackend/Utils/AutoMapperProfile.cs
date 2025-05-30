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
using UberEatsBackend.DTOs.Offers; // ← NUEVA LÍNEA
using UberEatsBackend.Models;

namespace UberEatsBackend.Utils
{
  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      // User mappings
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

      // Address mappings
      CreateMap<Address, AddressDto>();
      CreateMap<Address, ExtendedAddressDto>();
      CreateMap<CreateAddressDto, Address>();
      CreateMap<CreateExtendedAddressDto, Address>();
      CreateMap<UpdateAddressDto, Address>();

      // Business mappings
      CreateMap<Business, BusinessDto>()
          .ForMember(dest => dest.Restaurants, opt => opt.MapFrom(src => src.Restaurants))
          .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
          .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));
      CreateMap<CreateBusinessDto, Business>();
      CreateMap<UpdateBusinessDto, Business>();

      // Restaurant mappings
      CreateMap<Restaurant, RestaurantDto>()
          .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));
      CreateMap<Restaurant, RestaurantCardDto>()
          .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count))
          .ForMember(dest => dest.Cuisine, opt => opt.Ignore())
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));
      CreateMap<CreateRestaurantDto, Restaurant>();
      CreateMap<UpdateRestaurantDto, Restaurant>();

      // Category mappings
      CreateMap<Category, CategoryDto>()
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name))
          .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
      CreateMap<CreateCategoryDto, Category>();
      CreateMap<UpdateCategoryDto, Category>();

      // Product mappings
      CreateMap<Product, ProductDto>()
          .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
          .ForMember(dest => dest.BusinessId, opt => opt.MapFrom(src => src.Category.BusinessId))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Category.Business.Name));
      CreateMap<CreateProductDto, Product>();
      CreateMap<UpdateProductDto, Product>();

      // RestaurantProduct mappings
      CreateMap<RestaurantProduct, RestaurantProductDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
          .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
          .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.Product.BasePrice));
      CreateMap<CreateRestaurantProductDto, RestaurantProduct>();
      CreateMap<UpdateRestaurantProductDto, RestaurantProduct>();

      // Order mappings
      CreateMap<Order, OrderDto>()
          .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
          .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => $"{src.DeliveryAddress.Street}, {src.DeliveryAddress.City}"))
          .ForMember(dest => dest.DeliveryPersonName, opt => opt.MapFrom(src => src.DeliveryPerson != null ? src.DeliveryPerson.FullName : null))
          .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
          .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment));
      CreateMap<OrderItem, OrderItemDto>()
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
          .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));
      CreateMap<Payment, PaymentDto>();

      // ===== NUEVOS MAPPINGS PARA PRODUCT OFFERS =====

      // ProductOffer mappings
      CreateMap<ProductOffer, ProductOfferDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
          .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
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
          .ForMember(dest => dest.RestaurantId, opt => opt.Ignore()); // Se asigna en el servicio

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

      // OrderItemOffer mappings
      CreateMap<OrderItemOffer, AppliedOfferDto>()
          .ForMember(dest => dest.OfferId, opt => opt.MapFrom(src => src.OfferId))
          .ForMember(dest => dest.OfferName, opt => opt.MapFrom(src => src.OfferName));
    }
  }
}
