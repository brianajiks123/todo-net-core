using System.ComponentModel.DataAnnotations;

namespace TodoApi.Web.Features.Todos;

public record CreateTodoDto
(
    [Required(ErrorMessage = "Judul todo wajib diisi")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Judul harus antara 1 sampai 200 karakter")]
    string Title
);

public record UpdateTodoDto
(
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Judul harus antara 1 sampai 200 karakter (jika diisi)")]
    string? Title = null,

    bool? IsCompleted = null
);
