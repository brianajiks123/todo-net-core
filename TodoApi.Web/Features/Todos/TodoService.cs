using AutoMapper;

namespace TodoApi.Web.Features.Todos;

public class TodoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TodoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<Todo>> GetAll() 
        => await _unitOfWork.Todos.GetAllAsync();

    public async Task<Todo?> GetById(int id) 
        => await _unitOfWork.Todos.GetByIdAsync(id);

    public async Task<Todo> Create(string title)
    {
        var dto = new CreateTodoDto(title.Trim());
        var todo = _mapper.Map<Todo>(dto);

        await _unitOfWork.Todos.AddAsync(todo);
        await _unitOfWork.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> Update(int id, string? title, bool? isCompleted)
    {
        var todo = await GetById(id);
        if (todo is null) return false;

        if (title is not null) todo.Title = title.Trim();
        if (isCompleted.HasValue) todo.IsCompleted = isCompleted.Value;

        if (string.IsNullOrWhiteSpace(todo.Title))
            return false;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var todo = await GetById(id);
        if (todo is null) return false;

        _unitOfWork.Todos.Delete(todo);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Todo>> GetPaged(TodoQueryParams query)
        => await _unitOfWork.Todos.GetPagedAsync(query);
}
