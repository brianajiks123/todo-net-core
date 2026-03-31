using TodoApi.Web.Features.Todos.Mapping;
using AutoMapper;

namespace TodoApi.Web.Features.Todos.Mapping;

public class TodoProfile : Profile
{
    public TodoProfile()
    {
        // v1
        CreateMap<CreateTodoDto, Todo>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        // v2
        CreateMap<CreateTodoDtoV2, Todo>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate.HasValue ? src.DueDate.Value.UtcDateTime : (DateTime?)null))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<Todo, TodoResponseDto>();
    }
}
