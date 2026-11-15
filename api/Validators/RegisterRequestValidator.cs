using FluentValidation;
using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Services;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(AppDbContext db)
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Укажите email")
            .EmailAddress().WithMessage("Неверный формат email")
            .MaximumLength(256).WithMessage("Не длиннее 256 символов")
            .MustAsync(async (email, ct) =>
            {
                var normalized = AuthService.Normalize(email);
                return !await db.Users.AnyAsync(u => u.Email == normalized, ct);
            })
            .WithMessage("Этот email уже зарегистрирован");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Укажите пароль")
            .MinimumLength(8).WithMessage("Пароль — от 8 символов")
            .Matches("[0-9]").WithMessage("Нужна хотя бы одна цифра");
    }
}
