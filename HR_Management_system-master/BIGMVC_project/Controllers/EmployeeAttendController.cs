using BIGMVC_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIGMVC_project.Controllers
{
	public class EmployeeAttendController : Controller
	{
		// Action to display attendance records
		private readonly MyDbContext _context;
		public EmployeeAttendController(MyDbContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");
			if (employeeId == null)
			{
				return RedirectToAction("EmployeeLogin", "Login");
			}

			var attendance = _context.Attendances.Where(a => a.EmployeeId == employeeId).ToList();

			return View(attendance);
		}
		[HttpPost]
		public IActionResult PunchIn()
		{
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			var employee = _context.Employees.Find(employeeId); // check if the id in session are same in DB

			if (employee == null)
			{
				return RedirectToAction("EmployeeLogin", "Login");
			}


			var attendance = new Attendance
			{
				EmployeeId = employeeId.Value,
				PunchIn = DateTime.Now,
			};

			_context.Attendances.Add(attendance);
			_context.SaveChanges();
			return RedirectToAction("Index");
		}
		[HttpPost]

		public IActionResult PunchOut(int attendanceId)
		{
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			var attendance = _context.Attendances.Find(attendanceId);

			if (attendance != null && attendance.PunchOut == null)
			{
				attendance.PunchOut = DateTime.Now;
				_context.Update(attendance);
				_context.SaveChanges();
			}
			return RedirectToAction("Index", "EmployeeAttend");
		}
		public IActionResult LeaveRequest()
		{
			var employeeName = HttpContext.Session.GetString("EmployeeName");
			ViewBag.EmployeeName = employeeName;

			var leaveRequest = new LeaveRequest(); // Initialize a new leave request
			return View(leaveRequest); // Pass the initialized object to the view
		}
		// POST: SubmitLeaveRequest
		[HttpPost]
		public async Task<IActionResult> SubmitLeaveRequest(LeaveRequest leaveRequest)
		{
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");


			if (employeeId == null)
			{
				TempData["Message"] = "Session expired. Please log in again.";
				return RedirectToAction("EmployeeLogin", "Login");
			}

			// Set default values for the request
			leaveRequest.EmployeeId = employeeId.Value;
			leaveRequest.LeaveRequestsStatusEnum = "Pending"; // Default status
			leaveRequest.RequestName = "Vacation"; // Default request name

			// If the leave type is "Vacation", set StartTime and EndTime to null
			if (leaveRequest.LeaveType == "Vacation")
			{
				leaveRequest.StartTime = null;
				leaveRequest.EndTime = null;
			}



			// Save the leave request to the database
			_context.LeaveRequests.Add(leaveRequest);
			await _context.SaveChangesAsync();

			return RedirectToAction("EmployeeLeaveRequests", "EmployeeAttend");


		}
		public IActionResult EmployeeLeaveRequests()
		{
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			if (employeeId == null)
			{
				return RedirectToAction("EmployeeLogin", "Login");
			}

			// Get the leave requests for the logged-in employee
			var leaveRequests = _context.LeaveRequests.Where(l => l.EmployeeId == employeeId).ToList();

			return View(leaveRequests);
		}
		public IActionResult ViewTasks()
		{
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");
			var findd = _context.Missions.Find(employeeId);
			// Fetch tasks for the static EmployeeId
			var tasks = _context.Missions.Where(m => m.EmployeeId == findd.EmployeeId) // Filter tasks by the logged-in employee's ID
				.OrderBy(m => m.StartDate)
				.ToList();

			// Group tasks by status
			var todoTasks = tasks.Where(t => t.TasksStatusEnum == "To Do").ToList();
			var doingTasks = tasks.Where(t => t.TasksStatusEnum == "Doing").ToList();
			var doneTasks = tasks.Where(t => t.TasksStatusEnum == "Done").ToList();

			// Pass the grouped tasks to the view
			ViewBag.ToDoTasks = todoTasks;
			ViewBag.DoingTasks = doingTasks;
			ViewBag.DoneTasks = doneTasks;

			return View();
		}


		[HttpPost]
		public async Task<IActionResult> UpdateTaskStatus(int taskId, string status)
		{
			// Find the task by ID
			var task = await _context.Missions.FindAsync(taskId);
			if (task == null)
			{
				return NotFound("Task not found.");
			}
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");


			// Update the task status
			task.TasksStatusEnum = status;
			_context.Missions.Update(task);
			await _context.SaveChangesAsync();

			// Redirect back to the ViewTasks page
			return RedirectToAction("ViewTasks", "EmployeeAttend");
		}
		public IActionResult Profile()
		{
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			if (employeeId == null)
			{
				TempData["Message"] = "Session expired. Please log in again.";
				return RedirectToAction("EmployeeLogin", "Login");
			}



			// Fetch the employee's details from the database
			var employee = _context.Employees
				.Include(e => e.Department) // Include related department data if needed
				.Include(e => e.Manager)   // Include related manager data if needed
				.FirstOrDefault(e => e.Id == employeeId);

			if (employee == null)
			{
				TempData["Message"] = "Employee not found.";
				return RedirectToAction("EmployeeLogin", "Login");
			}

			return View(employee);
		}
		// GET: Display the edit profile form
		public IActionResult EditProfile()
		{
			// Retrieve the logged-in employee's ID from the session
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			if (employeeId == null)
			{
				TempData["Message"] = "Session expired. Please log in again.";
				return RedirectToAction("EmployeeLogin", "Login");
			}

			// Fetch the employee's details from the database
			var employee = _context.Employees.Find(employeeId);

			if (employee == null)
			{
				TempData["Message"] = "Employee not found.";
				return RedirectToAction("EmployeeLogin", "Login");
			}

			return View(employee);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProfile(Employee model, IFormFile? profileImage)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var employee = await _context.Employees.FindAsync(model.Id);

				if (employee == null)
				{
					TempData["Message"] = "Employee not found.";
					return RedirectToAction("Login");
				}
				employee.Name = model.Name;
				employee.Email = model.Email;
				employee.Address = model.Address;
				employee.Position = model.Position;

				// Handle profile image upload
				if (profileImage != null && profileImage.Length > 0)
				{
					// Define the file path
					var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
					var filePath = Path.Combine("wwwroot/images", fileName);

					// Save the file to the server
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await profileImage.CopyToAsync(stream);
					}

					// Update the ImagePath property
					employee.ImagePath = $"/images/{fileName}";
				}

				// Save changes to the database
				_context.Update(employee);
				await _context.SaveChangesAsync();

				TempData["Message"] = "Profile updated successfully.";
				return RedirectToAction("Profile");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "An error occurred while updating the profile.");
				return View(model);
			}
		}
		public IActionResult Evaluation()
		{
			var employeeId = HttpContext.Session.GetInt32("EmployeeID");

			//var findd = _context.Evaluations.Find(employeeId);

			var emp = _context.Evaluations.FirstOrDefault(e => e.EmployeeId == employeeId);


		
			return View(emp);
		}
		public IActionResult Dashboard()
		{
			return View();
		}
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("EmployeeLogin", "Login");
		}


	}
}

