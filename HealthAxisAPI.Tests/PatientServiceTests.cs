//using AutoMapper;
//using FluentAssertions;
//using HealthAxis.Shared.DTOs.PatientDtos;
//using HealthAxis.Shared.Enums;
//using HealthAxis.API.Exceptions;
//using HealthAxis.API.Models;
//using HealthAxis.API.Repositories.Interfaces;
//using HealthAxis.API.Services.Implementations;
//using Moq;
//using System.Reflection;
//using Xunit;

//namespace HealthAxis.Tests.Services
//{
//    public class PatientServiceTests
//    {
//        private readonly Mock<IPatientRepository> _patientRepository;
//        private readonly Mock<IRepository<Appointment>> _appointmentRepository;
//        private readonly Mock<IRepository<HealthRecord>> _healthRecordRepository;
//        private readonly Mock<IMapper> _mapper;

//        private readonly PatientService _service;

//        public PatientServiceTests()
//        {
//            _patientRepository = new Mock<IPatientRepository>();

//            _appointmentRepository =
//                new Mock<IRepository<Appointment>>();

//            _healthRecordRepository =
//                new Mock<IRepository<HealthRecord>>();

//            _mapper = new Mock<IMapper>();

//            _service = new PatientService(
//                _patientRepository.Object,
//                _appointmentRepository.Object,
//                _healthRecordRepository.Object,
//                _mapper.Object);
//        }

//        private List<Patient> GetPatients()
//        {
//            return new List<Patient>
//            {
//                new Patient
//                {
//                    PatientId = 1,
//                    FullName = "John",
//                    Email = "john@test.com",
//                    PhoneNumber = "9876543210",
//                    Gender = Gender.Male,
//                    DateOfBirth = new DateTime(2000,1,1)
//                },

//                new Patient
//                {
//                    PatientId = 2,
//                    FullName = "Mary",
//                    Email = "mary@test.com",
//                    PhoneNumber = "9999999999",
//                    Gender = Gender.Female,
//                    DateOfBirth = new DateTime(1999,5,5)
//                }
//            };
//        }

//        private List<PatientDto> GetPatientDtos()
//        {
//            return new List<PatientDto>
//            {
//                new PatientDto
//                {
//                    PatientId = 1,
//                    FullName = "John",
//                    Email = "john@test.com"
//                },

//                new PatientDto
//                {
//                    PatientId = 2,
//                    FullName = "Mary",
//                    Email = "mary@test.com"
//                }
//            };
//        }

//        [Fact]
//        public async Task GetAllAsync_ShouldReturnAllPatients()
//        {
//            // Arrange

//            var patients = GetPatients();

//            var patientDtos = GetPatientDtos();

//            _patientRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patients);

//            _mapper
//                .Setup(x =>
//                    x.Map<IEnumerable<PatientDto>>(patients))
//                .Returns(patientDtos);

//            // Act

//            var result = await _service.GetAllAsync();

//            // Assert

//            result.Should().NotBeNull();

//            result.Should().HaveCount(2);

//            result.First().FullName.Should().Be("John");

//            _patientRepository.Verify(
//                x => x.GetAllAsync(It.IsAny<CancellationToken>()),
//                Times.Once);

//            _mapper.Verify(
//                x => x.Map<IEnumerable<PatientDto>>(patients),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPatientsExist()
//        {
//            // Arrange

//            var patients = new List<Patient>();

//            var dtos = new List<PatientDto>();

//            _patientRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patients);

//            _mapper
//                .Setup(x =>
//                    x.Map<IEnumerable<PatientDto>>(patients))
//                .Returns(dtos);

//            // Act

//            var result = await _service.GetAllAsync();

//            // Assert

//            result.Should().BeEmpty();

//            _patientRepository.Verify(
//                x => x.GetAllAsync(It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryFails()
//        {
//            // Arrange

//            _patientRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            // Act

//            Func<Task> action = async () =>
//                await _service.GetAllAsync();

//            // Assert

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task GetAllAsync_ShouldCallMapperOnce()
//        {
//            // Arrange

//            var patients = GetPatients();

//            var dtos = GetPatientDtos();

//            _patientRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patients);

//            _mapper
//                .Setup(x =>
//                    x.Map<IEnumerable<PatientDto>>(patients))
//                .Returns(dtos);

//            // Act

//            await _service.GetAllAsync();

//            // Assert

//            _mapper.Verify(
//                x => x.Map<IEnumerable<PatientDto>>(patients),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldReturnPatient_WhenPatientExists()
//        {
//            // Arrange

//            int patientId = 1;

//            var patient = GetPatients().First();

//            var patientDto = GetPatientDtos().First();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(patientDto);

//            // Act

//            var result = await _service.GetByIdAsync(patientId);

//            // Assert

//            result.Should().NotBeNull();

//            result!.PatientId.Should().Be(1);

//            result.FullName.Should().Be("John");

//            result.Email.Should().Be("john@test.com");
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenPatientDoesNotExist()
//        {
//            // Arrange

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Patient?)null);

//            // Act

//            Func<Task> action = async () =>
//                await _service.GetByIdAsync(100);

//            // Assert

//            await action.Should()
//                .ThrowAsync<NotFoundException>()
//                .WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldThrowException_WhenRepositoryFails()
//        {
//            // Arrange

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(It.IsAny<int>(),
//                                           It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            // Act

//            Func<Task> action = async () =>
//                await _service.GetByIdAsync(1);

//            // Assert

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldCallRepositoryOnce()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = GetPatientDtos().First();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(dto);

//            // Act

//            await _service.GetByIdAsync(1);

//            // Assert

//            _patientRepository.Verify(
//                x => x.GetByIdAsync(1,
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }
//        [Fact]
//        public async Task GetByIdAsync_ShouldCallMapperOnce()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = GetPatientDtos().First();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(dto);

//            // Act

//            await _service.GetByIdAsync(1);

//            // Assert

//            _mapper.Verify(
//                x => x.Map<PatientDto>(patient),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetByIdAsync_ShouldPassCancellationToken()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = GetPatientDtos().First();

//            var token = new CancellationToken();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1, token))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(dto);

//            // Act

//            await _service.GetByIdAsync(1, token);

//            // Assert

//            _patientRepository.Verify(
//                x => x.GetByIdAsync(1, token),
//                Times.Once);
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldUpdatePatient_WhenPatientExists()
//        {
//            // Arrange

//            int patientId = 1;

//            var patient = GetPatients().First();

//            var updateDto = new UpdatePatientDto
//            {
//                FullName = "John Updated",
//                Email = "updated@test.com"
//            };

//            var updatedPatient = patient;

//            updatedPatient.FullName = updateDto.FullName!;
//            updatedPatient.Email = updateDto.Email!;

//            var patientDto = new PatientDto
//            {
//                PatientId = updatedPatient.PatientId,
//                FullName = updatedPatient.FullName,
//                Email = updatedPatient.Email
//            };

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _patientRepository
//                .Setup(x => x.UpdateAsync(patientId,
//                                          It.IsAny<Patient>(),
//                                          It.IsAny<CancellationToken>()))
//                .ReturnsAsync(updatedPatient);

//            _mapper
//                .Setup(x => x.Map(updateDto, patient));

//            _mapper
//                .Setup(x => x.Map<PatientDto>(updatedPatient))
//                .Returns(patientDto);

//            // Act

//            var result = await _service.UpdateAsync(patientId, updateDto);

//            // Assert

//            result.Should().NotBeNull();

//            result.FullName.Should().Be("John Updated");

//            result.Email.Should().Be("updated@test.com");
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldThrowNotFoundException_WhenPatientDoesNotExist()
//        {
//            // Arrange

//            var dto = new UpdatePatientDto();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(100,
//                                           It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Patient?)null);

//            // Act

//            Func<Task> action = async () =>
//                await _service.UpdateAsync(100, dto);

//            // Assert

//            await action.Should()
//                .ThrowAsync<NotFoundException>()
//                .WithMessage("Patient not found.");
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldCallMapper()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = new UpdatePatientDto
//            {
//                FullName = "Updated"
//            };

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1,
//                                           It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _patientRepository
//                .Setup(x => x.UpdateAsync(1,
//                                          patient,
//                                          It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map(dto, patient));

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(GetPatientDtos().First());

//            // Act

//            await _service.UpdateAsync(1, dto);

//            // Assert

//            _mapper.Verify(
//                x => x.Map(dto, patient),
//                Times.Once);
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldCallRepositoryUpdateOnce()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = new UpdatePatientDto();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1,
//                                           It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _patientRepository
//                .Setup(x => x.UpdateAsync(1,
//                                          patient,
//                                          It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map(dto, patient));

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(GetPatientDtos().First());

//            // Act

//            await _service.UpdateAsync(1, dto);

//            // Assert

//            _patientRepository.Verify(
//                x => x.UpdateAsync(1,
//                                   patient,
//                                   It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldThrowException_WhenRepositoryFails()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = new UpdatePatientDto();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1,
//                                           It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper
//                .Setup(x => x.Map(dto, patient));

//            _patientRepository
//                .Setup(x => x.UpdateAsync(1,
//                                          patient,
//                                          It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            // Act

//            Func<Task> action = async () =>
//                await _service.UpdateAsync(1, dto);

//            // Assert

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldAllowEmptyDto()
//        {
//            // Arrange

//            var patient = GetPatients().First();

//            var dto = new UpdatePatientDto();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1,
//                                           It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _patientRepository
//                .Setup(x => x.UpdateAsync(1,
//                                          patient,
//                                          It.IsAny<CancellationToken>()))
//                .ReturnsAsync(patient);

//            _mapper.Setup(x => x.Map(dto, patient));

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(GetPatientDtos().First());

//            // Act

//            var result = await _service.UpdateAsync(1, dto);

//            // Assert

//            result.Should().NotBeNull();
//        }

//        [Fact]
//        public async Task UpdateAsync_ShouldPassCancellationToken()
//        {
//            // Arrange

//            var token = new CancellationToken();

//            var patient = GetPatients().First();

//            var dto = new UpdatePatientDto();

//            _patientRepository
//                .Setup(x => x.GetByIdAsync(1, token))
//                .ReturnsAsync(patient);

//            _patientRepository
//                .Setup(x => x.UpdateAsync(1,
//                                          patient,
//                                          token))
//                .ReturnsAsync(patient);

//            _mapper.Setup(x => x.Map(dto, patient));

//            _mapper
//                .Setup(x => x.Map<PatientDto>(patient))
//                .Returns(GetPatientDtos().First());

//            // Act

//            await _service.UpdateAsync(1, dto, token);

//            // Assert

//            _patientRepository.Verify(
//                x => x.GetByIdAsync(1, token),
//                Times.Once);

//            _patientRepository.Verify(
//                x => x.UpdateAsync(1,
//                                   patient,
//                                   token),
//                Times.Once);
//        }

//    }
//}
