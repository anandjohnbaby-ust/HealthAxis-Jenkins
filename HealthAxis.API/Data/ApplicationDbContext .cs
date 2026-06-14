using HealthAxis.API.Enums;
using HealthAxis.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthAxis.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<HealthRecord> HealthRecords { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureAppointmentRelationships(modelBuilder);

            ConfigureHealthRecordRelationships(modelBuilder);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Specialisation)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            SeedData(modelBuilder);
        }

        private static void ConfigureAppointmentRelationships(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureHealthRecordRelationships(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HealthRecord>()
                .HasOne(h => h.Patient)
                .WithMany(p => p.HealthRecords)
                .HasForeignKey(h => h.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HealthRecord>()
                .HasOne(h => h.Doctor)
                .WithMany(d => d.HealthRecords)
                .HasForeignKey(h => h.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.HealthRecord)
                .WithOne(hr => hr.Appointment)
                .HasForeignKey<HealthRecord>(hr => hr.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doctor>()
                .HasData(

                    new Doctor
                    {
                        DoctorId = 1,
                        FullName = "Dr Arjun Narayanan",
                        Specialisation = Specialisation.Cardiology,
                        YearsOfExperience = 12,
                        ConsultationFee = 850m,
                        IsActive = true
                    },

                    new Doctor
                    {
                        DoctorId = 2,
                        FullName = "Dr Kavya Srinivasan",
                        Specialisation = Specialisation.Dermatology,
                        YearsOfExperience = 8,
                        ConsultationFee = 700m,
                        IsActive = true
                    },

                    new Doctor
                    {
                        DoctorId = 3,
                        FullName = "Dr Rohit Menon",
                        Specialisation = Specialisation.Neurology,
                        YearsOfExperience = 15,
                        ConsultationFee = 1200m,
                        IsActive = true
                    },

                    new Doctor
                    {
                        DoctorId = 4,
                        FullName = "Dr Priya Ramachandran",
                        Specialisation = Specialisation.Pediatrics,
                        YearsOfExperience = 6,
                        ConsultationFee = 650m,
                        IsActive = true
                    },

                    new Doctor
                    {
                        DoctorId = 5,
                        FullName = "Dr Siddharth Iyer",
                        Specialisation = Specialisation.Orthopedics,
                        YearsOfExperience = 10,
                        ConsultationFee = 900m,
                        IsActive = true
                    }
                );

                modelBuilder.Entity<Patient>()
                .HasData(

                    new Patient
                    {
                        PatientId = 1,
                        FullName = "Ananya Krishnan",
                        DateOfBirth = new DateTime(1998, 5, 12),
                        Gender = Gender.Female,
                        PhoneNumber = "9876543210",
                        Email = "ananya@example.com",
                        InsuranceId = "INS1001",
                        CreatedDate = new DateTime(2026, 1, 1)
                    },

                    new Patient
                    {
                        PatientId = 2,
                        FullName = "Rahul Nair",
                        DateOfBirth = new DateTime(1992, 9, 25),
                        Gender = Gender.Male,
                        PhoneNumber = "9876543211",
                        Email = "rahul@example.com",
                        InsuranceId = "INS1002",
                        CreatedDate = new DateTime(2026, 1, 1)
                    }
                );
        }
    }
}