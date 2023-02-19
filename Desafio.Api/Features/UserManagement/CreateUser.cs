using Desafio.Api.Common;
using Desafio.Api.Infra.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Features.UserManagement;

public sealed class CreateTodoListController : ApiControllerBase
{
    [HttpPost("/api/user")]
    public async Task<ActionResult<int>> Create(CreateUserCommand command, [FromServices] CreateUserCommandValidator validator)
    {
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

        return await Mediator.Send(command);
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
        
        await _context.Users.AddAsync(newUser);
        
        await _context.SaveChangesAsync();

        return newUser.Id;
    }
}