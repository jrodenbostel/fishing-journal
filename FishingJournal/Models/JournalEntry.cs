using System;
using System.ComponentModel.DataAnnotations;

namespace FishingJournal.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        
        [Required]
        public string Notes { get; set; }
        
        [Display(Name = "Location")]
        [Required]
        public string Location { get; set; }
        
        [Display(Name = "Weather Summary")]
        [Required]
        public string WeatherSummary { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}