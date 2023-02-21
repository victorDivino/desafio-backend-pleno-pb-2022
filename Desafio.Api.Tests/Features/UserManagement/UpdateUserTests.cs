using Desafio.Api.Domain.Entities;
using Desafio.Api.Infra.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace Desafio.Api.Features.UserManagement.UpdateUser;

public sealed class UpdateUserTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;

    public UpdateUserTests()
        => _mockContext = new Mock<ApplicationDbContext>();

    [Fact]
    public async Task Given_Command_And_Id_Invalid_When_Update_Should_Returns_BadRequest()
    {
        //Arrange
        UpdateUserCommand command = new();
        UpdateUserCommandValidator validator = new();
        UsersController controller = new();

        //Act
        IActionResult result = await controller.Update(command, validator, default);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Given_Command_And_Valid_When_Handle_Should_Update_User()
    {
        //Arrange
        UpdateUserCommand command = new(1, "New Name", "new@domain.com");
        var cancellationToken = CancellationToken.None;

        var mockUser = new Mock<User>();
        mockUser.SetupGet(x => x.Id).Returns(command.Id);
        mockUser.Object.Update("old Name", "old_email@domain.com");

        var users = new List<User>()
        {
            mockUser.Object
        }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());
        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        UpdateUserCommandHandler handler = new(_mockContext.Object);

        //Act
        var (_, result, _) = await handler.Handle(command, cancellationToken);

        //Assert
        result.Name.Should().Be(command.Name);
        result.Email.Should().Be(command.Email);

        _mockContext.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

     [Fact]
    public async Task Given_Command_And_Valid_New_Email_Already_Used_When_Handle_Should_Update_User()
    {
        //Arrange
        UpdateUserCommand command = new(2, "New Name", "new@domain.com");
        var cancellationToken = CancellationToken.None;

        var mockUser = new Mock<User>();
        mockUser.SetupGet(x => x.Id).Returns(command.Id);
        mockUser.Object.Update("old Name", "old_email@domain.com");

        var mockOthenUser = new Mock<User>();
        mockOthenUser.SetupGet(x => x.Id).Returns(command.Id + 1);
        mockOthenUser.Object.Update("Other Name", command.Email);

        var users = new List<User>()
        {
            mockUser.Object,
            mockOthenUser.Object
        }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());
        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        UpdateUserCommandHandler handler = new(_mockContext.Object);

        //Act
         var (isSuccess, _, exception) = await handler.Handle(command, cancellationToken);

        //Assert
        isSuccess.Should().BeFalse();
        exception.Should().BeOfType<UpdateUserException>();
        exception!.Message.Should().Be(UpdateUserExceptions.EmailDuplicate.Message);
        
        _mockContext.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Never);
    }

    [Fact]
    public async Task Given_Command_And_Valid_And_User_Not_Exists_When_Handle_Should_Returns_Not_Found_Exception()
    {
        //Arrange
        UpdateUserCommand command = new(10, "New Name", "test@domain.com");
        var cancellationToken = CancellationToken.None;
        _mockContext.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<User>());

        UpdateUserCommandHandler handler = new(_mockContext.Object);

        //Act
        var (isSuccess, _, exception) = await handler.Handle(command, cancellationToken);

        //Assert
        isSuccess.Should().BeFalse();
        exception.Should().BeOfType<UpdateUserException>();
        exception!.Message.Should().Be(UpdateUserExceptions.NotFound.Message);
        
        _mockContext.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Never);
    }
}