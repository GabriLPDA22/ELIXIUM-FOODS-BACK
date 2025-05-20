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
      CreateMap<User, UserDto>()
          .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));
      CreateMap<RegisterRequestDto, User>();
      CreateMap<CreateUserDto, User>();
      CreateMap<UpdateUserDto, User>();
      CreateMap<UpdateProfileDto, User>();

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
          .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count))
          .ForMember(dest => dest.Cuisine, opt => opt.Ignore())
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business != null ? src.Business.Name : string.Empty));
      CreateMap<CreateRestaurantDto, Restaurant>();
      CreateMap<UpdateRestaurantDto, Restaurant>();

      CreateMap<Menu, MenuDto>()
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant.Name))
          .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
      CreateMap<CreateMenuDto, Menu>();
      CreateMap<UpdateMenuDto, Menu>();

      CreateMap<Category, CategoryDto>()
          .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
      CreateMap<CreateCategoryDto, Category>();
      CreateMap<UpdateCategoryDto, Category>();

      CreateMap<Product, ProductDto>()
          .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
          .ForMember(dest => dest.RestaurantId, opt => opt.MapFrom(src => src.Category.Menu.RestaurantId))
          .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Category.Menu.Restaurant.Name))
          .ForMember(dest => dest.RestaurantLogo, opt => opt.MapFrom(src => src.Category.Menu.Restaurant.LogoUrl));
      CreateMap<CreateProductDto, Product>();
      CreateMap<UpdateProductDto, Product>();

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

      CreateMap<Promotion, PromotionDto>()
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name));
      CreateMap<CreatePromotionDto, Promotion>();
      CreateMap<UpdatePromotionDto, Promotion>();
    }
  }
}
