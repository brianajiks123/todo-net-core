using FluentValidation;

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
