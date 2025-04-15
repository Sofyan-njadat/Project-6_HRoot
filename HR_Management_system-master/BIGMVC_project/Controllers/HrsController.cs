using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BIGMVC_project.Models;
using System.Xml.Linq;
//using HR_Management.Data;
//using HR_Management.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_Management.Controllers
{
	public class HrsController : Controller
	{
		private readonly MyDbContext _context;

		public HrsController(MyDbContext context)
		{
			_context = context;
		}
		public IActionResult Dashboard()
		{

			var totalHR = _context.Hrs.Count();
			var totalEmployees = _context.Employees.Count();
			var totalManagers = _context.Managers.Count();
			var totalDepartments = _context.Departments.Count();
			var tasks = _context.Missions.Include(m => m.Employee).ToList();

			ViewBag.Tasks = tasks;
			ViewBag.TotalTasks = tasks.Count();
			ViewBag.TotalHR = totalHR;
			ViewBag.TotalEmployees = totalEmployees;
			ViewBag.TotalManagers = totalManagers;
			ViewBag.TotalDepartments = totalDepartments;

			var departmentEmployeeCount = _context.Employees
				.GroupBy(e => e.Department.Name)
				.Select(group => new
				{
					DepartmentName = group.Key,
					EmployeeCount = group.Count()
				})
				.ToList();

			ViewBag.DepartmentEmployeeCount = departmentEmployeeCount;

			return View();
		}




		public IActionResult LeaveRequests()
		{
			var allLeaveRequests = _context.LeaveRequests.ToList();
			return View(allLeaveRequests);
		}

		public IActionResult Feedback()
		{
			var feedbackList = _context.Feedbacks.ToList();
			return View(feedbackList);
		}


		[HttpPost]
		public async Task<IActionResult> SubmitFeedback(Feedback feedback)
		{
			if (ModelState.IsValid)
			{
				feedback.SubmittedAt = DateTime.Now;
				_context.Feedbacks.Add(feedback);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Success!";
				return RedirectToAction("Feedback");
			}

			return View("Feedback", feedback);
		}


		[HttpPost]
		public async Task<IActionResult> DeleteFeedback(int id)
		{
			var feedback = await _context.Feedbacks.FindAsync(id);
			if (feedback != null)
			{
				_context.Feedbacks.Remove(feedback);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Feedback");
		}


		[HttpPost]
		public async Task<IActionResult> ReplyToFeedback(int id, string reply)
		{
			var feedback = await _context.Feedbacks.FindAsync(id);
			if (feedback == null)
			{
				return NotFound();
			}

			feedback.ReplyMessage = reply;
			_context.Feedbacks.Update(feedback);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Success!";
			return RedirectToAction("Feedback");
		}


		//public IActionResult ContactUs()
		//{
		//    return View(new Feedback());
		//}


		[HttpPost]
		public IActionResult SendFeedback(Feedback feedback)
		{
			if (ModelState.IsValid)
			{
				feedback.SubmittedAt = DateTime.Now;
				_context.Feedbacks.Add(feedback);
				_context.SaveChanges();
				return RedirectToAction("ContactUs");
			}

			return View("ContactUs", feedback);
		}


		public IActionResult AddManager()
		{
			var departments = _context.Departments.ToList(); // assuming you're using Entity Framework
			ViewBag.Departments = departments;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> AddManager(Manager model, IFormFile profilePicture)
		{
			// Check if email is already taken
			if (_context.Managers.Any(m => m.Email == model.Email))
			{
				ModelState.AddModelError("Email", "This email is already in use.");
				return View(model);
			}

			// Check if the model is valid
			if (!ModelState.IsValid)
			{
				return View(model); // Return the view with validation errors if the model is invalid
			}

			// Handle profile picture upload
			if (profilePicture != null && profilePicture.Length > 0)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
				var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("ProfilePicture", "Invalid image format. Only .jpg, .jpeg, and .png are allowed.");
					return View(model);
				}

				// Clean the filename to avoid invalid characters
				var fileName = Path.GetFileNameWithoutExtension(profilePicture.FileName);
				var cleanedFileName = Path.Combine(fileName + fileExtension); // Add extension back
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "managers", cleanedFileName);
				var directoryPath = Path.GetDirectoryName(filePath);

				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await profilePicture.CopyToAsync(stream);
				}

				model.Image = cleanedFileName;
			}

			// Add the manager to the context
			_context.Managers.Add(model);
			await _context.SaveChangesAsync();

			// Redirect to a relevant action (for example, the details page of the new manager)
			return RedirectToAction("ManagerDetails", new { id = model.Id });
		}

		// GET: Hrs/ManagerDetails/5
		[HttpGet("manager/details/{id}")]
		public IActionResult ManagerDetails(int id)
		{
			var manager = _context.Managers
				.Include(m => m.Department)
				.FirstOrDefault(m => m.Id == id);

			if (manager == null)
			{
				return NotFound();
			}

			return RedirectToAction("Manager");
		}
		public IActionResult ExportLeaveRequestsToPDF()
		{
			var leaveRequests = _context.LeaveRequests.ToList();

			var pdfStream = new MemoryStream();

			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);

			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("Leave Requests", font)
			{
				Alignment = Element.ALIGN_CENTER
			};

			title.SpacingAfter = 20f;

			document.Add(title);

			document.Add(new Paragraph(" "));

			var table = new PdfPTable(5) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			table.AddCell("Employee ID");
			table.AddCell("Start Date");
			table.AddCell("End Date");
			table.AddCell("Reason");
			table.AddCell("Status");

			foreach (var request in leaveRequests)
			{
				table.AddCell(request.EmployeeId.ToString());
				table.AddCell(request.StartDate.ToShortDateString());
				table.AddCell(request.EndDate.ToShortDateString());
				table.AddCell(request.Reason);
				table.AddCell(request.LeaveRequestsStatusEnum);
			}

			document.Add(table);

			document.Close();

			return File(pdfStream.ToArray(), "application/pdf", "LeaveRequests.pdf");
		}

		public IActionResult DepartmentService()
		{
			return View();
		}
		//////////////////////////////////////
		/////////////////////////////////////////////
		/////////////////////////////////////////////
		///// GET: Hrs
		public async Task<IActionResult> Index()
		{
			return View(await _context.Hrs.ToListAsync());
		}

		// GET: Hrs/Details/5
		public async Task<IActionResult> HRDetails(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var hr = await _context.Hrs
				.FirstOrDefaultAsync(m => m.Id == id);
			if (hr == null)
			{
				return NotFound();
			}

			return View(hr);
		}

		// GET: Hrs/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Hrs/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Email,PasswordHash,Image")] Hr hr)
		{
			if (ModelState.IsValid)
			{
				_context.Add(hr);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(hr);
		}

		// GET: Hrs/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var hr = await _context.Hrs.FindAsync(id);
			if (hr == null)
			{
				return NotFound();
			}
			return View(hr);
		}

		// POST: Hrs/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,PasswordHash,Image")] Hr hr)
		{
			if (id != hr.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(hr);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!HrExists(hr.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(hr);
		}

		// GET: Hrs/Delete/5
		// GET: Hrs/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var hr = await _context.Hrs
				.FirstOrDefaultAsync(m => m.Id == id);
			if (hr == null)
			{
				return NotFound();
			}

			return View(hr);
		}

		// GET: Hrs/Create
		public IActionResult Createe()
		{
			return View(); // This will display the Create.cshtml form
		}

		// POST: Hrs/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Createe([Bind("Name,Email,PasswordHash,Image")] Hr hr)
		{
			if (ModelState.IsValid)
			{
				_context.Add(hr);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index)); // Redirect to HR list after creation
			}
			return View(hr); // Re-display form with validation errors
		}

		// POST: Hrs/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var hr = await _context.Hrs.FindAsync(id);
			if (hr != null)
			{
				_context.Hrs.Remove(hr);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool HrExists(int id)
		{
			return _context.Hrs.Any(e => e.Id == id);
		}

		// Export HR data to PDF
		public IActionResult ExportHRToPDF()
		{
			var HR = _context.Hrs.ToList();
			var pdfStream = new MemoryStream();
			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);
			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("HR Team", font)
			{
				Alignment = Element.ALIGN_CENTER
			};
			title.SpacingAfter = 20f;
			document.Add(title);
			document.Add(new Paragraph(" "));

			var table = new PdfPTable(3) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			table.AddCell("ID");
			table.AddCell("Name");
			table.AddCell("Email");

			foreach (var request in HR)
			{
				table.AddCell(request.Id.ToString());
				table.AddCell(request.Name ?? "N/A");
				table.AddCell(request.Email ?? "N/A");
			}

			document.Add(table);
			document.Close();
			pdfStream.Position = 0;

			return File(pdfStream.ToArray(), "application/pdf", "HRteam.pdf");
		}

		///////////////////////////////-----------------------------------------////////////////////////////////////
		///////////////////////////////-----------------------------------------////////////////////////////////////
		///////////////////////////////-----------------------------------------////////////////////////////////////

		public async Task<IActionResult> Department()
		{
			return View(await _context.Departments.ToListAsync());
		}

		// GET: Departments/Details/5
		public async Task<IActionResult> DetailsDepartment(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var department = await _context.Departments
				.FirstOrDefaultAsync(m => m.Id == id);
			if (department == null)
			{
				return NotFound();
			}

			return View(department);
		}
		// GET: Departments/Create
		public IActionResult DepartmentCreate()
		{
			return View();
		}

		// POST: Departments/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DepartmentCreate([Bind("Id,Name,Description")] Department department)
		{
			if (ModelState.IsValid)
			{
				_context.Add(department);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Department));
			}
			return View(department);
		}


		public IActionResult DeparmentToPDF()
		{
			var departments = _context.Departments.ToList();

			var pdfStream = new MemoryStream();
			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);

			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("Departments", font)
			{
				Alignment = Element.ALIGN_CENTER
			};

			title.SpacingAfter = 20f;

			document.Add(title);
			document.Add(new Paragraph(" "));

			var table = new PdfPTable(3) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			// Header
			table.AddCell("ID");
			table.AddCell("Name");
			table.AddCell("Description");

			foreach (var request in departments)
			{
				table.AddCell(request.Id.ToString());
				table.AddCell(request.Name ?? "N/A");
				table.AddCell(request.Description ?? "N/A");
			}

			document.Add(table);
			document.Close();

			pdfStream.Position = 0;

			return File(pdfStream.ToArray(), "application/pdf", "Departments.pdf");
		}




		// GET: Employees
		public async Task<IActionResult> Employee()
		{
			var employees = await _context.Employees
							   .Include(e => e.Department)
							   .Include(e => e.Manager) // Include Manager
							   .ToListAsync();

			return View(employees);
		}


		// GET: Employees/Details/5
		//public async Task<IActionResult> EmployeeDetails(int? id)
		//{
		//    if (id == null)
		//    {
		//        return NotFound();
		//    }

		//    var employee = await _context.Employees
		//                                 .Include(e => e.Department)
		//                                 .FirstOrDefaultAsync(m => m.Id == id);

		//    if (employee == null)
		//    {
		//        return NotFound();
		//    }

		//    return View(employee);
		//}

		//public IActionResult EmployeeCreate()
		//{
		//    ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "Name");

		//    return View();
		//}

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> EmployeeCreate([Bind("Id,Name,Email,PasswordHash,Address,Position,DepartmentId,ImagePath")] Employee employee)
		//{
		//    if (ModelState.IsValid)
		//    {
		//        // Check if the email already exists
		//        bool emailExists = await _context.Employees.AnyAsync(e => e.Email == employee.Email);
		//        if (emailExists)
		//        {
		//            ModelState.AddModelError("Email", "The email is already in use. Please use a different email.");
		//            return View(employee);
		//        }

		//        _context.Add(employee);
		//        await _context.SaveChangesAsync();
		//        return RedirectToAction(nameof(Employee));
		//    }

		//    // Get departments to populate the dropdown
		//    ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);

		//    return View(employee);
		//}

		// GET: Employees/Edit/5
		// GET: Employees/Edit/5
		public async Task<IActionResult> EmployeeEdit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var employee = await _context.Employees.FindAsync(id);
			if (employee == null)
			{
				return NotFound();
			}

			// Populate the Department and Manager dropdowns
			ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
			ViewBag.ManagerId = new SelectList(_context.Employees.Where(e => e.Id != employee.Id), "Id", "Name", employee.ManagerId);

			return View(employee);  // Return the employee model to the view
		}

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> EmployeeEdit(int id, Employee employee)
		//{
		//	if (id != employee.Id)
		//	{
		//		return NotFound();
		//	}

		//	// Ensure the ManagerId exists in the database
		//	var managerExists = await _context.Employees.AnyAsync(e => e.Id == employee.ManagerId);
		//	//if (employee.ManagerId.HasValue && !managerExists)
		//	//{
		//	//	ModelState.AddModelError("ManagerId", "The selected manager does not exist.");
		//	//	// Populate the Department and Manager dropdowns again in case of validation errors
		//	//	ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
		//	//	ViewBag.ManagerId = new SelectList(_context.Employees.Where(e => e.Id != employee.Id), "Id", "Name", employee.ManagerId);
		//	//	return View(employee);  // Return the form with validation error
		//	//
		//	//}
		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			//employee.Id = 0;
		//			// Update the employee data
		//			_context.Update(employee);
		//			await _context.SaveChangesAsync();  // Save the changes to the database
		//		}
		//		catch (DbUpdateConcurrencyException)
		//		{
		//			if (!EmployeeExists(employee.Id))
		//			{
		//				return NotFound();
		//			}
		//			else
		//			{
		//				throw;
		//			}
		//		}

		//		// Redirect to the employee listing page after successful update
		//		return RedirectToAction(nameof(Employee));
		//	}

		//	// Populate the Department and Manager dropdowns again in case of validation errors
		//	ViewBag.DepartmentId = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
		//	ViewBag.ManagerId = new SelectList(_context.Employees.Where(e => e.Id != employee.Id), "Id", "Name", employee.ManagerId);

		//	return View(employee);  // Return to the view with validation errors
		//}



		private bool EmployeeExists(int id)
		{
			return _context.Employees.Any(e => e.Id == id);
		}



		// GET: Manager/Delete/5
		public async Task<IActionResult> ManagerDelete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var manager = await _context.Managers
										 .Include(m => m.Department)
										 .Include(m => m.Employees) // Optional: Include employees if you want to show them
										 .FirstOrDefaultAsync(m => m.Id == id);

			if (manager == null)
			{
				return NotFound();
			}

			return View(manager); // You can show the manager's details to confirm deletion
		}

		// POST: Manager/Delete/5
		[HttpPost, ActionName("ManagerDelete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ManagerDeleteConfirmed(int id)
		{
			var manager = await _context.Managers
										 .Include(m => m.Evaluations)
										 .FirstOrDefaultAsync(m => m.Id == id);

			if (manager == null)
			{
				return NotFound();
			}

			_context.Evaluations.RemoveRange(manager.Evaluations);

			foreach (var employee in manager.Employees)
			{
				employee.ManagerId = null;
				_context.Employees.Update(employee);
			}

			_context.Managers.Remove(manager);

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Manager));
		}



		// Employee to PDF
		public IActionResult EmployeeToPDF()
		{
			var employees = _context.Employees
									.Include(e => e.Department)
									.Include(e => e.Manager)
									.ToList();

			var pdfStream = new MemoryStream();
			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);

			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("Our Employees", font)
			{
				Alignment = Element.ALIGN_CENTER
			};

			title.SpacingAfter = 20f;

			document.Add(title);
			document.Add(new Paragraph(" "));

			var table = new PdfPTable(6) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			// Header
			table.AddCell("Name");
			table.AddCell("Email");
			table.AddCell("Address");
			table.AddCell("Position");
			table.AddCell("Manager");
			table.AddCell("Department Name");

			foreach (var employee in employees)
			{
				table.AddCell(employee.Name ?? "N/A");
				table.AddCell(employee.Email ?? "N/A");
				table.AddCell(employee.Address ?? "N/A");
				table.AddCell(employee.Position ?? "N/A");
				table.AddCell(employee.Manager?.Name ?? "N/A");
				table.AddCell(employee.Department?.Name ?? "N/A");
			}

			document.Add(table);
			document.Close();

			pdfStream.Position = 0;

			return File(pdfStream.ToArray(), "application/pdf", "Employees.pdf");
		}

		//private bool EmployeeExists(int id)
		//{
		//    return _context.Employees.Any(e => e.Id == id);
		//}



		// GET: Managers
		public async Task<IActionResult> Manager()
		{
			var myDbContext = _context.Managers.Include(m => m.Department);
			return View(await myDbContext.ToListAsync());
		}

		// MANAGER TO PDF
		public IActionResult ManagerToPDF()
		{
			var managers = _context.Managers
				.Include(m => m.Department)
			.ToList();

			var pdfStream = new MemoryStream();
			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);

			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("Our Managers", font)
			{
				Alignment = Element.ALIGN_CENTER
			};

			title.SpacingAfter = 20f;

			document.Add(title);
			document.Add(new Paragraph(" "));

			var table = new PdfPTable(3) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			// Header
			table.AddCell("Name");
			table.AddCell("Email");
			table.AddCell("Department Name");

			foreach (var manager in managers)
			{
				table.AddCell(manager.Name ?? "N/A");
				table.AddCell(manager.Email ?? "N/A");
				table.AddCell(manager.Department?.Name ?? "N/A");
			}

			document.Add(table);
			document.Close();

			// إعادة تعيين Position بتاعة MemoryStream
			pdfStream.Position = 0;

			return File(pdfStream.ToArray(), "application/pdf", "Managers.pdf");
		}

		// GET: Managers/Details/5
		public async Task<IActionResult> ManagerDetails(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var manager = await _context.Managers
				.Include(m => m.Department)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (manager == null)
			{
				return NotFound();
			}

			return View(manager);
		}

		//// GET: Managers/Create
		//public IActionResult ManagerCreate()
		//{
		//	ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
		//	return View();
		//}

		//// POST: Managers/Create
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> ManagerCreate([Bind("Id,Name,Email,PasswordHash,DepartmentId,Image")] Manager manager)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_context.Add(manager);
		//		await _context.SaveChangesAsync();
		//		return RedirectToAction(nameof(Manager));
		//	}
		//	ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", manager.DepartmentId);
		//	return View(manager);
		//}




		// GET: Evaluations
		public async Task<IActionResult> Evaluation()
		{
			// تحميل قائمة التقييمات من قاعدة البيانات مع ربط الموظف والمدير
			var evaluations = await _context.Evaluations
				.Include(e => e.Employee)  // ربط التقييم مع الموظف
				.Include(e => e.Manager)   // ربط التقييم مع المدير
				.ToListAsync();

			// تمرير التقييمات إلى الـ View
			return View(evaluations);
		}




		// GET: Evaluations/Details/5
		public async Task<IActionResult> EvaluationDetails(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var evaluation = await _context.Evaluations
				.Include(e => e.Employee)
				.Include(e => e.Manager)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (evaluation == null)
			{
				return NotFound();
			}

			return View(evaluation);
		}
		// GET: Managers/Edit/5
		public async Task<IActionResult> ManagerEdit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var manager = await _context.Managers.FindAsync(id);
			if (manager == null)
			{
				return NotFound();
			}

			ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", manager.DepartmentId);
			return View(manager);
		}

		// POST: Managers/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ManagerEdit(int id, [Bind("Id,Name,Email,PasswordHash,DepartmentId,Image")] Manager manager)
		{
			if (id != manager.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(manager);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Managers.Any(e => e.Id == manager.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Manager));
			}

			ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", manager.DepartmentId);
			return View(manager);
		}

		//// GET: Managers/Delete/5
		//[ValidateAntiForgeryToken]
		//[HttpPost, ActionName("ManagerDelete")]
		//public async Task<IActionResult> ManagerDeleteConfirmed(int id)
		//{
		//    var manager = await _context.Managers.FindAsync(id);
		//    if (manager != null)
		//    {
		//        // حذف التقييمات المرتبطة بالمدير
		//        var evaluations = _context.Evaluations.Where(e => e.Id == id);
		//        _context.Evaluations.RemoveRange(evaluations);

		//        // حذف المدير نفسه
		//        _context.Managers.Remove(manager);
		//        await _context.SaveChangesAsync();
		//    }

		//    return RedirectToAction(nameof(Manager));
		//}

		public IActionResult EvaluationToPDF()
		{
			var evaluations = _context.Evaluations
				.Include(e => e.Employee)
				.Include(e => e.Manager)
			.ToList();

			var pdfStream = new MemoryStream();
			var document = new Document(PageSize.A4);
			var writer = PdfWriter.GetInstance(document, pdfStream);

			document.Open();

			var font = FontFactory.GetFont("Arial", 20, Font.BOLD);
			Paragraph title = new Paragraph("Employee Evaluations", font)
			{
				Alignment = Element.ALIGN_CENTER
			};

			title.SpacingAfter = 20f;

			document.Add(title);
			document.Add(new Paragraph(" "));

			var table = new PdfPTable(5) { WidthPercentage = 100 };
			table.SpacingBefore = 20f;

			// Header
			table.AddCell("Employee Name");
			table.AddCell("Manager Name");
			table.AddCell("Evaluation Date");
			table.AddCell("Rating");
			table.AddCell("Comments");

			foreach (var evaluation in evaluations)
			{
				table.AddCell(evaluation.Employee?.Name ?? "N/A");
				table.AddCell(evaluation.Manager?.Name ?? "N/A");
				table.AddCell(evaluation.DateEvaluated?.ToString("yyyy-MM-dd") ?? "N/A");
				table.AddCell(evaluation.EvaluationsStatusEnum.ToString() ?? "N/A");
				table.AddCell(evaluation.Comments ?? "N/A");
			}

			document.Add(table);
			document.Close();

			// إعادة تعيين Position بتاعة MemoryStream
			pdfStream.Position = 0;

			return File(pdfStream.ToArray(), "application/pdf", "Evaluations.pdf");
		}

	}
}

