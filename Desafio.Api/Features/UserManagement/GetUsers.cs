using Desafio.Api.Common;
using Desafio.Api.Infra.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Features.UserManagement.GetUsers;

public sealed class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] GetUsersQuery query,
        [FromServices] GetUsersQueryValidator validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

        var usersViewModel = await Mediator.Send(query, cancellationToken); 
        
        if(!usersViewModel.Any()) 
            return NoContent();

        return Ok(usersViewModel);
    }
}

public readonly record struct GetUsersViewModel(
    int Id,
    string Name, 
    string Email);

public record class GetUsersQuery(
    int? Id,
    string? Name, 
    string? Email) 
    : IRequest<IEnumerable<GetUsersViewModel>>;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(m => m.Name)
            .MaximumLength(250);

        RuleFor(m => m.Email)
            .MaximumLength(250)
            .EmailAddress();
    }
}

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<GetUsersViewModel>>
{
    private readonly ApplicationDbContext _context;

    public GetUsersQueryHandler(ApplicationDbContext context)
        => _context = context;

    public Task<IEnumerable<GetUsersViewModel>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = _context.Users.AsQueryable();
        
        if (query.Id.GetValueOrDefault() > 0)
            users = users.Where(u => u.Id == query.Id);

        if (!string.IsNullOrEmpty(query.Name))
            users = users.Where(u => u.Name.Equals(query.Name, StringComparison.InvariantCultureIgnoreCase));

        if (!string.IsNullOrEmpty(query.Email))
            users = users.Where(u => u.Email.Equals(query.Email, StringComparison.InvariantCultureIgnoreCase));

        var result = users.Select(u => new GetUsersViewModel(u.Id, u.Name, u.Email)) ?? Enumerable.Empty<GetUsersViewModel>();

        return Task.FromResult(result);
    }
}