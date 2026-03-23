using FluentValidation;

namespace TodoApi.Web.Features.Todos;

public class CreateTodoDtoValidator : AbstractValidator<CreateTodoDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTodoDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul todo wajib diisi")
            .Must(title => !string.IsNullOrWhiteSpace(title?.Trim()))
                .WithMessage("Judul tidak boleh hanya whitespace")
            .MinimumLength(1)
            .MaximumLength(200);

        RuleFor(x => x.Title)
            .MustAsync(async (title, ct) =>
            {
                if (string.IsNullOrWhiteSpace(title)) return true;
                return !await _unitOfWork.Todos.ExistsWithTitleAsync(title, ct);
            })
            .WithMessage("Judul sudah ada");
    }
}
