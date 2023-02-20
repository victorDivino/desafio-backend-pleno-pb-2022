using Desafio.Api.Common;
using Desafio.Api.Infra.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Features.UserManagement;

public sealed class CreateUserController : ApiControllerBase
{
    [HttpPost("/api/user")]
    public async Task<IActionResult> Create(CreateUserCommand command, [FromServices] CreateUserCommandValidator validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

public record struct CreateUserCommand : IRequest<int>
{
    public string Name { get; init; }
    public string Email { get; init; }
}

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(m => m.Name)
            .NotEmpty()
            .Length(3, 250);

        RuleFor(m => m.Email)
            .NotEmpty()
            .MaximumLength(250)
            .EmailAddress();
    }
}

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly ApplicationDbContext _context;

    public CreateUserCommandHandler(ApplicationDbContext context) 
        => _context = context;

    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var newUser = Domain.Entities.User.Create(command.Name, command.Email);
        
        await _context.Users.AddAsync(newUser, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}