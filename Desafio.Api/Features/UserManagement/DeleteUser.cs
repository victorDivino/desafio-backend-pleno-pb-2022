using Desafio.Api.Common;
using Desafio.Api.Infra.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OperationResult;

namespace Desafio.Api.Features.UserManagement.DeleteUser;

public sealed class UsersController : ApiControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromServices] DeleteUserCommandValidator validator,
        CancellationToken cancellationToken)
    {
        DeleteUserCommand command = new(id);

        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Exception.Message);

        return Ok();
    }
}

public readonly record struct DeleteUserCommand(int Id) : IRequest<Result>;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(m => m.Id)
           .NotEmpty();
    }
}

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly ApplicationDbContext _context;

    public DeleteUserCommandHandler(ApplicationDbContext context)
        => _context = context;

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == command.Id);

        if (user is null)
            return DeleteUserExceptions.NotFound;

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}

public static class DeleteUserExceptions
{
    public static DeleteUserException NotFound => new DeleteUserException("User not found");
}

public sealed class DeleteUserException : Exception
{
    public DeleteUserException(string message) : base(message) { }
}

