using EcomPostgresDb.Application.Common.Interfaces;
using EcomPostgresDb.Application.Common.Models;
using EcomPostgresDb.Domain.Entities;
using EcomPostgresDb.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace EcomPostgresDb.Application.Features.Customers.Commands;

// ─── Register Customer ────────────────────────────────────────────────────────

public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<Guid>>;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class RegisterCustomerCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    : IRequestHandler<RegisterCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCustomerCommand cmd, CancellationToken ct)
    {
        if (await uow.Customers.EmailExistsAsync(cmd.Email, ct))
            return Result.Failure<Guid>("Email address is already registered.");

        var hash = hasher.Hash(cmd.Password);
        var customer = Customer.Create(cmd.FirstName, cmd.LastName, cmd.Email, hash);

        await uow.Customers.AddAsync(customer, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Success(customer.Id);
    }
}

// ─── Login Customer ───────────────────────────────────────────────────────────

public record LoginCustomerCommand(string Email, string Password) : IRequest<Result<string>>;

public sealed class LoginCustomerCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokenService)
    : IRequestHandler<LoginCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await uow.Customers.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct);
        if (customer is null || !hasher.Verify(cmd.Password, customer.PasswordHash))
            return Result.Failure<string>("Invalid email or password.");

        if (!customer.IsActive)
            return Result.Failure<string>("Account is deactivated.");

        var token = tokenService.GenerateToken(customer.Id, customer.Email, "Customer");
        return Result.Success(token);
    }
}
