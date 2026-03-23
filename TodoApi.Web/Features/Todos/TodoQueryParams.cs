using System.ComponentModel.DataAnnotations;

namespace TodoApi.Web.Features.Todos;

public record TodoQueryParams
(
    [Range(1, int.MaxValue, ErrorMessage = "Page harus minimal 1")]
    int? Page = 1,

    [Range(1, 50, ErrorMessage = "Size harus antara 1 sampai 50")]
    int? Size = 10,

    string? Search = null,

    string? Sort = null
);

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
