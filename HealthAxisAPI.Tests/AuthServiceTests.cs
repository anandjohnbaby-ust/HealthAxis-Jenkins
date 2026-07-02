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
                store.Object, null, null, null, null, null, null, null, null);

            _context = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            _patientRepository = new Mock<IPatientRepository>();
            _doctorRepository = new Mock<IDoctorRepository>();

            var inMemorySettings = new Dictionary<string, string>
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
