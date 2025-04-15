using BIGMVC_project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BIGMVC_project.Services;
using System.Net.Sockets;
using System.Net;
//using BCrypt.Net;

namespace BIGMVC_project.Controllers
{
	public class LoginController : Controller
	{

		private readonly EmailService _emailService;
		private readonly MyDbContext _context;


		public LoginController(EmailService emailService, MyDbContext context)
		{
			_emailService = emailService;
			_context = context;
		}


		// GET: LoginController
		public ActionResult Index()
		{
			return View();
		}


		//-------HRLogin------

		public string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 
				{
					return ip.ToString();
				}
			}
			return "127.0.0.1";
		}
		[HttpPost]
		public async Task<IActionResult> HrLogin(Hr model)
		{
			string userIp = GetLocalIPAddress();

			Console.WriteLine($"User Local IP: {userIp}");

			if (userIp == "192.168.1.124")
			{
				if (!ModelState.IsValid)
					return View(model);

				var hr = await _context.Hrs
					.FirstOrDefaultAsync(h => h.Email == model.Email && h.PasswordHash == model.PasswordHash);

				if (hr == null)
				{
					ModelState.AddModelError("", "Invalid email or password");
					return View(model);
				}


				HttpContext.Session.SetString("UserLoggedIn", "true");
				HttpContext.Session.SetInt32("HrID", hr.Id);
				HttpContext.Session.SetString("HrName", hr.Name ?? ""); // تجنب الخطأ إذا كانت null
				HttpContext.Session.SetString("HrEmail", hr.Email ?? "");
				HttpContext.Session.SetString("HrImage", hr.Image ?? "");

				return RedirectToAction("Dashboard", "Hrs");
				//return Json(new { success = true, message = $"You are connected. Your Local IP: {userIp}" });
			}

			return Json(new { success = false, message = $"You are NOT on the company network! Your Local IP: {userIp}" });
		}
		public ActionResult HRLogin()
		{
			return View();
		}
		//[HttpPost]
		//public async Task<IActionResult> HrLogin(Hr model)
		//{
		//	if (!ModelState.IsValid)
		//		return View(model);

		//	var hr = await _context.Hrs
		//		.FirstOrDefaultAsync(h => h.Email == model.Email && h.PasswordHash == model.PasswordHash);

		//	if (hr == null)
		//	{
		//		ModelState.AddModelError("", "Invalid email or password");
		//		return View(model);
		//	}


		//	HttpContext.Session.SetString("UserLoggedIn", "true");
		//	HttpContext.Session.SetInt32("HrID", hr.Id);
		//	HttpContext.Session.SetString("HrName", hr.Name ?? ""); // تجنب الخطأ إذا كانت null
		//	HttpContext.Session.SetString("HrEmail", hr.Email ?? "");
		//	HttpContext.Session.SetString("HrImage", hr.Image ?? "");

		//	return RedirectToAction("Dashboard", "Hrs");
		//}


		//-------ManagerLogin------

		public ActionResult ManagerLogin()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> ManagerLogin(Manager model)
		{
			string userIp = GetLocalIPAddress();

			Console.WriteLine($"User Local IP: {userIp}");

			if (userIp == "192.168.1.124")
			{
				if (!ModelState.IsValid)
					return View(model);

				var manager = await _context.Managers
					.FirstOrDefaultAsync(m => m.Email == model.Email && m.PasswordHash == model.PasswordHash);

				if (manager == null)
				{
					ModelState.AddModelError("", "Invalid email or password");
					return View(model);
				}


				HttpContext.Session.SetString("UserLoggedIn", "true");
				HttpContext.Session.SetInt32("ManagerID", manager.Id);
				HttpContext.Session.SetString("ManagerName", manager.Name ?? "");
				HttpContext.Session.SetString("ManagerEmail", manager.Email ?? "");
				HttpContext.Session.SetInt32("ManagerDepartmentID", manager.DepartmentId ?? 0); // التعامل مع null
				HttpContext.Session.SetString("ManagerImage", manager.Image ?? "");

				return RedirectToAction("Show_Employee", "Manager");
				//return Json(new { success = true, message = $"You are connected. Your Local IP: {userIp}" });
			}

			return Json(new { success = false, message = $"You are NOT on the company network! Your Local IP: {userIp}" });
		}

		//[HttpPost]
		//public async Task<IActionResult> ManagerLogin(Manager model)
		//{
		//	if (!ModelState.IsValid)
		//		return View(model);

		//	var manager = await _context.Managers
		//		.FirstOrDefaultAsync(m => m.Email == model.Email && m.PasswordHash == model.PasswordHash);

		//	if (manager == null)
		//	{
		//		ModelState.AddModelError("", "Invalid email or password");
		//		return View(model);
		//	}


		//	HttpContext.Session.SetString("UserLoggedIn", "true");
		//	HttpContext.Session.SetInt32("ManagerID", manager.Id);
		//	HttpContext.Session.SetString("ManagerName", manager.Name ?? "");
		//	HttpContext.Session.SetString("ManagerEmail", manager.Email ?? "");
		//	HttpContext.Session.SetInt32("ManagerDepartmentID", manager.DepartmentId ?? 0); // التعامل مع null
		//	HttpContext.Session.SetString("ManagerImage", manager.Image ?? "");

		//	return RedirectToAction("Show_Employee", "Manager");
		//}


		//-------EmployeeLogin------
		public ActionResult EmployeeLogin()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> EmployeeLogin(Employee model)
		{
			string userIp = GetLocalIPAddress();

			Console.WriteLine($"User Local IP: {userIp}");

			if (userIp == "192.168.1.124")
			{
				if (!ModelState.IsValid)
					return View(model);

				var employee = await _context.Employees
					.FirstOrDefaultAsync(e => e.Email == model.Email && e.PasswordHash == model.PasswordHash);

				if (employee == null)
				{
					ModelState.AddModelError("", "Invalid email or password");
					return View(model);
				}


				HttpContext.Session.SetString("UserLoggedIn", "true");
				HttpContext.Session.SetInt32("EmployeeID", employee.Id);
				HttpContext.Session.SetString("EmployeeName", employee.Name ?? "");
				HttpContext.Session.SetString("EmployeeEmail", employee.Email ?? "");
				HttpContext.Session.SetString("EmployeeImage", employee.ImagePath ?? "");
				HttpContext.Session.SetString("EmployeeAddress", employee.Address ?? "");
				HttpContext.Session.SetString("EmployeePosition", employee.Position ?? "");

				if (employee.ManagerId.HasValue)
					HttpContext.Session.SetInt32("ManagerID", employee.ManagerId.Value);

				if (employee.DepartmentId.HasValue)
					HttpContext.Session.SetInt32("DepartmentID", employee.DepartmentId.Value);

				return RedirectToAction("Index", "EmployeeAttend");
				//return Json(new { success = true, message = $"You are connected. Your Local IP: {userIp}" });
			}

			return Json(new { success = false, message = $"You are NOT on the company network! Your Local IP: {userIp}" });
		}
		//[HttpPost]
		//public async Task<IActionResult> EmployeeLogin(Employee model)
		//{
		//	if (!ModelState.IsValid)
		//		return View(model);

		//	var employee = await _context.Employees
		//		.FirstOrDefaultAsync(e => e.Email == model.Email && e.PasswordHash == model.PasswordHash);

		//	if (employee == null)
		//	{
		//		ModelState.AddModelError("", "Invalid email or password");
		//		return View(model);
		//	}


		//	HttpContext.Session.SetString("UserLoggedIn", "true");
		//	HttpContext.Session.SetInt32("EmployeeID", employee.Id);
		//	HttpContext.Session.SetString("EmployeeName", employee.Name ?? "");
		//	HttpContext.Session.SetString("EmployeeEmail", employee.Email ?? "");
		//	HttpContext.Session.SetString("EmployeeImage", employee.ImagePath ?? "");
		//	HttpContext.Session.SetString("EmployeeAddress", employee.Address ?? "");
		//	HttpContext.Session.SetString("EmployeePosition", employee.Position ?? "");

		//	if (employee.ManagerId.HasValue)
		//		HttpContext.Session.SetInt32("ManagerID", employee.ManagerId.Value);

		//	if (employee.DepartmentId.HasValue)
		//		HttpContext.Session.SetInt32("DepartmentID", employee.DepartmentId.Value);

		//	return RedirectToAction("Index", "EmployeeAttend");
		//}

		public IActionResult Logout()
		{

			HttpContext.Session.Remove("UserLoggedIn");
			return RedirectToAction("Index", "Home");

		}


		// GET: LoginController/Details/5
		public ActionResult Details(int id)
		{
			return View();
		}


		// --------- ForgotPassword ---------
		public ActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			if (!string.IsNullOrEmpty(email))
			{
				// تخزين البريد الإلكتروني في Session
				HttpContext.Session.SetString("ResetEmail", email);

				// تحقق مما إذا كان البريد موجودًا في قاعدة البيانات
				var hr = _context.Hrs.FirstOrDefault(h => h.Email == email);
				var manager = _context.Managers.FirstOrDefault(m => m.Email == email);
				var employee = _context.Employees.FirstOrDefault(e => e.Email == email);

				if (hr != null || manager != null || employee != null)
				{
					// إنشاء رابط إعادة التعيين
					string resetLink = Url.Action("ResetPassword", "Login", null, Request.Scheme);
					string message = $"Click <a href='{resetLink}'>here</a> to reset your password.";

					await _emailService.SendEmailAsync(email, "Reset Your Password", message);
					ViewBag.Message = "Password reset email has been sent!";
				}
				else
				{
					ViewBag.Error = "Email not found in our system.";
				}
			}

			return View();
		}




		//--------Reset password ---------

		[HttpGet]
		public IActionResult ResetPassword()
		{
			// استرجاع البريد الإلكتروني من السيشن
			string email = HttpContext.Session.GetString("ResetEmail");

			if (string.IsNullOrEmpty(email))
			{
				return RedirectToAction("ForgotPassword"); // إذا لم يكن هناك بريد مخزن، أعده إلى صفحة نسيان كلمة المرور
			}

			return View();
		}

		[HttpPost]
		public IActionResult ResetPassword(string newPassword)
		{
			string email = HttpContext.Session.GetString("ResetEmail");

			if (!string.IsNullOrEmpty(email))
			{
				// البحث عن المستخدم في قاعدة البيانات
				var hr = _context.Hrs.FirstOrDefault(h => h.Email == email);
				var manager = _context.Managers.FirstOrDefault(m => m.Email == email);
				var employee = _context.Employees.FirstOrDefault(e => e.Email == email);

				if (hr != null)
				{
					hr.PasswordHash = newPassword; // تحديث كلمة المرور
				}
				else if (manager != null)
				{
					manager.PasswordHash = newPassword;
				}
				else if (employee != null)
				{
					employee.PasswordHash = newPassword;
				}
				else
				{
					ViewBag.Error = "User not found.";
					return View();
				}

				_context.SaveChanges(); // حفظ التغييرات في قاعدة البيانات
				HttpContext.Session.Remove("ResetEmail"); // مسح البريد من السيشن بعد الاستخدام

				ViewBag.Message = "Your password has been reset successfully!";
			}
			return RedirectToAction(nameof(Index));
		}


		// GET: LoginController/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: LoginController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: LoginController/Edit/5
		public ActionResult Edit(int id)
		{
			return View();
		}

		// POST: LoginController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int id, IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}

		// GET: LoginController/Delete/5
		public ActionResult Delete(int id)
		{
			return View();
		}

		// POST: LoginController/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return View();
			}
		}
	}
}
