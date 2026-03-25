using AutoMapper;

namespace TodoApi.Web.Features.Todos;

public enum UpdateResult { Success, NotFound, InvalidTitle }

public record UpdateResultWithData(UpdateResult Result, TodoResponseDto? Data = null);

public class TodoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TodoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TodoResponseDto?> GetById(int id, int userId)
    {
        var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);

        return todo is null ? null : _mapper.Map<TodoResponseDto>(todo);
    }

    public async Task<TodoResponseDto> Create(string title, int userId)
    {
        var dto = new CreateTodoDto(title.Trim());
        var todo = _mapper.Map<Todo>(dto);
        
        todo.UserId = userId;
        todo.CreatedBy = userId;
        todo.UpdatedBy = userId;
        todo.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Todos.AddAsync(todo);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TodoResponseDto>(todo);
    }

    public async Task<UpdateResultWithData> Update(int id, string? title, bool? isCompleted, int userId)
    {
        var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);

        if (todo is null) return new(UpdateResult.NotFound);

        if (title is not null) todo.Title = title.Trim();
        if (isCompleted.HasValue) todo.IsCompleted = isCompleted.Value;

        if (string.IsNullOrWhiteSpace(todo.Title))
            return new(UpdateResult.InvalidTitle);

        todo.UpdatedBy = userId;
        todo.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new(UpdateResult.Success, _mapper.Map<TodoResponseDto>(todo));
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);

        if (todo is null) return false;

        _unitOfWork.Todos.Delete(todo);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResult<TodoResponseDto>> GetPaged(TodoQueryParams query, int userId)
    {
        var paged = await _unitOfWork.Todos.GetPagedAsync(query, userId);

        return new PagedResult<TodoResponseDto>
        {
            Items = _mapper.Map<IEnumerable<TodoResponseDto>>(paged.Items),
            Page = paged.Page,
            Size = paged.Size,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages
        };
    }
}
