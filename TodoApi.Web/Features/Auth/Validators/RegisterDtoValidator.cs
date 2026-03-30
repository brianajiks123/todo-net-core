using FluentValidation;

namespace TodoApi.Web.Features.Auth.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50)
            .MustAsync(async (u, ct) => !await _unitOfWork.Users.ExistsByUsernameAsync(u, ct))
            .WithMessage("Username is already taken.");

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
