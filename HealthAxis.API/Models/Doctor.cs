    using HealthAxis.API.Enums;
    using HealthAxis.API.Utilities;
    using System.ComponentModel.DataAnnotations;

    namespace HealthAxis.API.Models
    {
        public class Doctor
        {
            [Key]
            public int DoctorId { get; set; }

            [Required(ErrorMessage = ValidationMessages.FullNameRequired)]
            [StringLength(ValidationLimits.FullNameLength)]
            [RegularExpression(
                RegexPatterns.FullName,
                ErrorMessage = ValidationMessages.InvalidFullNameFormat)]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = ValidationMessages.SpecialisationRequired)]
            public Specialisation Specialisation { get; set; }

            [Range(
                ValidationLimits.MinExperience,
                ValidationLimits.MaxExperience,
                ErrorMessage = ValidationMessages.InvalidExperienceRange)]
            public int YearsOfExperience { get; set; }

            [Range(
                typeof(decimal),
                ValidationLimits.MinConsultationFee,
                ValidationLimits.MaxConsultationFee,
                ErrorMessage = ValidationMessages.InvalidConsultationFee)]
            public decimal ConsultationFee { get; set; }

            public bool IsActive { get; set; } = true;

            // Navigation Properties
            public virtual ICollection<Appointment> Appointments { get; set; }
                = new List<Appointment>();

            public virtual ICollection<HealthRecord> HealthRecords { get; set; }
                = new List<HealthRecord>();

            public bool IsAvailable(
                DateTime scheduledDate,
                string timeSlot)
            {

                return !Appointments.Any(a =>
                    a.ScheduledDate.Date == scheduledDate.Date &&
                    a.TimeSlot == timeSlot &&
                    a.Status != AppointmentStatus.Cancelled);
            }

            public int GetUpcomingAppointmentCount()
            {

                return Appointments.Count(a =>
                    a.ScheduledDate.Date >= DateTime.Today &&
                    a.Status != AppointmentStatus.Cancelled);
            }
            
            public void Activate()
            {
                IsActive = true;
            }

            public void Deactivate()
            {
                IsActive = false;
            }
        }
    }
