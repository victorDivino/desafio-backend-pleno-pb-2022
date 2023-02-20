using Desafio.Api.Domain.Entities;
using Desafio.Api.Infra.Data;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Api.Features.UserManagement.GetUsers;

public sealed class GetUsersTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;

    public GetUsersTests()
        => _mockContext = new Mock<ApplicationDbContext>();

    [Fact]
    public async Task Given_A_Users_List_When_Get_User_By_Email_Should_Returns_Only_One_User_With_The_Same_Email()
    {
        //Arrange
        GetUsersQuery query = new(null, null, "admin@domian.com");

        var users = new List<User>()
        {
            User.Create("user", "user@domian.com"),
            User.Create("admin", query.Email!)
        }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());
        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        GetUsersQueryHandler handler = new(_mockContext.Object);

        //Act
        var result = await handler.Handle(query, default);

        //Assert
        result.Should()
            .ContainSingle(x => x.Email.Equals(query.Email))
            .And.HaveCount(1);
    }
}

