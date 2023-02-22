using Desafio.Api.Domain.Entities;
using Desafio.Api.Infra.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Desafio.Api.Features.UserManagement.DeleteUser;

public sealed class DeleteUserTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;

    public DeleteUserTests()
        => _mockContext = new Mock<ApplicationDbContext>();

    [Fact]
    public async Task Given_That_Delete_Command_And_Is_Valid_And_User_Exists_When_Handle_Delete_Command_Should_Be_Delete_User()
    {
        //Arrange
        DeleteUserCommand command = new(1);
        var cancellationToken = CancellationToken.None;
        var mockUser = new Mock<User>();
        mockUser.SetupGet(x => x.Id).Returns(command.Id);
        
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

        DeleteUserCommandHandler handler = new(_mockContext.Object);

        //Act
        var result = await handler.Handle(command, cancellationToken);

        //Assert
        result.IsSuccess.Should().BeTrue();
        
        _mockContext.Verify(
            x => x.Users.Remove(It.Is<User>(u => u.Id == command.Id)),
            Times.Once);

        _mockContext.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }
}