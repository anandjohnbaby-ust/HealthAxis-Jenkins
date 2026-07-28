using FluentAssertions;
using HealthAxis.API.Data;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Services.Implementation;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.Shared.DTOs.AuthDtos;
using HealthAxis.Shared.DTOs.CommonDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<ApplicationDbContext> _context;
        private readonly Mock<IPatientRepository> _patientRepository;
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly IConfiguration _configuration;

        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _context = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            _patientRepository = new Mock<IPatientRepository>();
            _doctorRepository = new Mock<IDoctorRepository>();

            var inMemorySettings = new Dictionary<string, string?>
    {
        { "Jwt:Key", "supersecretkey1234567890" },
        { "Jwt:Issuer", "test" },
        { "Jwt:Audience", "test" },
        { "Jwt:AccessTokenExpirationMinutes", "60" },
        { "Jwt:RefreshTokenExpirationDays", "7" }
    };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new AuthService(
                _userManager.Object,
                _context.Object,
                _patientRepository.Object,
                _doctorRepository.Object,
                _configuration);
        }
        // Register-related tests need a real DbContext (transactions + DbSet.Add
        // aren't practical to mock), so they get their own EF Core InMemory
        // context and a fresh AuthService instance built from it.
        private (AuthService Service, ApplicationDbContext Context) CreateServiceWithInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var service = new AuthService(
                _userManager.Object,
                context,
                _patientRepository.Object,
                _doctorRepository.Object,
                _configuration);

            return (service, context);
        }

        // =====================================================================
        // Login
        // =====================================================================

        [Fact]
        public async Task Login_ShouldReturnInvalid_WhenUserNotFound()
        {
            _userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = "x@x.com", Password = "p" });

            success.Should().BeFalse();
            message.Should().Be("Invalid Credentials");
        }

        [Fact]
        public async Task Login_ShouldReturnInvalid_WhenPasswordIncorrect()
        {
            var user = new ApplicationUser { Id = "u1", Email = "x@x.com" };
            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "wrong" });

            success.Should().BeFalse();
            message.Should().Be("Invalid Credentials");
        }

        [Fact]
        public async Task Login_ShouldSucceed_WhenNoRoles()
        {
            var user = new ApplicationUser { Id = "u2", Email = "plain@x.com" };

            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "p")).ReturnsAsync(true);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "p" });

            success.Should().BeTrue();
            message.Should().Be("Login Successful");
            at.Should().NotBeNullOrEmpty();
            rt.Should().NotBeNullOrEmpty();
            exp.Should().Be(60);

            user.RefreshToken.Should().Be(rt);
            user.RefreshTokenExpiryTime.Should().BeAfter(DateTime.UtcNow);

            _userManager.Verify(u => u.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldIncludePatientClaims_WhenUserIsPatient()
        {
            var user = new ApplicationUser { Id = "u3", Email = "patient@x.com" };
            var patient = new Patient { PatientId = 55, UserId = user.Id, FullName = "Pat Ient" };

            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "p")).ReturnsAsync(true);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Patient" });
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _patientRepository.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync(patient);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "p" });

            success.Should().BeTrue();
            at.Should().NotBeNullOrEmpty();
            _patientRepository.Verify(r => r.GetByUserIdAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldSkipPatientClaims_WhenPatientRecordMissing()
        {
            var user = new ApplicationUser { Id = "u4", Email = "orphan@x.com" };

            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "p")).ReturnsAsync(true);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Patient" });
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _patientRepository.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync((Patient?)null);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "p" });

            success.Should().BeTrue();
            at.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldIncludeDoctorClaims_WhenUserIsDoctor()
        {
            var user = new ApplicationUser { Id = "u5", Email = "doc@x.com" };
            var doctor = new Doctor { DoctorId = 77, UserId = user.Id, FullName = "Doc Tor" };

            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "p")).ReturnsAsync(true);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Doctor" });
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _doctorRepository.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync(doctor);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "p" });

            success.Should().BeTrue();
            at.Should().NotBeNullOrEmpty();
            _doctorRepository.Verify(r => r.GetByUserIdAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldSkipDoctorClaims_WhenDoctorRecordMissing()
        {
            var user = new ApplicationUser { Id = "u6", Email = "orphandoc@x.com" };

            _userManager.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManager.Setup(u => u.CheckPasswordAsync(user, "p")).ReturnsAsync(true);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Doctor" });
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _doctorRepository.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);

            var (success, message, at, rt, exp) = await _service.Login(new LoginDto { Email = user.Email, Password = "p" });

            success.Should().BeTrue();
            at.Should().NotBeNullOrEmpty();
        }

        // =====================================================================
        // Register
        // =====================================================================

        [Fact]
        public async Task Register_ShouldReturnFailure_WhenPasswordsDoNotMatch()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            var dto = new RegisterDto
            {
                Email = "a@a.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Different1!",
                FullName = "A A",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                PhoneNumber = "123"
            };

            var (success, message, userId) = await service.Register(dto);

            success.Should().BeFalse();
            message.Should().Be("Password and Confirm Password do not match.");
            userId.Should().BeEmpty();

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Register_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            var existing = new ApplicationUser { Id = "existing", Email = "dup@x.com" };
            _userManager.Setup(u => u.FindByEmailAsync("dup@x.com")).ReturnsAsync(existing);

            var dto = new RegisterDto
            {
                Email = "dup@x.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!",
                FullName = "B B",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                PhoneNumber = "123"
            };

            var (success, message, userId) = await service.Register(dto);

            success.Should().BeFalse();
            message.Should().Be("Email already exists.");

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Register_ShouldReturnFailure_WhenUserCreationFails()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("bad@x.com")).ReturnsAsync((ApplicationUser?)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

            var dto = new RegisterDto
            {
                Email = "bad@x.com",
                Password = "weak",
                ConfirmPassword = "weak",
                FullName = "C C",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                PhoneNumber = "123"
            };

            var (success, message, userId) = await service.Register(dto);

            success.Should().BeFalse();
            message.Should().Be("Weak password");
            userId.Should().BeEmpty();
            context.Patients.Should().BeEmpty();

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Register_ShouldSucceed_WhenValid()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser?)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) => u.Id = "new-user-id");
            _userManager
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var dto = new RegisterDto
            {
                Email = "new@x.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!",
                FullName = "D D",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                PhoneNumber = "456"
            };

            var (success, message, userId) = await service.Register(dto);

            success.Should().BeTrue();
            message.Should().Be("Patient registered successfully.");
            userId.Should().Be("new-user-id");

            context.Patients.Should().ContainSingle(p => p.Email == "new@x.com" && p.FullName == "D D");
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"), Times.Once);

            await context.DisposeAsync();
        }

        [Fact]
        public async Task Register_ShouldRollbackAndRethrow_WhenExceptionOccursMidTransaction()
        {
            var (service, context) = CreateServiceWithInMemoryContext();

            _userManager.Setup(u => u.FindByEmailAsync("crash@x.com")).ReturnsAsync((ApplicationUser?)null);
            _userManager
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ThrowsAsync(new Exception("role assignment failed"));

            var dto = new RegisterDto
            {
                Email = "crash@x.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!",
                FullName = "E E",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                PhoneNumber = "789"
            };

            Func<Task> action = async () => await service.Register(dto);

            await action.Should().ThrowAsync<Exception>().WithMessage("role assignment failed");
            context.Patients.Should().BeEmpty();

            await context.DisposeAsync();
        }

        // =====================================================================
        // RefreshToken
        // =====================================================================

        [Fact]
        public async Task RefreshToken_ShouldReturnInvalid_WhenTokenNotFound()
        {
            _userManager.Setup(u => u.Users).Returns(new List<ApplicationUser>().AsQueryable());

            var (success, message, at, rt, exp) = await _service.RefreshToken(new RefreshTokenDto { RefreshToken = "missing" });

            success.Should().BeFalse();
            message.Should().Be("Invalid Refresh Token.");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnExpired_WhenTokenExpired()
        {
            var user = new ApplicationUser
            {
                Id = "u7",
                Email = "expired@x.com",
                RefreshToken = "expiredtoken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            _userManager.Setup(u => u.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());

            var (success, message, at, rt, exp) = await _service.RefreshToken(new RefreshTokenDto { RefreshToken = "expiredtoken" });

            success.Should().BeFalse();
            message.Should().Be("Refresh Token has expired.");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnFailure_WhenUpdateFails()
        {
            var user = new ApplicationUser
            {
                Id = "u8",
                Email = "updatefail@x.com",
                RefreshToken = "validtoken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            _userManager.Setup(u => u.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManager
                .Setup(u => u.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "update failed" }));

            var (success, message, at, rt, exp) = await _service.RefreshToken(new RefreshTokenDto { RefreshToken = "validtoken" });

            success.Should().BeFalse();
            message.Should().Be("Unable to update refresh token.");
        }

        [Fact]
        public async Task RefreshToken_ShouldSucceed_WhenTokenValid()
        {
            var user = new ApplicationUser
            {
                Id = "u9",
                Email = "valid@x.com",
                RefreshToken = "goodtoken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            _userManager.Setup(u => u.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var (success, message, at, rt, exp) = await _service.RefreshToken(new RefreshTokenDto { RefreshToken = "goodtoken" });

            success.Should().BeTrue();
            message.Should().Be("Token refreshed successfully.");
            at.Should().NotBeNullOrEmpty();
            rt.Should().NotBeNullOrEmpty();
            rt.Should().NotBe("goodtoken");
            exp.Should().Be(60);
        }

        // =====================================================================
        // ChangePasswordAsync
        // =====================================================================

        [Fact]
        public async Task ChangePassword_ShouldThrowNotFound_WhenUserMissing()
        {
            _userManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            Func<Task> action = async () => await _service.ChangePasswordAsync("x", new ChangePasswordDto());

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("User not found.");
        }

        [Fact]
        public async Task ChangePassword_ShouldThrowValidation_WhenChangeFails()
        {
            var user = new ApplicationUser { Id = "u3" };
            _userManager.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(u => u.ChangePasswordAsync(user, "c", "n")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "err" }));

            Func<Task> action = async () => await _service.ChangePasswordAsync(user.Id, new ChangePasswordDto { CurrentPassword = "c", NewPassword = "n" });

            await action.Should().ThrowAsync<ValidationException>().WithMessage("err");
        }

        [Fact]
        public async Task ChangePassword_ShouldSucceed_WhenChangeSucceeds()
        {
            var user = new ApplicationUser { Id = "u4" };
            _userManager.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManager.Setup(u => u.ChangePasswordAsync(user, "c", "n")).ReturnsAsync(IdentityResult.Success);

            await _service.ChangePasswordAsync(user.Id, new ChangePasswordDto { CurrentPassword = "c", NewPassword = "n" });

            _userManager.Verify(u => u.ChangePasswordAsync(user, "c", "n"), Times.Once);
        }

    }
}




