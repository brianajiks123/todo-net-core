using TodoApi.Web.Features.Todos.Mapping;
using AutoMapper;

namespace TodoApi.Web.Features.Todos.Mapping;

public class TodoProfile : Profile
{
    public TodoProfile()
    {
        CreateMap<CreateTodoDto, Todo>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<Todo, TodoResponseDto>();
    }
}
