using Desafio.Api.Common;
using Desafio.Api.Infra.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OperationResult;

namespace Desafio.Api.Features.UserManagement.UpdateUser;

public sealed class UsersController : ApiControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateUserCommand command,
        [FromServices] UpdateUserCommandValidator validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Exception.Message);

        return Ok(result.Value);
    }
}

public readonly record struct UpdateUserViewModel(
    int Id,
    string Name,
    string Email);

public readonly record struct UpdateUserCommand(
    int Id,
    string Name,
    string Email)
    : IRequest<Result<UpdateUserViewModel>>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(m => m.Id)
           .NotEmpty();

        RuleFor(m => m.Name)
            .NotEmpty()
            .Length(3, 250);

        RuleFor(m => m.Email)
            .NotEmpty()
            .MaximumLength(250)
            .EmailAddress();
    }
}

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UpdateUserViewModel>>
{
    private readonly ApplicationDbContext _context;

    public UpdateUserCommandHandler(ApplicationDbContext context)
        => _context = context;

    public async Task<Result<UpdateUserViewModel>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == command.Id);

        if (user is null)
            return UpdateUserExceptions.NotFound;

        var hasOtherUserWithEmail = _context.Users.Any(
            x => x.Id != user.Id &&
            x.Email.ToLower() == command.Email.Trim().ToLower());

        if (hasOtherUserWithEmail)
            return UpdateUserExceptions.EmailDuplicate;

        user.Update(command.Name, command.Email);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateUserViewModel(user.Id, user.Name, user.Email);
    }
}

public static class UpdateUserExceptions
{
    public static UpdateUserException NotFound => new UpdateUserException("User not found");
    public static UpdateUserException EmailDuplicate => new UpdateUserException("New email is already being used");
}

public sealed class UpdateUserException : Exception
{
    public UpdateUserException(string message) : base(message) { }
}