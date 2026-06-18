using AutoMapper;
using FluentAssertions;
using HealthAxis.API.DTOs.DoctorDtos;
using HealthAxis.API.Enums;
using HealthAxis.API.Exceptions;
using HealthAxis.API.Models;
using HealthAxis.API.Repositories.Interfaces;
using HealthAxis.API.Services.Implementations;
using Moq;
using Xunit;

namespace HealthAxis.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly Mock<IMapper> _mapper;

        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _doctorRepository = new Mock<IDoctorRepository>();

            _mapper = new Mock<IMapper>();

            _service = new DoctorService(
                _doctorRepository.Object,
                _mapper.Object);
        }

        private List<Doctor> GetDoctors()
        {
            return new List<Doctor>
            {
                new Doctor
                {
                    DoctorId = 1,
                    FullName = "John Smith",
                    Specialisation = Specialisation.Cardiology,
                    YearsOfExperience = 10,
                    ConsultationFee = 500,
                    IsActive = true
                },

                new Doctor
                {
                    DoctorId = 2,
                    FullName = "Mary Jane",
                    Specialisation = Specialisation.Dermatology,
                    YearsOfExperience = 7,
                    ConsultationFee = 700,
                    IsActive = true
                }
            };
        }

        private List<DoctorDto> GetDoctorDtos()
        {
            return new List<DoctorDto>
            {
                new DoctorDto
                {
                    DoctorId = 1,
                    FullName = "John Smith",
                    Specialisation = Specialisation.Cardiology,
                    YearsOfExperience = 10,
                    ConsultationFee = 500,
                    IsActive = true
                },

                new DoctorDto
                {
                    DoctorId = 2,
                    FullName = "Mary Jane",
                    Specialisation = Specialisation.Dermatology,
                    YearsOfExperience = 7,
                    ConsultationFee = 700,
                    IsActive = true
                }
            };
        }

        //==========================================================
        // GetAllAsync()
        //==========================================================

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDoctors()
        {
            // Arrange

            var doctors = GetDoctors();

            var doctorDtos = GetDoctorDtos();

            _doctorRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctors);

            _mapper
                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            // Act

            var result = await _service.GetAllAsync();

            // Assert

            result.Should().NotBeNull();

            result.Should().HaveCount(2);

            result.First().DoctorId.Should().Be(1);

            result.First().FullName.Should().Be("John Smith");

            _doctorRepository.Verify(
                x => x.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _mapper.Verify(
                x => x.Map<IEnumerable<DoctorDto>>(doctors),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
        {
            // Arrange

            var doctors = new List<Doctor>();

            var doctorDtos = new List<DoctorDto>();

            _doctorRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctors);

            _mapper
                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            // Act

            var result = await _service.GetAllAsync();

            // Assert

            result.Should().NotBeNull();

            result.Should().BeEmpty();

            _doctorRepository.Verify(
                x => x.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _doctorRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action = async () =>
                await _service.GetAllAsync();

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var doctors = GetDoctors();

            var doctorDtos = GetDoctorDtos();

            _doctorRepository
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctors);

            _mapper
                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            // Act

            await _service.GetAllAsync();

            // Assert

            _mapper.Verify(
                x => x.Map<IEnumerable<DoctorDto>>(doctors),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var doctors = GetDoctors();

            var doctorDtos = GetDoctorDtos();

            _doctorRepository
                .Setup(x => x.GetAllAsync(token))
                .ReturnsAsync(doctors);

            _mapper
                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
                .Returns(doctorDtos);

            // Act

            await _service.GetAllAsync(token);

            // Assert

            _doctorRepository.Verify(
                x => x.GetAllAsync(token),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDoctor_WhenDoctorExists()
        {
            // Arrange

            int doctorId = 1;

            var doctor = GetDoctors().First();

            var doctorDto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(doctorDto);

            // Act

            var result = await _service.GetByIdAsync(doctorId);

            // Assert

            result.Should().NotBeNull();

            result!.DoctorId.Should().Be(1);

            result.FullName.Should().Be("John Smith");

            result.Specialisation.Should().Be(Specialisation.Cardiology);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
        {
            // Arrange

            _doctorRepository
                .Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            // Act

            Func<Task> action = async () =>
                await _service.GetByIdAsync(100);

            // Assert

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Doctor not found");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _doctorRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(),
                                           It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action = async () =>
                await _service.GetByIdAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldCallRepositoryOnce()
        {
            // Arrange

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetByIdAsync(1);

            // Assert

            _doctorRepository.Verify(
                x => x.GetByIdAsync(1,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetByIdAsync(1);

            // Assert

            _mapper.Verify(
                x => x.Map<DoctorDto>(doctor),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(1, token))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetByIdAsync(1, token);

            // Assert

            _doctorRepository.Verify(
                x => x.GetByIdAsync(1, token),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldReturnDoctor_WhenDoctorIsAvailable()
        {
            // Arrange

            int doctorId = 1;

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            var result = await _service.GetAvailableDoctorByIdAsync(doctorId);

            // Assert

            result.Should().NotBeNull();

            result.DoctorId.Should().Be(1);

            result.FullName.Should().Be("John Smith");
        }

     

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act

            Func<Task> action = async () =>
                await _service.GetAvailableDoctorByIdAsync(1);

            // Assert

            await action.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database Error");
        }

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldCallRepositoryOnce()
        {
            // Arrange

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetAvailableDoctorByIdAsync(1);

            // Assert

            _doctorRepository.Verify(
                x => x.GetAvailableDoctorByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldCallMapperOnce()
        {
            // Arrange

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetAvailableDoctorByIdAsync(1);

            // Assert

            _mapper.Verify(
                x => x.Map<DoctorDto>(doctor),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldPassCancellationToken()
        {
            // Arrange

            var token = new CancellationToken();

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(1, token))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            await _service.GetAvailableDoctorByIdAsync(1, token);

            // Assert

            _doctorRepository.Verify(
                x => x.GetAvailableDoctorByIdAsync(1, token),
                Times.Once);
        }

        [Fact]
        public async Task GetAvailableDoctorByIdAsync_ShouldReturnMappedDoctorDto()
        {
            // Arrange

            var doctor = GetDoctors().First();

            var dto = GetDoctorDtos().First();

            _doctorRepository
                .Setup(x => x.GetAvailableDoctorByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _mapper
                .Setup(x => x.Map<DoctorDto>(doctor))
                .Returns(dto);

            // Act

            var result = await _service.GetAvailableDoctorByIdAsync(1);

            // Assert

            result.FullName.Should().Be(dto.FullName);

            result.Specialisation.Should().Be(dto.Specialisation);

            result.YearsOfExperience.Should().Be(dto.YearsOfExperience);

            result.ConsultationFee.Should().Be(dto.ConsultationFee);

            result.IsActive.Should().BeTrue();
        }
    }
}