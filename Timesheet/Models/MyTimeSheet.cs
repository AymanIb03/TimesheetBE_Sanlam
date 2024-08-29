using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Timesheet.Models
{
    public class MyTimesheet
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double HoursWorked { get; set; }
        
        [ForeignKey("Assignments")]
        public int AssignementId { get; set; }
        [JsonIgnore]
        public Assignment? Assignments { get; set; }
         public bool? IsValidated { get; set; } = null;

    }
}