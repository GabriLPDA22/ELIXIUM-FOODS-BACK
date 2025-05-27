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
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
      CreateMap<RegisterRequestDto, User>();
      CreateMap<CreateUserDto, User>()
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
      CreateMap<UpdateUserDto, User>()
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
      CreateMap<UpdateProfileDto, User>();

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

      // Promotion mappings
      CreateMap<Promotion, PromotionDto>()
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name));
      CreateMap<CreatePromotionDto, Promotion>();
      CreateMap<UpdatePromotionDto, Promotion>();
    }
  }
}
