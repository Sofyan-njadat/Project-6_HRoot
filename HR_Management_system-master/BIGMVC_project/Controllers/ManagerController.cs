using BIGMVC_project.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BIGMVC_project.Controllers
{
	//----------------
	public class ManagerController : Controller
	{
		private readonly MyDbContext _context;
		public ManagerController(MyDbContext context)
		{
			_context = context;
		}

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


		public IActionResult Index()
		{
			string userIp = GetLocalIPAddress();

			Console.WriteLine($"User Local IP: {userIp}");

			if (userIp == "192.168.1.13")
			{
				var managerId = HttpContext.Session.GetString("ID");

				if (managerId == null)
				{
					return RedirectToAction("Login");
				}

				int managerIdInt = int.Parse(managerId);
				Console.WriteLine("Manager ID: " + managerIdInt);

				var employees = _context.Employees.Where(e => e.ManagerId == managerIdInt).ToList();

				ViewBag.Employees = employees;
				return View(employees);
			}

			return Json(new { success = false, message = $"You are NOT on the company network! Your Local IP: {userIp}" });
			
		}
		public IActionResult Addemployee()
		{
			ViewBag.depname = _context.Departments.Select(e => new { e.Id, e.Name }).ToList();
			return View();
		}
		[HttpPost]
		//[ValidateAntiForgeryToken]
		public IActionResult Addemployee(Employee employees)
		{
			var findd = _context.Employees.FirstOrDefault(m => m.Email == employees.Email);
			if (findd != null)
			{
				ViewBag.errors = "This Employee is already Exist";
				return View();
			}
			//var con = _context.Departments.Where(m => m.Id == employees.DepartmentId).ToList();
			//HttpContext.Session.SetInt32("iddep", );
			if (ModelState.IsValid)
			{
				employees.ImagePath = "https://i.pinimg.com/474x/ad/4d/39/ad4d39691e21a7d805eaca3b90ab8554.jpg";
				_context.Add(employees);
				_context.SaveChanges();
				return View(); //RedirectToAction("Index")
			}
			//ViewBag.depname = _context.Departments.Select(e => new { e.Id, e.Name }).ToList();
			return View();
		}
		[HttpGet]
		public IActionResult LeavingR()
		{
			var data = _context.LeaveRequests.ToList();
			return View(data);
		}
		[HttpPost]
		public IActionResult LeavingR(int id, string status)
		{
			var findd = _context.LeaveRequests.Find(id);
			var condd = _context.Employees.Where(m => m.Id == id);
			//var findemp = _context.Employees.Find(findd.Id);
			if (findd != null)
			{
				findd.LeaveRequestsStatusEnum = status;
				_context.LeaveRequests.Update(findd);
				_context.SaveChanges();
			}
			return RedirectToAction("LeavingR");
		}
		public IActionResult contact()
		{
			return View();
		}
		[HttpPost]
		public IActionResult contact(Feedback feedbacks)
		{
			if (ModelState.IsValid)
			{
				_context.Add(feedbacks);
				_context.SaveChanges();
				return View();
			}
			return View();
		}

		public IActionResult About_us()
		{
			return View();
		}
		public IActionResult Show_Employee()
		{
			var idd = HttpContext.Session.GetInt32("ManagerID");
			HttpContext.Session.GetString("img");
			var getemp = _context.Employees.Where(e => e.ManagerId == idd).ToList();
			ViewBag.Employees = getemp;
			return View(getemp);
		}
		public IActionResult Questions()
		{
			return View();
		}
		//contact هي نفسها صفحه الهووووممممممممممممممممممممممممممممممممم
		[HttpPost]
		public IActionResult Questions(IFormCollection form,Evaluation ev, int id)
		{
			// Process the form data
			int q1 = int.Parse(form["q1"]);
			int q2 = int.Parse(form["q2"]);
			int q3 = int.Parse(form["q3"]);
			int q4 = int.Parse(form["q4"]);
			int q5 = int.Parse(form["q5"]);
			int q6 = int.Parse(form["q6"]);
			int q7 = int.Parse(form["q7"]);
			int q8 = int.Parse(form["q8"]);
			int q9 = int.Parse(form["q9"]);
			int q10 = int.Parse(form["q10"]);

			int totalScore = q1 + q2 + q3 + q4 + q5 + q6 + q7 + q8 + q9 + q10;
			//totalScore = Math.Max(0, Math.Min(10, totalScore));

			// Determine evaluation result
			string result;
			if (totalScore >= 7)
			{
				result = "excellent";
			}
			else if (totalScore >= 4)
			{
				result = "good";
			}
			else
			{
				result = "bad";
			}

			// Update the evaluation result in the database
			

			if (result != null)
			{
				ev.Id = 0;
				ev.EmployeeId = id;
				ev.ManagerId = 1;
				ev.EvaluationsStatusEnum = result;
				//ev.DateEvaluated = DateTime.Today();
		 // Assuming the column name is "EvaluationResult"
				_context.Evaluations.Add(ev);
				_context.SaveChanges();
			}

			// Pass the result to the view
			ViewBag.TotalScore = totalScore;

			return View("Addemployee");
		}
		public IActionResult AssignTasks(int id, int a)
		{
			HttpContext.Session.SetInt32("idempNew", id);

			HttpContext.Session.SetString("EmployeeName", _context.Employees.FirstOrDefault(x => x.Id == id).Name);
			return View();
		}
		[HttpPost]
		public IActionResult AssignTasks(int id)
		{
			var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

			if (employee == null)
			{
				// إذا لم يتم العثور على الموظف، يمكنك إعادة التوجيه أو عرض رسالة خطأ
				return RedirectToAction("Index");
			}
			HttpContext.Session.SetInt32("idemp", employee.Id);
			HttpContext.Session.SetString("nameemp",employee.Name);
			// إرسال معلومات الموظف إلى الصفحة
			//TempData["Employee"] = employee;
			return View();
		}


		[HttpPost]
		public IActionResult CreateTask(Mission task, int Employeeid)
		{
		
			var employee = _context.Employees.FirstOrDefault(e => e.Id == Convert.ToInt32(HttpContext.Session.GetInt32("idempNew")));

			if (employee == null)
			{
				ModelState.AddModelError("", "Invalid EmployeeId.");
				return View("AssignTasks");
			}
			

			task.EmployeeId = Convert.ToInt32(HttpContext.Session.GetInt32("idempNew")); ;  // تعيين EmployeeId للمهمة
			_context.Add(task);
			_context.SaveChanges();

			return RedirectToAction("Show_Employee", "Manager" ) ;
		}


		[HttpGet]
		public IActionResult EmployeeTasks(int id) // غيرت EmployeeId لـ id عشان يتماشى مع الرابط
		{
			var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

			if (employee == null)
			{
				return RedirectToAction("Index");
			}

			var tasks = _context.Missions.Where(t => t.EmployeeId == id).ToList();

			ViewBag.Employee = employee;
			return View(tasks);
		}


		[HttpGet]
		public IActionResult EmployeeAttendances(int id) // غيرت EmployeeId لـ id عشان يتماشى مع الرابط
		{
			var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

			if (employee == null)
			{
				return RedirectToAction("Index");
			}

			var Attendancs = _context.Attendances.Where(t => t.EmployeeId == id).ToList();

			ViewBag.Employee = employee;
			return View(Attendancs);
		}
		public IActionResult Profile()
		{
			var idds = HttpContext.Session.GetInt32("ManagerID");
			var emailM = HttpContext.Session.GetString("Email");
			var manager = _context.Managers.Find(idds);
			return View(manager);
		}
		public IActionResult EditProfile()
		{
			var emailM = HttpContext.Session.GetString("ManagerEmail");
			if (string.IsNullOrEmpty(emailM))
			{
				return RedirectToAction("Profile", "Manager");
			}

			var manager = _context.Managers.FirstOrDefault(x => x.Email == emailM);
			if (manager == null)
			{
				return RedirectToAction("Login", "Manager");
			}

			return View(manager);
		}
		[HttpPost]
		public IActionResult HandleEditProfile(Manager m, IFormFile ImageFile)
		{
			var emailM = HttpContext.Session.GetString("ManagerEmail");
			var existingManager = _context.Managers.FirstOrDefault(x => x.Email == emailM);

			if (existingManager != null)
			{
				// تحديث البيانات مع الحفاظ على القيم الحالية إذا لم يتم إدخال قيم جديدة
				existingManager.Name = m.Name ?? existingManager.Name;
				existingManager.Email = m.Email ?? existingManager.Email;
				existingManager.PasswordHash = m.PasswordHash ?? existingManager.PasswordHash;

				// التعامل مع الصورة الشخصية
				if (ImageFile != null)
				{
					string fileName = Path.GetFileName(ImageFile.FileName);
					string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

					using (var stream = new FileStream(path, FileMode.Create))
					{
						ImageFile.CopyTo(stream);
					}
					existingManager.Image = fileName;
					//m.Image = fileName;
					HttpContext.Session.SetString("Img", existingManager.Image);
				}

				_context.Managers.Update(existingManager);
				_context.SaveChanges();

				return RedirectToAction("Show_Employee", "Manager");
			}

			return RedirectToAction("Login", "Manager");
		}
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Login");
		}
		public IActionResult ContactUs()
		{
			return View();
		}
		public IActionResult Services()
		{
			return View();
		}
	}


}

