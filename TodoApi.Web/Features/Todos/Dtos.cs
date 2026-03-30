using System.ComponentModel.DataAnnotations;

namespace TodoApi.Web.Features.Todos;

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
    DateTime UpdatedAt
);
