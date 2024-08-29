using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timesheet.Data;
using Timesheet.Models;
using System.Threading.Tasks;
using Timesheet.Data.Models;
using Timesheet.Extension;
using System.Security.Claims;

namespace Timesheet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailSender _emailSender;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, EmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userDetails = new List<object>();

            foreach (var user in users)
            {
                var assignments = await _context.Assignments
                    .Where(a => a.UserId == user.Id)
                    .Include(a => a.Projects)
                    .ToListAsync();

                var timesheets = await _context.Timesheets
                    .Where(t => assignments.Select(a => a.Id).Contains(t.AssignementId))
                    .ToListAsync();

                var roles = await _userManager.GetRolesAsync(user);

                userDetails.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.IsActive,
                    Roles = roles,
                    Assignments = assignments.Select(a => new
                    {
                        a.Id,
                        a.ProjectId,
                        a.Projects.ProjectName,
                    }),
                    Timesheets = timesheets.Select(t => new
                    {
                        t.Id,
                        t.Date,
                        t.HoursWorked,
                        t.IsValidated
                    })
                });
            }

            return Ok(userDetails);
        }

        [HttpPost("ValidateTimesheet/{id}")]
        public async Task<IActionResult> ValidateTimesheet(int id, [FromBody] bool isValidated)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet == null)
            {
                return NotFound("Timesheet not found");
            }

            timesheet.IsValidated = isValidated;

            // Envoi de notification à l'utilisateur si la feuille de temps est validée ou rejetée
            var assignment = await _context.Assignments.FindAsync(timesheet.AssignementId);
            if (assignment != null)
            {
                var userId = assignment.UserId;
                var message = isValidated
                    ? "Votre timesheet a été validé."
                    : "Votre timesheet a été rejeté.";

                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = NotificationType.User,
                    DateCreated = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return Ok(timesheet);
        }

        [HttpPost("ToggleUserStatus/{userId}")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Ok(user);
        }

        [HttpPost("AssignUserToProject")]
        public async Task<IActionResult> AssignUserToProject([FromBody] AssignUserToProjectDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var project = await _context.Projects.FindAsync(model.ProjectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            var assignment = new Assignment
            {
                UserId = model.UserId,
                ProjectId = model.ProjectId
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(assignment);
        }

        [HttpGet("GetProjectsNotAssignedToUser/{userId}")]
        public async Task<IActionResult> GetProjectsNotAssignedToUser(string userId)
        {
            var projects = await _context.Projects
                .Where(p => !_context.Assignments.Any(a => a.ProjectId == p.Id && a.UserId == userId))
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPost("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] DtoEditUser model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var userRoles = await _userManager.GetRolesAsync(user);
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!removeRolesResult.Succeeded)
            {
                return BadRequest("Failed to remove user roles.");
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    return BadRequest("Role does not exist.");
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addRoleResult.Succeeded)
                {
                    return BadRequest("Failed to add new role to user.");
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(user);
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("GetUser/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var assignments = await _context.Assignments
                .Where(a => a.UserId == user.Id)
                .Include(a => a.Projects)
                .ToListAsync();

            var timesheets = await _context.Timesheets
                .Where(t => assignments.Select(a => a.Id).Contains(t.AssignementId))
                .ToListAsync();

            var roles = await _userManager.GetRolesAsync(user);

            var userDetails = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.IsActive,
                Roles = roles,
                Assignments = assignments.Select(a => new
                {
                    a.Id,
                    a.ProjectId,
                    a.Projects.ProjectName,
                }),
                Timesheets = timesheets.Select(t => new
                {
                    t.Id,
                    t.Date,
                    t.HoursWorked,
                    t.IsValidated
                })
            };

            return Ok(userDetails);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(DtoNewUser user)
        {
            if (ModelState.IsValid)
            {
                string generatedPassword = PasswordGenerator.GenerateRandomPassword();

                AppUser appUser = new()
                {
                    UserName = user.userName,
                    Email = user.email,
                    IsActive = true
                };

                IdentityResult result = await _userManager.CreateAsync(appUser, generatedPassword);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(user.role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(user.role));
                    }

                    await _userManager.AddToRoleAsync(appUser, user.role);

                    await _emailSender.SendEmailAsync(
                        user.email,
                        "Votre nouveau compte a été créé",
                        "CreateUser",
                        userName: user.userName,
                        password: generatedPassword);

                    return Ok("User created successfully and email sent.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }

        [HttpGet("GetProjects")]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Assignments)
                .ThenInclude(a => a.User)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Assignments = p.Assignments.Select(a => new AssignmentDto
                    {
                        Id = a.Id,
                        UserName = a.User.UserName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errors });
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return Ok(project);
        }

        [HttpPut("EditProject")]
        public async Task<IActionResult> EditProject([FromBody] Project project)
        {
            if (project == null || project.Id == 0)
            {
                return BadRequest("Invalid project data.");
            }

            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null)
            {
                return NotFound("Project not found");
            }

            existingProject.ProjectName = project.ProjectName;
            _context.Projects.Update(existingProject);
            await _context.SaveChangesAsync();

            return Ok(existingProject);
        }

        [HttpDelete("DeleteProject/{projectId}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok("Project deleted successfully");
        }

        [HttpGet("GetProject/{projectId}")]
        public async Task<IActionResult> GetProject(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Assignments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound("Project not found");
            }

            return Ok(project);
        }

        [HttpGet("GetTimesheets")]
        public async Task<IActionResult> GetTimesheets()
        {
            var timesheets = await _context.Timesheets
                .Include(t => t.Assignments)
                .ThenInclude(a => a.Projects)
                .Include(t => t.Assignments)
                .ThenInclude(a => a.User)
                .Select(t => new
                {
                    t.Id,
                    ProjectName = t.Assignments.Projects.ProjectName,
                    UserName = t.Assignments.User.UserName,
                    t.Date,
                    t.HoursWorked,
                    t.IsValidated
                })
                .ToListAsync();

            return Ok(timesheets);
        }

        [HttpGet("GetAssignments")]
        public async Task<IActionResult> GetAssignments()
        {
            var assignments = await _context.Assignments
                .Include(a => a.Projects)
                .Include(a => a.User)
                .Select(a => new
                {
                    a.Id,
                    ProjectName = a.Projects.ProjectName,
                    UserName = a.User.UserName,
                })
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpDelete("DeleteAssignment/{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound("Assignment not found");
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok("Assignment deleted successfully");
        }

        [HttpGet("AdminInfo")]
        public async Task<IActionResult> GetAdminInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var admin = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                })
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                return NotFound("Admin not found.");
            }

            return Ok(admin);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
                return Unauthorized("Admin not found.");

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
                return BadRequest("The current password is incorrect.");

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("Password changed successfully.");
        }

        [HttpGet("DownloadTimesheets")]
        public async Task<IActionResult> DownloadTimesheets(string userId, int projectId, DateTime startDate, DateTime endDate, bool proceed = false)
        {
            if (string.IsNullOrEmpty(userId) || projectId <= 0 || startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                return BadRequest("Invalid parameters. Please ensure that userId, projectId, startDate, and endDate are correctly provided.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            var assignments = await _context.Assignments
                .Where(a => a.UserId == userId && a.ProjectId == projectId)
                .ToListAsync();

            if (!assignments.Any())
            {
                return NotFound("No assignments found for the given user and project.");
            }

            var timesheets = await _context.Timesheets
                .Where(t => assignments.Select(a => a.Id).Contains(t.AssignementId)
                            && t.Date >= startDate
                            && t.Date <= endDate)
                .ToListAsync();

            var invalidTimesheets = timesheets
                .Where(t => t.IsValidated == false || t.IsValidated == null)
                .ToList();

            var validTimesheets = timesheets
                .Where(t => t.IsValidated == true)
                .ToList();

            if (invalidTimesheets.Any() && !proceed)
            {
                return Ok(new
                {
                    Message = "Some timesheets are not validated. Do you want to continue with only the validated timesheets?",
                    InvalidTimesheets = invalidTimesheets.Select(t => new
                    {
                        t.Id,
                        t.Date,
                        t.HoursWorked,
                        t.IsValidated
                    }),
                    ValidTimesheets = validTimesheets.Select(t => new
                    {
                        t.Id,
                        t.Date,
                        t.HoursWorked
                    })
                });
            }

            if (!validTimesheets.Any())
            {
                return BadRequest(new
                {
                    Message = "No validated timesheets found in the selected date range.",
                });
            }

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Timesheets");

                worksheet.Cell(1, 1).Value = "Utilisateur : " + user.UserName;
                worksheet.Cell(2, 1).Value = "Projet : " + project.ProjectName;
                worksheet.Cell(3, 1).Value = "Intervalle de dates : " + startDate.ToShortDateString() + " - " + endDate.ToShortDateString();

                worksheet.Cell(5, 1).Value = "Date";
                worksheet.Cell(5, 2).Value = "Heures travaillées";

                for (int i = 0; i < validTimesheets.Count; i++)
                {
                    worksheet.Cell(i + 6, 1).Value = validTimesheets[i].Date.ToShortDateString();
                    worksheet.Cell(i + 6, 2).Value = validTimesheets[i].HoursWorked;
                }

                string fileName = $"Timesheets_{user.UserName}_{project.ProjectName}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet("GetProjectsForUser/{userId}")]
        public async Task<IActionResult> GetProjectsForUser(string userId)
        {
            var projects = await _context.Assignments
                .Where(a => a.UserId == userId)
                .Include(a => a.Projects)
                .Select(a => new
                {
                    a.ProjectId,
                    a.Projects.ProjectName
                })
                .ToListAsync();

            return Ok(projects);
        }

        // Ajout d'un endpoint pour récupérer les notifications
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return Ok(notifications);
        }

        // Endpoint pour marquer une notification comme lue
        [HttpPost("MarkNotificationAsRead/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(notification);
        }
    }
}
