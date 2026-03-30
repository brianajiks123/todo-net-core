using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TodoApi.Web.Features.Todos;

public class CreateTodoDtoValidator : AbstractValidator<CreateTodoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateTodoDtoValidator(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Todo title is required.")
            .Must(title => !string.IsNullOrWhiteSpace(title?.Trim()))
                .WithMessage("Title must not be whitespace only.")
            .MinimumLength(1)
            .MaximumLength(200);

        RuleFor(x => x.Title)
            .MustAsync(async (title, ct) =>
            {
                if (string.IsNullOrWhiteSpace(title)) return true;
                var userId = GetCurrentUserId();
                return !await _unitOfWork.Todos.ExistsWithTitleAsync(title, userId, ct);
            })
            .WithMessage("A todo with this title already exists.");
    }

    private int GetCurrentUserId()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new InvalidOperationException("User ID not found in token.");

        return userId;
    }
}
