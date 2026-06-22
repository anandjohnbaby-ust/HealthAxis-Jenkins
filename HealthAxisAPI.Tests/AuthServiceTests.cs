//using FluentAssertions;
//using HealthAxis.API.DTOs.AuthDtos;
//using HealthAxis.API.Models;
//using HealthAxis.API.Services.Implementation;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using Xunit;

//namespace HealthAxis.Tests.Services
//{
//    public class AuthServiceTests
//    {
//        private readonly Mock<UserManager<ApplicationUser>> _userManager;
//        private readonly Mock<IConfiguration> _configuration;

//        private readonly AuthService _service;

//        //public AuthServiceTests()
//        //{
//        //    var store =
//        //        new Mock<IUserStore<ApplicationUser>>();

//        //    _userManager =
//        //        new Mock<UserManager<ApplicationUser>>(
//        //            store.Object,
//        //            null!,
//        //            null!,
//        //            null!,
//        //            null!,
//        //            null!,
//        //            null!,
//        //            null!,
//        //            null!);

//        //    _configuration =
//        //        new Mock<IConfiguration>();

//        //    _configuration
//        //        .Setup(x => x["Jwt:AccessTokenExpirationMinutes"])
//        //        .Returns("15");

//        //    _configuration
//        //        .Setup(x => x["Jwt:Issuer"])
//        //        .Returns("HealthAxis.API");

//        //    _configuration
//        //        .Setup(x => x["Jwt:Audience"])
//        //        .Returns("HealthAxis.API");

//        //    _configuration
//        //        .Setup(x => x["Jwt:Key"])
//        //        .Returns("78acd5d93d025413e160ebc47dd1c2ead48aee30e3a4080b71c49c9221c97958");

//        //    _service =
//        //        new AuthService(
//        //            _userManager.Object,
//        //            _configuration.Object);
//        //}

//        //private RegisterDto GetRegisterDto()
//        //{
//        //    return new RegisterDto
//        //    {
//        //        Email = "john@test.com",
//        //        Password = "Password@123",
//        //        ConfirmPassword = "Password@123",
//        //        Role = "Patient"
//        //    };
//        //}

//        private ApplicationUser GetUser()
//        {
//            return new ApplicationUser
//            {
//                Id = "1",
//                Email = "john@test.com",
//                UserName = "john@test.com"
//            };
//        }


//        //[Fact]
//        //public async Task Register_ShouldFail_WhenPasswordsDoNotMatch()
//        //{
//        //    var dto = GetRegisterDto();

//        //    dto.ConfirmPassword = "WrongPassword";

//        //    var result =
//        //        await _service.Register(dto);

//        //    result.Success.Should().BeFalse();

//        //    result.Message.Should()
//        //        .Be("Password Do not Match");
//        //}

//        //[Fact]
//        //public async Task Register_ShouldReturnIdentityErrors()
//        //{
//        //    var dto = GetRegisterDto();

//        //    var errors = new IdentityError[]
//        //    {
//        //new IdentityError
//        //{
//        //    Description = "Email already exists"
//        //}
//        //    };

//        //    _userManager
//        //        .Setup(x => x.CreateAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Password))
//        //        .ReturnsAsync(
//        //            IdentityResult.Failed(errors));

//        //    var result =
//        //        await _service.Register(dto);

//        //    result.Success.Should().BeFalse();

//        //    result.Message.Should()
//        //        .Contain("Email already exists");
//        //}

//        //[Fact]
//        //public async Task Register_ShouldCallCreateAsyncOnce()
//        //{
//        //    var dto = GetRegisterDto();

//        //    _userManager
//        //        .Setup(x => x.CreateAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Password))
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    _userManager
//        //        .Setup(x => x.AddToRoleAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Role))
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    await _service.Register(dto);

//        //    _userManager.Verify(
//        //        x => x.CreateAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Password),
//        //        Times.Once);
//        //}

//        //[Fact]
//        //public async Task Register_ShouldAddUserToRole()
//        //{
//        //    var dto = GetRegisterDto();

//        //    _userManager
//        //        .Setup(x => x.CreateAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Password))
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    _userManager
//        //        .Setup(x => x.AddToRoleAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Role))
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    await _service.Register(dto);

//        //    _userManager.Verify(
//        //        x => x.AddToRoleAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Role),
//        //        Times.Once);
//        //}

//        //[Fact]
//        //public async Task Register_ShouldCreateUserWithCorrectEmail()
//        //{
//        //    // Arrange

//        //    var dto = GetRegisterDto();

//        //    ApplicationUser? createdUser = null;

//        //    _userManager
//        //        .Setup(x => x.CreateAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Password))
//        //        .Callback<ApplicationUser, string>((user, _) =>
//        //        {
//        //            createdUser = user;
//        //        })
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    _userManager
//        //        .Setup(x => x.AddToRoleAsync(
//        //            It.IsAny<ApplicationUser>(),
//        //            dto.Role))
//        //        .ReturnsAsync(IdentityResult.Success);

//        //    // Act

//        //    await _service.Register(dto);

//        //    // Assert

//        //    createdUser.Should().NotBeNull();

//        //    createdUser!.Email.Should().Be(dto.Email);

//        //    createdUser.UserName.Should().Be(dto.Email);
//        //}

//        [Fact]
//        public async Task Login_ShouldFail_WhenUserDoesNotExist()
//        {
//            var dto = new LoginDto
//            {
//                Email = "unknown@test.com",
//                Password = "Password@123"
//            };

//            _userManager
//                .Setup(x => x.FindByEmailAsync(dto.Email))
//                .ReturnsAsync((ApplicationUser?)null);

//            var result = await _service.Login(dto);

//            result.Success.Should().BeFalse();

//            result.Message.Should().Be("Invalid Credentials");

//            result.AccessToken.Should().BeEmpty();

//            result.RefreshToken.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task Login_ShouldFail_WhenPasswordIsWrong()
//        {
//            var dto = new LoginDto
//            {
//                Email = "john@test.com",
//                Password = "WrongPassword"
//            };

//            var user = GetUser();

//            _userManager
//                .Setup(x => x.FindByEmailAsync(dto.Email))
//                .ReturnsAsync(user);

//            _userManager
//                .Setup(x => x.CheckPasswordAsync(user, dto.Password))
//                .ReturnsAsync(false);

//            var result = await _service.Login(dto);

//            result.Success.Should().BeFalse();

//            result.Message.Should().Be("Invalid Credentials");
//        }
//    }
//}