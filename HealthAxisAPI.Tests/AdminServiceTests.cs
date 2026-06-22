//using AutoMapper;
//using FluentAssertions;
//using HealthAxis.Shared.DTOs.AppointmentDtos;
//using HealthAxis.Shared.DTOs.DoctorDtos;
//using HealthAxis.API.Exceptions;
//using HealthAxis.API.Models;
//using HealthAxis.API.Repositories.Interfaces;
//using HealthAxis.API.Services.Implementations;
//using Moq;
//using Xunit;

//namespace HealthAxis.Tests.Services
//{
//    public class AdminServiceTests
//    {
//        private readonly Mock<IDoctorRepository> _doctorRepository;
//        private readonly Mock<IAppointmentRepository> _appointmentRepository;
//        private readonly Mock<IMapper> _mapper;

//        private readonly AdminService _service;

//        //public AdminServiceTests()
//        //{
//        //    _doctorRepository =
//        //        new Mock<IDoctorRepository>();

//        //    _appointmentRepository =
//        //        new Mock<IAppointmentRepository>();

//        //    _mapper =
//        //        new Mock<IMapper>();

//        //    _service =
//        //        new AdminService(
//        //            _doctorRepository.Object,
//        //            _appointmentRepository.Object,
//        //            _mapper.Object);
//        //}

//        private List<Doctor> GetDoctors()
//        {
//            return new List<Doctor>
//            {
//                new Doctor
//                {
//                    DoctorId = 1,
//                    FullName = "John",
//                    ConsultationFee = 500,
//                    IsActive = true
//                },

//                new Doctor
//                {
//                    DoctorId = 2,
//                    FullName = "Mary",
//                    ConsultationFee = 600,
//                    IsActive = true
//                }
//            };
//        }

//        private List<DoctorDto> GetDoctorDtos()
//        {
//            return new List<DoctorDto>
//            {
//                new DoctorDto
//                {
//                    DoctorId = 1,
//                    FullName = "John",
//                    ConsultationFee = 500
//                },

//                new DoctorDto
//                {
//                    DoctorId = 2,
//                    FullName = "Mary",
//                    ConsultationFee = 600
//                }
//            };
//        }

//        private CreateDoctorDto GetCreateDoctorDto()
//        {
//            return new CreateDoctorDto
//            {
//                FullName = "New Doctor",
//                ConsultationFee = 700,
//                YearsOfExperience = 5
//            };
//        }

//        [Fact]
//        public async Task GetDoctors_ShouldReturnDoctors()
//        {
//            var doctors = GetDoctors();

//            var dtos = GetDoctorDtos();

//            _doctorRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctors);

//            _mapper
//                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
//                .Returns(dtos);

//            var result = await _service.GetDoctors();

//            result.Should().HaveCount(2);

//            result.First().DoctorId.Should().Be(1);
//        }

//        [Fact]
//        public async Task GetDoctors_ShouldReturnEmptyCollection()
//        {
//            var doctors = new List<Doctor>();

//            var dtos = new List<DoctorDto>();

//            _doctorRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctors);

//            _mapper
//                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
//                .Returns(dtos);

//            var result = await _service.GetDoctors();

//            result.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task GetDoctors_ShouldThrowException_WhenRepositoryFails()
//        {
//            _doctorRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            Func<Task> action =
//                async () => await _service.GetDoctors();

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task GetDoctors_ShouldCallMapperOnce()
//        {
//            var doctors = GetDoctors();

//            var dtos = GetDoctorDtos();

//            _doctorRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctors);

//            _mapper
//                .Setup(x => x.Map<IEnumerable<DoctorDto>>(doctors))
//                .Returns(dtos);

//            await _service.GetDoctors();

//            _mapper.Verify(
//                x => x.Map<IEnumerable<DoctorDto>>(doctors),
//                Times.Once);
//        }

//        [Fact]
//        public async Task CreateDoctor_ShouldCreateDoctor()
//        {
//            var dto = GetCreateDoctorDto();

//            var doctor = new Doctor();

//            var response = new DoctorDto
//            {
//                DoctorId = 1
//            };

//            _mapper
//                .Setup(x => x.Map<Doctor>(dto))
//                .Returns(doctor);

//            _doctorRepository
//                .Setup(x => x.AddAsync(doctor,
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper
//                .Setup(x => x.Map<DoctorDto>(doctor))
//                .Returns(response);

//            var result =
//                await _service.CreateDoctor(dto);

//            result.DoctorId.Should().Be(1);
//        }

//        [Fact]
//        public async Task CreateDoctor_ShouldCallRepositoryOnce()
//        {
//            var dto = GetCreateDoctorDto();

//            var doctor = new Doctor();

//            _mapper
//                .Setup(x => x.Map<Doctor>(dto))
//                .Returns(doctor);

//            _doctorRepository
//                .Setup(x => x.AddAsync(doctor,
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper
//                .Setup(x => x.Map<DoctorDto>(doctor))
//                .Returns(new DoctorDto());

//            await _service.CreateDoctor(dto);

//            _doctorRepository.Verify(
//                x => x.AddAsync(doctor,
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task CreateDoctor_ShouldCallMapperTwice()
//        {
//            var dto = GetCreateDoctorDto();

//            var doctor = new Doctor();

//            _mapper
//                .Setup(x => x.Map<Doctor>(dto))
//                .Returns(doctor);

//            _doctorRepository
//                .Setup(x => x.AddAsync(doctor,
//                    It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper
//                .Setup(x => x.Map<DoctorDto>(doctor))
//                .Returns(new DoctorDto());

//            await _service.CreateDoctor(dto);

//            _mapper.Verify(
//                x => x.Map<Doctor>(dto),
//                Times.Once);

//            _mapper.Verify(
//                x => x.Map<DoctorDto>(doctor),
//                Times.Once);
//        }

//        [Fact]
//        public async Task CreateDoctor_ShouldThrowException_WhenRepositoryFails()
//        {
//            var dto = GetCreateDoctorDto();

//            var doctor = new Doctor();

//            _mapper
//                .Setup(x => x.Map<Doctor>(dto))
//                .Returns(doctor);

//            _doctorRepository
//                .Setup(x => x.AddAsync(doctor,
//                    It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            Func<Task> action =
//                async () => await _service.CreateDoctor(dto);

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task UpdateDoctor_ShouldThrowNotFoundException_WhenDoctorDoesNotExist()
//        {
//            // Arrange

//            var dto = new UpdateDoctorDto();

//            _doctorRepository
//                .Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Doctor?)null);

//            // Act

//            Func<Task> action =
//                async () => await _service.UpdateDoctor(100, dto);

//            // Assert

//            await action.Should()
//                .ThrowAsync<NotFoundException>()
//                .WithMessage("Doctor with id 100 not found");
//        }

//        [Fact]
//        public async Task UpdateDoctor_ShouldCallRepositoryUpdateOnce()
//        {
//            var doctor = GetDoctors().First();

//            var dto = new UpdateDoctorDto();

//            _doctorRepository
//                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper.Setup(x => x.Map(dto, doctor));

//            _doctorRepository
//                .Setup(x => x.UpdateAsync(1, doctor, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper
//                .Setup(x => x.Map<DoctorDto>(doctor))
//                .Returns(new DoctorDto());

//            await _service.UpdateDoctor(1, dto);

//            _doctorRepository.Verify(
//                x => x.UpdateAsync(
//                    1,
//                    doctor,
//                    It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task UpdateDoctor_ShouldCallMapper()
//        {
//            var doctor = GetDoctors().First();

//            var dto = new UpdateDoctorDto();

//            _doctorRepository
//                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper.Setup(x => x.Map(dto, doctor));

//            _doctorRepository
//                .Setup(x => x.UpdateAsync(1, doctor, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper
//                .Setup(x => x.Map<DoctorDto>(doctor))
//                .Returns(new DoctorDto());

//            await _service.UpdateDoctor(1, dto);

//            _mapper.Verify(
//                x => x.Map(dto, doctor),
//                Times.Once);

//            _mapper.Verify(
//                x => x.Map<DoctorDto>(doctor),
//                Times.Once);
//        }

//        [Fact]
//        public async Task UpdateDoctor_ShouldThrowException_WhenRepositoryFails()
//        {
//            var doctor = GetDoctors().First();

//            var dto = new UpdateDoctorDto();

//            _doctorRepository
//                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(doctor);

//            _mapper.Setup(x => x.Map(dto, doctor));

//            _doctorRepository
//                .Setup(x => x.UpdateAsync(1, doctor, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            Func<Task> action =
//                async () => await _service.UpdateDoctor(1, dto);

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        private List<AppointmentDto> GetAppointmentDtos()
//        {
//            return new List<AppointmentDto>
//    {
//        new AppointmentDto
//        {
//            AppointmentId = 1
//        },
//        new AppointmentDto
//        {
//            AppointmentId = 2
//        }
//    };
//        }

//        private List<Appointment> GetAppointments()
//        {
//            return new List<Appointment>
//    {
//        new Appointment
//        {
//            AppointmentId = 1
//        },
//        new Appointment
//        {
//            AppointmentId = 2
//        }
//    };
//        }

//        [Fact]
//        public async Task GetAppointmentReport_ShouldReturnAppointments()
//        {
//            var appointments = GetAppointments();

//            var dtos = GetAppointmentDtos();

//            _appointmentRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(appointments);

//            _mapper
//                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
//                .Returns(dtos);

//            var result =
//                await _service.GetAppointmentReport();

//            result.Should().HaveCount(2);
//        }

//        [Fact]
//        public async Task GetAppointmentReport_ShouldReturnEmptyCollection()
//        {
//            _appointmentRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(new List<Appointment>());

//            _mapper
//                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(It.IsAny<IEnumerable<Appointment>>()))
//                .Returns(new List<AppointmentDto>());

//            var result =
//                await _service.GetAppointmentReport();

//            result.Should().BeEmpty();
//        }

//        [Fact]
//        public async Task GetAppointmentReport_ShouldThrowException_WhenRepositoryFails()
//        {
//            _appointmentRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception("Database Error"));

//            Func<Task> action =
//                async () => await _service.GetAppointmentReport();

//            await action.Should()
//                .ThrowAsync<Exception>()
//                .WithMessage("Database Error");
//        }

//        [Fact]
//        public async Task GetAppointmentReport_ShouldCallMapperOnce()
//        {
//            var appointments = GetAppointments();

//            var dtos = GetAppointmentDtos();

//            _appointmentRepository
//                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(appointments);

//            _mapper
//                .Setup(x => x.Map<IEnumerable<AppointmentDto>>(appointments))
//                .Returns(dtos);

//            await _service.GetAppointmentReport();

//            _mapper.Verify(
//                x => x.Map<IEnumerable<AppointmentDto>>(appointments),
//                Times.Once);
//        }
//    }
//}