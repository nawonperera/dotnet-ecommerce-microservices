using AutoFixture;
using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.Services;
using Moq;
using Xunit;

namespace eCommerce.UserService.UsersUnitTests;

public class UsersServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UsersService _usersService;

    public UsersServiceTests()
    {
        _fixture = new Fixture();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();

        _usersService = new UsersService(
            _userRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetUserByUserID_ExistingUser_ReturnsUserDTO()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applicationUser = _fixture.Create<ApplicationUser>();
        var userDto = _fixture.Create<UserDTO>();

        _userRepositoryMock.Setup(repo => repo.GetUsersByUserID(userId))
            .ReturnsAsync(applicationUser);
        _mapperMock.Setup(mapper => mapper.Map<UserDTO>(applicationUser))
            .Returns(userDto);

        // Act
        var result = await _usersService.GetUserByUserID(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto, result);
    }

    [Fact]
    public async Task GetUserByUserID_NonExistingUser_ReturnsNullUserDTO()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(repo => repo.GetUsersByUserID(userId))
            .ReturnsAsync((ApplicationUser?)null);
        _mapperMock.Setup(mapper => mapper.Map<UserDTO>(It.IsAny<ApplicationUser>()))
            .Returns((UserDTO?)null!);

        // Act
        var result = await _usersService.GetUserByUserID(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthenticationResponse()
    {
        // Arrange
        var loginRequest = _fixture.Create<LoginRequest>();
        var applicationUser = _fixture.Create<ApplicationUser>();
        var authResponse = _fixture.Build<AuthenticationResponse>()
                                   .With(r => r.Success, false)
                                   .With(r => r.Token, string.Empty)
                                   .Create();

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(applicationUser);
        _mapperMock.Setup(mapper => mapper.Map<AuthenticationResponse>(applicationUser))
            .Returns(authResponse);

        // Act
        var result = await _usersService.Login(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("token", result.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsNull()
    {
        // Arrange
        var loginRequest = _fixture.Create<LoginRequest>();

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _usersService.Login(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Register_SuccessfulRegistration_ReturnsAuthenticationResponse()
    {
        // Arrange
        var registerRequest = _fixture.Create<RegisterRequest>();
        var applicationUser = _fixture.Create<ApplicationUser>();
        var addedUser = _fixture.Create<ApplicationUser>();
        var authResponse = _fixture.Build<AuthenticationResponse>()
                                   .With(r => r.Success, false)
                                   .With(r => r.Token, string.Empty)
                                   .Create();

        _mapperMock.Setup(mapper => mapper.Map<ApplicationUser>(registerRequest))
            .Returns(applicationUser);
        _userRepositoryMock.Setup(repo => repo.AddUser(applicationUser))
            .ReturnsAsync(addedUser);
        _mapperMock.Setup(mapper => mapper.Map<AuthenticationResponse>(applicationUser))
            .Returns(authResponse);

        // Act
        var result = await _usersService.Register(registerRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("token", result.Token);
    }

    [Fact]
    public async Task Register_FailedRegistration_ReturnsNull()
    {
        // Arrange
        var registerRequest = _fixture.Create<RegisterRequest>();
        var applicationUser = _fixture.Create<ApplicationUser>();

        _mapperMock.Setup(mapper => mapper.Map<ApplicationUser>(registerRequest))
            .Returns(applicationUser);
        _userRepositoryMock.Setup(repo => repo.AddUser(applicationUser))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _usersService.Register(registerRequest);

        // Assert
        Assert.Null(result);
    }
}
