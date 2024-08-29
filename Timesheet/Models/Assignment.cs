using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Timesheet.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Projects")]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public Project Projects { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}