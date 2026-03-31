using FluentValidation;

namespace TodoApi.Web.Features.Todos;

public class CreateTodoDtoV2Validator : AbstractValidator<CreateTodoDtoV2>
{
    public CreateTodoDtoV2Validator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Todo title is required.")
            .MinimumLength(1).MaximumLength(200);

        RuleFor(x => x.DueDate)
            .Must(d => d!.Value.UtcDateTime > DateTime.UtcNow)
            .WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
