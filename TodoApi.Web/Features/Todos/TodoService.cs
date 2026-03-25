using AutoMapper;

namespace TodoApi.Web.Features.Todos;

public enum UpdateResult { Success, NotFound, InvalidTitle }

public class TodoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TodoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<Todo>> GetAll(int userId) 
        => await _unitOfWork.Todos.GetAllByUserIdAsync(userId);

    public async Task<Todo?> GetById(int id, int userId) 
        => await _unitOfWork.Todos.GetByIdAsync(id, userId);

    public async Task<Todo> Create(string title, int userId)
    {
        var dto = new CreateTodoDto(title.Trim());
        var todo = _mapper.Map<Todo>(dto);

        todo.UserId = userId;

        await _unitOfWork.Todos.AddAsync(todo);
        await _unitOfWork.SaveChangesAsync();
        return todo;
    }

    public async Task<UpdateResult> Update(int id, string? title, bool? isCompleted, int userId)
    {
        var todo = await GetById(id, userId);

        if (todo is null) return UpdateResult.NotFound;

        if (title is not null) todo.Title = title.Trim();
        if (isCompleted.HasValue) todo.IsCompleted = isCompleted.Value;

        if (string.IsNullOrWhiteSpace(todo.Title))
            return UpdateResult.InvalidTitle;

        await _unitOfWork.SaveChangesAsync();
        return UpdateResult.Success;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var todo = await GetById(id, userId);
        if (todo is null) return false;

        _unitOfWork.Todos.Delete(todo);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Todo>> GetPaged(TodoQueryParams query, int userId)
        => await _unitOfWork.Todos.GetPagedAsync(query, userId);
}
