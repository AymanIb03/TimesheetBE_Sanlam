using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timesheet.Data;
using Timesheet.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Timesheet.Data.Models;
using System.Collections.Generic;

namespace Timesheet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class TimesheetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TimesheetController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimesheet(MyTimesheet timesheet)
        {
            ModelState.Remove("Assignments");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignment = await _context.Assignments
                .Include(a => a.Projects)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == timesheet.AssignementId && a.UserId == userId);

            if (assignment == null)
            {
                return BadRequest("Assignement introuvable ou n'appartenant pas à l'utilisateur.");
            }

            var existingTimesheet = await _context.Timesheets
                .FirstOrDefaultAsync(t => t.AssignementId == timesheet.AssignementId &&
                                          t.Date == timesheet.Date);

            if (existingTimesheet != null)
            {
                return BadRequest("Un timesheet pour ce projet et cette date existe déjà.");
            }

            timesheet.Assignments = assignment;

            _context.Timesheets.Add(timesheet);
            await _context.SaveChangesAsync();

            // Envoi de notification à l'admin pour le nouveau timesheet
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var notifications = admins.Select(admin => new Notification
            {
                UserId = admin.Id,
                Message = $"Un nouveau timesheet a été soumis par {assignment.User.UserName}.",
                Type = NotificationType.Admin,
                DateCreated = DateTime.Now,
                IsRead = false,
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(timesheet);
        }

        [HttpGet("GroupedByProject")]
        public async Task<IActionResult> GetTimesheetsGroupedByProject()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var timesheetsGroupedByProject = await _context.Timesheets
                .Include(t => t.Assignments)
                .ThenInclude(a => a.Projects)
                .Where(t => t.Assignments.UserId == userId)
                .GroupBy(t => t.Assignments.ProjectId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    ProjectName = g.First().Assignments.Projects.ProjectName,
                    Timesheets = g.Select(t => new
                    {
                        t.Id,
                        t.Date,
                        t.HoursWorked,
                        t.IsValidated
                    }).ToList()
                })
                .ToListAsync();

            return Ok(timesheetsGroupedByProject);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTimesheets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var timesheets = await _context.Timesheets
                .Include(t => t.Assignments)
                .ThenInclude(a => a.Projects)
                .Where(t => t.Assignments.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Date,
                    t.HoursWorked,
                    ProjectId = t.Assignments.ProjectId,
                    ProjectName = t.Assignments.Projects.ProjectName,
                    t.IsValidated
                })
                .ToListAsync();

            return Ok(timesheets);
        }

        [HttpGet("UserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            return Ok(user);
        }

        [HttpGet("UserProjects")]
        public async Task<IActionResult> GetUserProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var projects = await _context.Projects
                .Where(p => p.Assignments.Any(a => a.UserId == userId))
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("AssignmentsByProject/{projectId}")]
        public async Task<IActionResult> GetAssignmentsByProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignments = await _context.Assignments
                .Where(a => a.ProjectId == projectId && a.UserId == userId)
                .Select(a => new
                {
                    a.Id
                })
                .ToListAsync();

            if (assignments == null || assignments.Count == 0)
            {
                return NotFound("Aucun assignement trouvé pour ce projet.");
            }

            return Ok(assignments);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimesheet(int id, MyTimesheet updatedTimesheet)
        {
            if (id != updatedTimesheet.Id)
            {
                return BadRequest("L'ID du timesheet ne correspond pas.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var timesheet = await _context.Timesheets
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == id && t.Assignments.UserId == userId);

            if (timesheet == null)
            {
                return NotFound("Timesheet non trouvé ou ne vous appartient pas.");
            }

            timesheet.Date = updatedTimesheet.Date;
            timesheet.HoursWorked = updatedTimesheet.HoursWorked;
            timesheet.AssignementId = updatedTimesheet.AssignementId;
            timesheet.IsValidated = null;

            _context.Entry(timesheet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimesheetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool TimesheetExists(int id)
        {
            return _context.Timesheets.Any(t => t.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimesheet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var timesheet = await _context.Timesheets
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == id && t.Assignments.UserId == userId);

            if (timesheet == null)
            {
                return NotFound("Timesheet non trouvé ou ne vous appartient pas.");
            }

            _context.Timesheets.Remove(timesheet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimesheetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var timesheet = await _context.Timesheets
                .Include(t => t.Assignments)
                .ThenInclude(a => a.Projects)
                .Where(t => t.Id == id && t.Assignments.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Date,
                    t.HoursWorked,
                    ProjectId = t.Assignments.ProjectId,
                    ProjectName = t.Assignments.Projects.ProjectName,
                    t.IsValidated
                })
                .FirstOrDefaultAsync();

            if (timesheet == null)
            {
                return NotFound("Timesheet non trouvé ou ne vous appartient pas.");
            }

            return Ok(timesheet);
        }

        [HttpGet("AllUserProjects")]
        public async Task<IActionResult> GetAllUserProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var projects = await _context.Projects
                .Where(p => p.Assignments.Any(a => a.UserId == userId))
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName,
                    Assignments = p.Assignments.Where(a => a.UserId == userId).Select(a => new
                    {
                        a.Id
                    }).ToList()
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("AssignmentsByUser")]
        public async Task<IActionResult> GetAssignmentsByUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignments = await _context.Assignments
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.Id,
                    a.ProjectId
                })
                .ToListAsync();

            if (assignments == null || assignments.Count == 0)
            {
                return NotFound("Aucun assignement trouvé pour cet utilisateur.");
            }

            return Ok(assignments);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
                return Unauthorized("Utilisateur non trouvé.");

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
                return BadRequest("Le mot de passe actuel est incorrect.");

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("Le mot de passe a été changé avec succès.");
        }

        // Ajout d'un endpoint pour récupérer les notifications pour l'utilisateur
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
