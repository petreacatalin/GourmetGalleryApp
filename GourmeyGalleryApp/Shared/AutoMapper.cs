// AutoMapperProfile.cs
using AutoMapper;
using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();
        CreateMap<Recipe, RecipeDto>().ReverseMap();       
        CreateMap<Comment, CommentDto>().ReverseMap();
        CreateMap<Rating, RatingDto>().ReverseMap();
        CreateMap<IngredientsTotal, IngredientsTotalDto>().ReverseMap();
        CreateMap<Instructions, InstructionsDto>().ReverseMap();
        CreateMap<Step, StepDto>().ReverseMap();
        CreateMap<Ingredient, IngredientDto>().ReverseMap();
        CreateMap<InformationTime, InformationTimeDto>().ReverseMap();
        CreateMap<NutritionFacts, NutritionFactsDto>().ReverseMap();
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<UserBadge, UserBadgeDto>().ReverseMap();
        CreateMap<Badge, BadgeDto>().ReverseMap();
        CreateMap<ApplicationUser, ApplicationUserDto>()
    .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.UserBadges));

    }
}
