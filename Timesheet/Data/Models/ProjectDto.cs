namespace Timesheet.Data.Models
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public List<AssignmentDto> Assignments { get; set; }
    }
}
