using HealthAxis.API.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAxis.API.Models
{
    public class HealthRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required(ErrorMessage = ValidationMessages.AppointmentRequired)]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = ValidationMessages.PatientRequired)]
        public int PatientId { get; set; }

        [Required(ErrorMessage = ValidationMessages.DoctorRequired)]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = ValidationMessages.VisitDateRequired)]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.DiagnosisRequired)]
        [StringLength(ValidationLimits.DiagnosisLength)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.PrescriptionRequired)]
        [StringLength(ValidationLimits.PrescriptionLength)]
        public string Prescription { get; set; } = string.Empty;

        [StringLength(ValidationLimits.NotesLength)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(AppointmentId))]
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
