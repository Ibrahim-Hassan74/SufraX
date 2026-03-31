using AutoMapper;
using Domain.Entities.Common;
using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.RepositoryContracts.Common;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Services.Account;
using EStoreX.Core.Services.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

namespace E_StoreX.ServiceTests
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IEmailSenderService> _emailSenderMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IUserManagementService> _userManagementServiceMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthenticationService _authenticationService;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IEntityImageManager<ApplicationUser>> _imageManagerMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IStringLocalizer<AuthenticationService>> _localizerMock;


        public AuthenticationServiceTests()
        {
            // UserManager<ApplicationUser> requires IUserStore
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            _authenticationServiceMock = new Mock<IAuthenticationService>();

            // SignInManager<ApplicationUser> requires UserManager + context
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

            // RoleManager<ApplicationRole> requires IRoleStore
            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                roleStoreMock.Object, null, null, null, null);

            _emailSenderMock = new Mock<IEmailSenderService>();
            _httpContextAccessorMock = contextAccessor;
            _jwtServiceMock = new Mock<IJwtService>();
            _userManagementServiceMock = new Mock<IUserManagementService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _imageManagerMock = new Mock<IEntityImageManager<ApplicationUser>>();
            _imageServiceMock = new Mock<IImageService>();
            _configurationMock = new Mock<IConfiguration>();
            _localizerMock = new Mock<IStringLocalizer<AuthenticationService>>();

            _authenticationService = new AuthenticationService(
                _userManagerMock.Object,
                _emailSenderMock.Object,
                _signInManagerMock.Object,
                _httpContextAccessorMock.Object,
                _jwtServiceMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _userManagementServiceMock.Object,
                _roleManagerMock.Object,
                _imageManagerMock.Object,
                _imageServiceMock.Object,
                _configurationMock.Object,
                _localizerMock.Object
            );
        }

        #region RegisterUserAsync Tests

        [Fact]
        public async Task RegisterAsync_ShouldReturnFailure_WhenDtoIsNull()
        {
            // Act
            var result = await _authenticationService.RegisterAsync(null, null) as ApiErrorResponse;

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid registration data.");
            result.Errors.Should().Contain("Registration data cannot be null.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "Ibrahim",
                Email = "test@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync(new ApplicationUser());


            // Act
            var result = await _authenticationService.RegisterAsync(dto, null) as ApiErrorResponse;

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(409);
            result.Message.Should().Be("Email is already registered.");
            result.Errors.Should().Contain("This email is already in use.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFailure_WhenUserCreationFails()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "Ibrahim",
                Email = "fail@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

            // Act
            var result = await _authenticationService.RegisterAsync(dto, null) as ApiErrorResponse;

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Registration failed.");
            result.Errors.Should().Contain("Weak password");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccessAndAssignRole_WhenUserCreationSucceeds()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "Ibrahim",
                Email = "success@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var user = new ApplicationUser { Email = dto.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(UserTypeOptions.User.ToString()))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserTypeOptions.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);


            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-token");

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.RegisterAsync(dto, null);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Registration successful. Please check your email to confirm your account.");

            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserTypeOptions.User.ToString()), Times.Once);
        }


        [Fact]
        public async Task RegisterAsync_ShouldSendEmailWithToken_WhenUserCreatedSuccessfully()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "Ibrahim",
                Email = "mail@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-token");

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.RegisterAsync(dto, null);

            // Assert
            _userManagerMock.Verify(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u => u.LastEmailConfirmationToken == "test-token")), Times.Once);
            _emailSenderMock.Verify(s => s.SendEmailAsync(It.Is<EmailDTO>(e => e.Email == dto.Email && e.Subject.Contains("Confirm"))), Times.Once);
        }


        [Fact]
        public async Task RegisterAsync_ShouldFailValidation_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "Ibrahim",
                Email = "invalid@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "NotMatching"
            };

            // Act
            Func<Task> act = async () => await _authenticationService.RegisterAsync(dto, null);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Passwords do not match*");
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateRole_WhenRoleDoesNotExist()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "UserTest",
                Email = "rolecheck@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(UserTypeOptions.User.ToString()))
                .ReturnsAsync(false);

            _roleManagerMock
                .Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);


            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-token");

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _authenticationService.RegisterAsync(dto, null);

            // Assert
            _roleManagerMock.Verify(r => r.CreateAsync(It.Is<ApplicationRole>(x => x.Name == UserTypeOptions.User.ToString())), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAssignRoleToUser_WhenUserCreated()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "RoleAssign",
                Email = "assign@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var user = new ApplicationUser { Email = dto.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(r => r.RoleExistsAsync(UserTypeOptions.User.ToString()))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserTypeOptions.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);


            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-token");

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _authenticationService.RegisterAsync(dto, null);

            // Assert
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), UserTypeOptions.User.ToString()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldGenerateTokenAndUpdateUser_WhenSendingEmail()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                UserName = "EmailToken",
                Email = "token@test.com",
                Phone = "123456",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test-token");

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _authenticationService.RegisterAsync(dto, null);

            // Assert
            _userManagerMock.Verify(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u => u.LastEmailConfirmationToken == "test-token")), Times.Once);
            _emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<EmailDTO>()), Times.Once);
        }


        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenDtoIsNull()
        {
            // Act
            var result = await _authenticationService.LoginAsync(null);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid login data.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoginDTO { Email = "notfound@test.com", Password = "Password123" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task LoginAsync_ShouldSendEmail_WhenEmailNotConfirmed()
        {
            // Arrange
            var dto = new LoginDTO { Email = "ibrahim@test.com", Password = "Password123" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email, EmailConfirmed = false };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("token");
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(403);
            _emailSenderMock.Verify(e => e.SendEmailAsync(It.Is<EmailDTO>(m => m.Email == user.Email)), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnSuccess_WhenPasswordSignInSucceeds()
        {
            // Arrange
            var dto = new LoginDTO { Email = "success@test.com", Password = "Password123", RememberMe = true };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, true))
                .ReturnsAsync(SignInResult.Success);

            _jwtServiceMock.Setup(j => j.CreateJwtToken(user, dto.RememberMe)).ReturnsAsync(new ApiSuccessResponse
            {
                RefreshToken = "refresh",
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddDays(7)
            });

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Login successful.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenLockedOut()
        {
            // Arrange
            var dto = new LoginDTO { Email = "lock@test.com", Password = "Password123" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, true))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(423);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenNotAllowed()
        {
            // Arrange
            var dto = new LoginDTO { Email = "notallowed@test.com", Password = "Password123" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, true))
                .ReturnsAsync(SignInResult.NotAllowed);

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenInvalidPassword()
        {
            // Arrange
            var dto = new LoginDTO { Email = "wrong@test.com", Password = "wrong11" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, true))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _authenticationService.LoginAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
        }

        #endregion

        #region ConfirmEmailAsync Tests
        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new ConfirmEmailDTO { UserId = "123", Token = "validToken123" };
            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authenticationService.ConfirmEmailAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnFailure_WhenEmailAlreadyConfirmed()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), EmailConfirmed = true };
            var dto = new ConfirmEmailDTO { UserId = user.Id.ToString(), Token = "validToken123" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);

            var result = await _authenticationService.ConfirmEmailAsync(dto);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Email is already confirmed.");
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnFailure_WhenTokenMismatch()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), EmailConfirmed = false, LastEmailConfirmationToken = "realToken123" };
            var dto = new ConfirmEmailDTO { UserId = user.Id.ToString(), Token = "wrongToken" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            var result = await _authenticationService.ConfirmEmailAsync(dto);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid or expired confirmation token.");
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnSuccess_WhenConfirmationSucceeds()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), EmailConfirmed = false, LastEmailConfirmationToken = "validToken123" };
            var dto = new ConfirmEmailDTO { UserId = user.Id.ToString(), Token = "validToken123" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(false);
            _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, dto.Token)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.RemoveAuthenticationTokenAsync(user,It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var result = await _authenticationService.ConfirmEmailAsync(dto);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Email confirmed successfully.");
            user.LastEmailConfirmationToken.Should().BeNull();
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnFailure_WhenConfirmationFails()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), EmailConfirmed = false, LastEmailConfirmationToken = "validToken123" };
            var dto = new ConfirmEmailDTO { UserId = user.Id.ToString(), Token = "validToken123" };

            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid token" });

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(false);
            _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, dto.Token)).ReturnsAsync(identityResult);

            var result = await _authenticationService.ConfirmEmailAsync(dto) as ApiErrorResponse;

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Invalid token");
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new ForgotPasswordDTO { Email = "notfound@test.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Incorrect email.");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldReturnFailure_WhenEmailNotConfirmed()
        {
            var dto = new ForgotPasswordDTO { Email = "user@test.com" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("confirm your email");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldReturnFailure_WhenUserHasExternalLogins()
        {
            var dto = new ForgotPasswordDTO { Email = "external@test.com" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(new List<UserLoginInfo> { new UserLoginInfo("Google", "id", "Google") });

            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("external provider");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldReturnFailure_WhenResetTokenWasSentRecently()
        {
            var dto = new ForgotPasswordDTO { Email = "user@test.com" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetLoginsAsync(user)).ReturnsAsync(new List<UserLoginInfo>());

            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync("existing-token");
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.ToString());

            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(429);
            result.Message.Should().Contain("already sent recently");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldGenerateNewToken_WhenNoExistingToken()
        {
            var dto = new ForgotPasswordDTO { Email = "user@test.com" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetLoginsAsync(user)).ReturnsAsync(new List<UserLoginInfo>());
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync((string?)null);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("new-token");
            _userManagerMock.Setup(x => x.SetAuthenticationTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailDTO>())).Returns(Task.CompletedTask);
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost", 5001);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("reset link has been sent");
            _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailDTO>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldIncludeEncodedTokenInResetLink()
        {
            var dto = new ForgotPasswordDTO { Email = "user@test.com" };
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = dto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetLoginsAsync(user)).ReturnsAsync(new List<UserLoginInfo>());
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync((string?)null);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token-123");
            _userManagerMock.Setup(x => x.SetAuthenticationTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost", 5001);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            EmailDTO? sentEmail = null;
            _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailDTO>()))
                .Callback<EmailDTO>(email => sentEmail = email)
                .Returns(Task.CompletedTask);

            var result = await _authenticationService.ForgotPasswordAsync(dto, null);

            sentEmail.Should().NotBeNull();
            sentEmail!.HtmlMessage.Should().Contain($"reset-password?userId={user.Id}");
            sentEmail.HtmlMessage.Should().Contain("token=");
        }


        #endregion

        #region VerifyResetPasswordTokenAsync Tests

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenUserIdOrTokenIsMissing()
        {
            // Arrange
            var dto = new VerifyResetPasswordDTO { UserId = "", Token = "" };

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid verification request.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new VerifyResetPasswordDTO { UserId = "123", Token = "validToken" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenTokenFormatIsInvalid()
        {
            // Arrange
            var dto = new VerifyResetPasswordDTO { UserId = "123", Token = "###INVALID_BASE64###" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(new ApplicationUser());

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token format.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenStoredTokenIsNull()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new VerifyResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("reset-token"))
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid or expired token.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenStoredTokenDoesNotMatch()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new VerifyResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("wrong-token"))
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync("expected-token");

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid or expired token.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenTokenTimeIsInvalid()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var validToken = "valid-token";
            var dto = new VerifyResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(validToken))
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(validToken);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Token validation failed.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnFailure_WhenTokenIsExpired()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var validToken = "valid-token";
            var dto = new VerifyResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(validToken))
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(validToken);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.AddMinutes(-10).ToString());

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Reset password link has expired.");
        }

        [Fact]
        public async Task VerifyResetPasswordTokenAsync_ShouldReturnSuccess_WhenTokenIsValidAndNotExpired()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var validToken = "valid-token";
            var dto = new VerifyResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(validToken))
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(validToken);
            _userManagerMock.Setup(m => m.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.ToString());

            // Act
            var result = await _authenticationService.VerifyResetPasswordTokenAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("The reset password token is valid.");
        }


        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFailure_WhenDtoIsNull()
        {
            // Act
            var result = await _authenticationService.ResetPasswordAsync(null);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid reset password data.");
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFailure_WhenTokenFormatIsInvalid()
        {
            // Arrange
            var dto = new ResetPasswordDTO
            {
                UserId = Guid.NewGuid().ToString(),
                Token = "not-base64", // invalid 
                NewPassword = "Pass123!"
            };

            var user = new ApplicationUser { Id = Guid.Parse(dto.UserId), Email = "test@test.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync(dto.UserId)).ReturnsAsync(user);

            // Act
            var result = await _authenticationService.ResetPasswordAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid or expired token.");
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new ResetPasswordDTO
            {
                UserId = Guid.NewGuid().ToString(),
                Token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("reset-token")),
                NewPassword = "Pass123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(dto.UserId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authenticationService.ResetPasswordAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFailure_WhenResetPasswordFails()
        {
            // Arrange
            var rawToken = "reset-token";
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com" };
            var dto = new ResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = encoded,
                NewPassword = "Pass123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, rawToken, "Pass123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(rawToken);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.ToString());
            _userManagerMock.Setup(x => x.RemoveAuthenticationTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);


            // Act
            var result = await _authenticationService.ResetPasswordAsync(dto) as ApiErrorResponse;

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to reset password.");
            result.Errors.Should().Contain("Password too weak");
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldRemoveTokens_WhenPasswordResetSucceeds()
        {
            // Arrange
            var rawToken = "reset-token";
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com" };
            var dto = new ResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = encoded,
                NewPassword = "Pass123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, rawToken, "Pass123!"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(rawToken);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.ToString());
            _userManagerMock.Setup(x => x.RemoveAuthenticationTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);


            // Act
            var result = await _authenticationService.ResetPasswordAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password has been reset successfully.");
            _userManagerMock.Verify(x => x.RemoveAuthenticationTokenAsync(user, "ResetPassword", "Token"), Times.Once);
            _userManagerMock.Verify(x => x.RemoveAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldDecodeTokenBeforeReset()
        {
            // Arrange
            var rawToken = "reset-token";
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new ResetPasswordDTO
            {
                UserId = user.Id.ToString(),
                Token = encoded,
                NewPassword = "Pass123!"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, rawToken, "Pass123!"))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "Token"))
                .ReturnsAsync(rawToken);
            _userManagerMock.Setup(x => x.GetAuthenticationTokenAsync(user, "ResetPassword", "TokenTime"))
                .ReturnsAsync(DateTime.UtcNow.ToString());
            _userManagerMock.Setup(x => x.RemoveAuthenticationTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.ResetPasswordAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, rawToken, "Pass123!"), Times.Once);
        }

        #endregion

        #region ResetPasswordAsync Tests 
        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenModelIsNull()
        {
            var result = await _authenticationService.RefreshTokenAsync(null);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token model.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenOrRefreshTokenIsEmpty()
        {
            var model = new TokenModel { Token = "", RefreshToken = "" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token model.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenIsInvalidFormat()
        {
            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Throws<SecurityTokenException>();

            var model = new TokenModel { Token = "invalidToken", RefreshToken = "refresh123" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenPrincipalIsNull()
        {
            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Returns((ClaimsPrincipal?)null);

            var model = new TokenModel { Token = "validButInvalidPrincipal", RefreshToken = "refresh123" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenEmailClaimIsMissing()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity()); // no email claim
            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Returns(principal);

            var model = new TokenModel { Token = "valid", RefreshToken = "refresh123" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid token.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, "test@example.com") }
            ));

            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Returns(principal);

            _userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync((ApplicationUser?)null);

            var model = new TokenModel { Token = "valid", RefreshToken = "refresh123" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenRefreshTokenIsInvalidOrExpired()
        {
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                RefreshToken = "storedRefresh",
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddMinutes(-1)
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, user.Email) }
            ));

            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Returns(principal);

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var model = new TokenModel { Token = "valid", RefreshToken = "wrongRefresh" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid refresh token.");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenValidRefreshTokenAndJwt()
        {
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                RefreshToken = "refresh123",
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddMinutes(10)
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("remember_me", "true")
                }
            ));

            var authResponse = new ApiSuccessResponse
            {
                RefreshToken = "newRefresh123",
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddHours(1)
            };

            _jwtServiceMock.Setup(x => x.GetPrincipalFromJwtToken(It.IsAny<string>()))
                .Returns(principal);

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _jwtServiceMock.Setup(x => x.CreateJwtToken(user, true))
                .ReturnsAsync(authResponse);

            var model = new TokenModel { Token = "valid", RefreshToken = "refresh123" };

            var result = await _authenticationService.RefreshTokenAsync(model);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Token refreshed successfully.");
            result.Should().BeOfType<ApiSuccessResponse>();
            ((ApiSuccessResponse)result).RefreshToken.Should().Be("newRefresh123");
        }

        #endregion

        #region UpdateAddress Tests 
        [Fact]
        public async Task UpdateAddress_ShouldReturnFalse_WhenEmailIsNull()
        {
            var result = await _authenticationService.UpdateAddress(null, new Address());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAddress_ShouldReturnFalse_WhenAddressIsNull()
        {
            var result = await _authenticationService.UpdateAddress("test@example.com", null);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAddress_ShouldReturnFalse_WhenUserNotFound()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync("notfound@example.com"))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _authenticationService.UpdateAddress("notfound@example.com", new Address());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAddress_ShouldReturnTrue_WhenUserExistsAndAddressUpdated()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };
            var address = new Address { City = "Cairo" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(x => x.AuthenticationRepository.UpdateAddress(user.Id, address))
                .ReturnsAsync(true);

            var result = await _authenticationService.UpdateAddress(user.Email, address);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAddress_ShouldReturnFalse_WhenUserExistsButUpdateFails()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };
            var address = new Address { City = "Cairo" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(x => x.AuthenticationRepository.UpdateAddress(user.Id, address))
                .ReturnsAsync(false);

            var result = await _authenticationService.UpdateAddress(user.Email, address);

            result.Should().BeFalse();
        }



        #endregion

        #region GetAddress Tests

        [Fact]
        public async Task GetAddress_ShouldReturnNull_WhenEmailIsNull()
        {
            var result = await _authenticationService.GetAddress(null);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAddress_ShouldReturnNull_WhenEmailIsEmpty()
        {
            var result = await _authenticationService.GetAddress("");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAddress_ShouldReturnNull_WhenUserNotFound()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync("notfound@example.com"))
                .ReturnsAsync(value: (ApplicationUser?)null);

            var result = await _authenticationService.GetAddress("notfound@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAddress_ShouldReturnNull_WhenAddressNotFound()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(x => x.AuthenticationRepository.GetAddress(user.Id))
                .ReturnsAsync((Address?)null);

            var result = await _authenticationService.GetAddress(user.Email);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAddress_ShouldReturnMappedAddress_WhenAddressExists()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };
            var address = new Address { City = "Cairo", Street = "Main Street" };
            var expectedDto = new ShippingAddressDTO { City = "Cairo", Street = "Main Street" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(x => x.AuthenticationRepository.GetAddress(user.Id))
                .ReturnsAsync(address);

            _mapperMock.Setup(x => x.Map<ShippingAddressDTO>(address))
                .Returns(expectedDto);

            var result = await _authenticationService.GetAddress(user.Email);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDto);
        }


        #endregion

        #region LogoutAsync Tests
        [Fact]
        public async Task LogoutAsync_ShouldAlwaysReturnSuccessResponse()
        {
            var result = await _authenticationService.LogoutAsync("any@email.com");

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Logged out successfully.");
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task LogoutAsync_ShouldSignOutUser()
        {
            await _authenticationService.LogoutAsync("any@email.com");

            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldNotUpdateUser_WhenEmailIsNull()
        {
            await _authenticationService.LogoutAsync(null);

            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_ShouldNotUpdateUser_WhenUserNotFound()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync("missing@email.com"))
                .ReturnsAsync((ApplicationUser?)null);

            await _authenticationService.LogoutAsync("missing@email.com");

            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_ShouldClearRefreshToken_WhenUserExists()
        {
            var user = new ApplicationUser
            {
                Email = "test@email.com",
                RefreshToken = "old-refresh-token",
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddDays(1)
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            await _authenticationService.LogoutAsync(user.Email);

            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpirationDateTime.Should().Be(DateTime.MinValue);

            _userManagerMock.Verify(u => u.UpdateAsync(user), Times.Once);
        }


        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserResponse_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var expectedResponse = new ApplicationUserResponse { Id = userId, Email = "test@email.com" };

            _userManagementServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _authenticationService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _userManagementServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((ApplicationUserResponse?)null);

            // Act
            var result = await _authenticationService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldCallServiceExactlyOnce()
        {
            // Arrange
            var userId = "some-user-id";
            _userManagementServiceMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(new ApplicationUserResponse());

            // Act
            await _authenticationService.GetUserByIdAsync(userId);

            // Assert
            _userManagementServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }


        #endregion

        #region UpdateUserProfileAsync Tests

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnFailure_WhenDtoIsNull()
        {
            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(null);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Be("Invalid update data.");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var dto = new UpdateUserDTO { UserId = "not-exist" };
            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldUpdateDisplayNameAndPhoneNumber_WhenProvided()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new UpdateUserDTO
            {
                UserId = user.Id.ToString(),
                DisplayName = "New NameEn",
                PhoneNumber = "123456789"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            user.DisplayName.Should().Be("New NameEn");
            user.PhoneNumber.Should().Be("123456789");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new UpdateUserDTO { UserId = user.Id.ToString(), DisplayName = "Test" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto) as ApiErrorResponse;

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Update failed");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldChangePassword_WhenValidPasswordProvided()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new UpdateUserDTO
            {
                UserId = user.Id.ToString(),
                CurrentPassword = "oldPass",
                NewPassword = "newPass"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Profile updated successfully.");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnFailure_WhenChangePasswordFails()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new UpdateUserDTO
            {
                UserId = user.Id.ToString(),
                CurrentPassword = "oldPass",
                NewPassword = "newPass"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password change failed" }));

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto) as ApiErrorResponse;

            // Assert
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Password change failed");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldSucceed_WhenOnlyProfileUpdated_NoPasswordChange()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var dto = new UpdateUserDTO { UserId = user.Id.ToString(), DisplayName = "Updated" };

            _userManagerMock.Setup(m => m.FindByIdAsync(dto.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authenticationService.UpdateUserProfileAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Profile updated successfully.");
        }


        #endregion

        #region ExternalLoginCallbackAsync Tests

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenRemoteErrorProvided()
        {
            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync("Some error") as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>();
            var failure = result as ApiErrorResponse;
            failure!.Message.Should().Contain("Error from external login provider");
            failure.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenExternalLoginInfoIsNull()
        {
            // Arrange
            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync((ExternalLoginInfo?)null);

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>()
                  .Which.Message.Should().Be("Error loading external login information.");
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnSuccess_WhenUserAlreadyExistsWithLogin()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@example.com" };
            var info = new ExternalLoginInfo(new ClaimsPrincipal(), loginProvider: "Google", providerKey: "12345", displayName: "Google");

            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(info);

            _userManagerMock.Setup(u => u.FindByLoginAsync("Google", "12345"))
                .ReturnsAsync(user);

            _jwtServiceMock.Setup(j => j.CreateJwtToken(user, false))
                .ReturnsAsync(new ApiSuccessResponse { RefreshToken = "token123" });

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiSuccessResponse;

            // Assert
            result.Should().BeOfType<ApiSuccessResponse>()
                  .Which.Message.Should().Be("Login successful.");
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenEmailAndNameAreMissing()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var info = new ExternalLoginInfo(
                principal,
                loginProvider: "Google",
                providerKey: "123",
                displayName: "Google"
            );

            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(info);

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>()
                  .Which.Message.Should().Contain("External provider did not supply enough information");
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenEmailAlreadyRegistered()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Email, "existing@example.com")
        }));
            var info = new ExternalLoginInfo(principal, "Google", "123", "Google");

            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(info);

            _userManagerMock.Setup(u => u.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(new ApplicationUser { Email = "existing@example.com" });

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>()
                  .Which.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenUserCreationFails()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Email, "new@example.com"),
            new Claim(ClaimTypes.Name, "New User")
        }));
            var info = new ExternalLoginInfo(principal, "Google", "123", "Google");

            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(info);

            _userManagerMock.Setup(u => u.FindByEmailAsync("new@example.com"))
                .ReturnsAsync((ApplicationUser)null!);

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Create failed" }));

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>()
                  .Which.Message.Should().Be("Failed to create account from external login.");
        }

        [Fact]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailure_WhenLinkingLoginFails()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "new@example.com"),
                new Claim(ClaimTypes.Name, "New User")
            }));
            var info = new ExternalLoginInfo(principal, "Google", "123", "Google");

            _signInManagerMock.Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(info);

            _userManagerMock.Setup(u => u.FindByEmailAsync("new@example.com"))
                .ReturnsAsync((ApplicationUser)null!);

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.AddLoginAsync(It.IsAny<ApplicationUser>(), info))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Login failed" }));

            // Act
            var result = await _authenticationService.ExternalLoginCallbackAsync() as ApiErrorResponse;

            // Assert
            result.Should().BeOfType<ApiErrorResponse>()
                  .Which.Message.Should().Be("Failed to link external login.");
        }

        #endregion
    }

}
