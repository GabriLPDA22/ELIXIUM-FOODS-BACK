using AutoMapper;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.DTOs.Category;
using UberEatsBackend.DTOs.Menu;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.DTOs.Restaurant;
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
      // User Mappings
      CreateMap<User, UserDto>()
          .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));

      CreateMap<RegisterRequestDto, User>();
      CreateMap<CreateUserDto, User>();
      CreateMap<UpdateUserDto, User>();
      CreateMap<UpdateProfileDto, User>();

      // Address Mappings
      CreateMap<Address, AddressDto>();
      CreateMap<Address, ExtendedAddressDto>();
      CreateMap<CreateAddressDto, Address>();
      CreateMap<CreateExtendedAddressDto, Address>();
      CreateMap<UpdateAddressDto, Address>();

      // Business Mappings
      CreateMap<Business, BusinessDto>()
          .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
          .ForMember(dest => dest.Restaurants, opt => opt.MapFrom(src => src.Restaurants));
      CreateMap<CreateBusinessDto, Business>();
      CreateMap<UpdateBusinessDto, Business>();

      // Restaurant Mappings - Sin UserId/Owner
      CreateMap<Restaurant, RestaurantDto>()
          .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));

      CreateMap<Restaurant, RestaurantCardDto>()
          .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count))
          .ForMember(dest => dest.Cuisine, opt => opt.Ignore()) // Puede ser populado manualmente
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));

      CreateMap<CreateRestaurantDto, Restaurant>();
      CreateMap<UpdateRestaurantDto, Restaurant>();

      // Menu Mappings
      CreateMap<Menu, MenuDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
          .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
      CreateMap<CreateMenuDto, Menu>();
      CreateMap<UpdateMenuDto, Menu>();

      // Category Mappings
      CreateMap<Category, CategoryDto>()
          .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
      CreateMap<CreateCategoryDto, Category>();
      CreateMap<UpdateCategoryDto, Category>();

      // Product Mappings
      CreateMap<Product, ProductDto>()
          .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
          .ForMember(dest => dest.RestaurantId, opt => opt.MapFrom(src => src.Category.Menu.RestaurantId))
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Category.Menu.Restaurant.Name))
          .ForMember(dest => dest.RestaurantLogo, opt => opt.MapFrom(src => src.Category.Menu.Restaurant.LogoUrl));
      CreateMap<CreateProductDto, Product>();
      CreateMap<UpdateProductDto, Product>();

      // Order Mappings
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
