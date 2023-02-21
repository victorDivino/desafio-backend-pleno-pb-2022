using Desafio.Api.Domain.Entities;
using Desafio.Api.Infra.Data;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;

namespace Desafio.Api.Features.UserManagement.CreateUser;

public sealed class CreateUserTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IServiceProvider> _mockServices;
    private readonly Mock<ISender> _mockMediator;

    public CreateUserTests()
    {
        _mockContext = new Mock<ApplicationDbContext>();
        _mockServices = new Mock<IServiceProvider>();
        _mockMediator = new Mock<ISender>();
    }

    [Fact]
    public async Task Given_Command_And_InValid_When_Create_Should_Returns_BadRequest()
    {
        //Arrange
        CreateUserCommand command = new();
        CreateUserCommandValidator validator = new();
        UsersController controller = new();

        //Act
        IActionResult result = await controller.Create(command, validator, default);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Given_Command_And_Valid_When_Create_Should_Returns_Ok()
    {
        //Arrange
        CreateUserCommand command = new("test", "test@domain.com");
        CreatedUserViewModel viewModel = new(default, command.Name, command.Email);
        CreateUserCommandValidator validator = new();
        UsersController controller = new();
        var cancellationToken = CancellationToken.None;

        _mockMediator.Setup(x => x.Send(command, cancellationToken)).ReturnsAsync(viewModel);
        _mockServices.Setup(x => x.GetService(typeof(ISender))).Returns(_mockMediator.Object);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.RequestServices = _mockServices.Object;

        //Act
        var result = await controller.Create(command, validator, default);

        //Assert
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status201Created);
        result.As<ObjectResult>().Value.Should().Be(viewModel);
    }

    [Fact]
    public async Task Given_Command_And_Valid_When_Handle_Should_Create_User()
    {
        //Arrange
        CreateUserCommand command = new("test", "test@domain.com");
        var cancellationToken = CancellationToken.None;
        _mockContext.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<User>());
        CreateUserCommandHandler handler = new(_mockContext.Object);

        //Act
        await handler.Handle(command, cancellationToken);

        //Assert
        _mockContext.Verify(x => x.Users.AddAsync(
            It.Is<User>(
                m => m.Name.Equals(command.Name) &&
                m.Email.Equals(command.Email)),
                cancellationToken),
                Times.Once);

        _mockContext.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }
}