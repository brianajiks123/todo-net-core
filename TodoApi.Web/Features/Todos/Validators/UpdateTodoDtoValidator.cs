using FluentValidation;

namespace TodoApi.Web.Features.Todos;

public class UpdateTodoDtoValidator : AbstractValidator<UpdateTodoDto>
{
    public UpdateTodoDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .MinimumLength(1).When(x => x.Title != null)
            .WithMessage("Title must be at least 1 character (if provided).");
    }
}
