using BIGMVC_project.Models;
using Microsoft.AspNetCore.Mvc;

namespace BIGMVC_project.Controllers
{
	public class User1Controller : Controller
	{

		private readonly MyDbContext _context;

		public User1Controller(MyDbContext context)
		{
			_context = context;
		}
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Login(Manager manager)
		{


				var checc = _context.Managers
					.FirstOrDefault(m => m.Email.ToLower() == manager.Email.ToLower()
									  && m.PasswordHash == manager.PasswordHash);

				if (checc != null)
				{
					HttpContext.Session.SetString("IImg", checc.Image);
					HttpContext.Session.SetString("Name", checc.Name);
					HttpContext.Session.SetString("Email", checc.Email);
					HttpContext.Session.SetInt32("Id", checc.Id);
					return RedirectToAction("Show_Employee", "Manager");
				}
				else
				{
					ModelState.AddModelError("", "Invalid Email or Password");
				}
			
			return View();
		}

	}
}
