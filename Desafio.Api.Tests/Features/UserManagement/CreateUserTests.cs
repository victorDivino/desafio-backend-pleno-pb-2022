using Desafio.Api.Domain.Entities;
using Desafio.Api.Infra.Data;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.EntityFrameworkCore;

namespace Desafio.Api.Features.UserManagement;

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
        CreateUserController controller = new();

        //Act
        IActionResult result = await controller.Create(command, validator, default);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Given_Command_And_Valid_When_Create_Should_Returns_Ok()
    {
        //Arrange
        CreateUserCommand command = new()
        {
            Name = "test",
            Email = "test@domain.com"
        };
        CreateUserCommandValidator validator = new();
        CreateUserController controller = new();
        var cancellationToken = CancellationToken.None;

        _mockMediator.Setup(x => x.Send(command, cancellationToken)).ReturnsAsync((int)default);
        _mockServices.Setup(x => x.GetService(typeof(ISender))).Returns(_mockMediator.Object);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.RequestServices = _mockServices.Object;

        //Act
        var result = await controller.Create(command, validator, default);

        //Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Given_Command_And_Valid_When_Handle_Should_Create_User()
    {
        //Arrange
        CreateUserCommand command = new()
        {
            Name = "test",
            Email = "test@domain.com"
        };
        var cancellationToken = CancellationToken.None;
        CreateUserCommandHandler handler = new(_mockContext.Object);

        _mockContext.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<User>());

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