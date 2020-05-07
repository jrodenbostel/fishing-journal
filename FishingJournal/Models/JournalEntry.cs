using System;
using System.ComponentModel.DataAnnotations;

namespace FishingJournal.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }

        public string Email { get; set; }
        
        [Required] public string Notes { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        [Display(Name = "Location")]
        [Required]
        public string LocationOverride { get; set; }
        
        [Required]
        public string Precipitation { get; set; }
        
        [Required]
        public string Temperature { get; set; }
        
        [Required]
        public string Humidity { get; set; }
        
        [Required]
        public string BarometricPressure { get; set; }
        
        [Required]
        public string WindSpeed { get; set; }
        
        [Required]
        public string WindDirection { get; set; }

        [DataType(DataType.Date)] public DateTime Date { get; set; }
    }
}