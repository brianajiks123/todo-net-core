using TodoApi.Web.Features.Todos.Mapping;
using AutoMapper;

namespace TodoApi.Web.Features.Todos.Mapping;

public class TodoProfile : Profile
{
    public TodoProfile()
    {
        CreateMap<CreateTodoDto, Todo>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(_ => false));
    }
}
