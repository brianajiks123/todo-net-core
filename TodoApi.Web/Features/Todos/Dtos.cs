using System.ComponentModel.DataAnnotations;

namespace TodoApi.Web.Features.Todos;

// v1
public record CreateTodoDto
(
    [Required(ErrorMessage = "Todo title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    string Title
);

public record UpdateTodoDto
(
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters (if provided).")]
    string? Title = null,
    bool? IsCompleted = null
);

public record TodoResponseDto
(
    int Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    int CreatedBy,
    int UpdatedBy,
    DateTime UpdatedAt,
    DateTime? DueDate = null
);

// v2
public record CreateTodoDtoV2
(
    [Required(ErrorMessage = "Todo title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    string Title,
    DateTimeOffset? DueDate = null
);

public record UpdateTodoDtoV2
(
    string? Title = null,
    bool? IsCompleted = null,
    DateTimeOffset? DueDate = null
);
