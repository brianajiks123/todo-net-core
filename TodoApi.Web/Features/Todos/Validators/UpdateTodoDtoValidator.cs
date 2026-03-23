using FluentValidation;

namespace TodoApi.Web.Features.Todos;

public class UpdateTodoDtoValidator : AbstractValidator<UpdateTodoDto>
{
    public UpdateTodoDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Judul maksimal 200 karakter")
            .MinimumLength(1).When(x => x.Title != null)
            .WithMessage("Judul minimal 1 karakter (jika diisi)");
    }
}
