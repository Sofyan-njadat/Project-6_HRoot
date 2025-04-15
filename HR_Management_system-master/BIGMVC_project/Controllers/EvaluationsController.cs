using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BIGMVC_project.Models;

namespace BIGMVC_project.Controllers
{
	public class EvaluationsController : Controller
	{
		private readonly MyDbContext _context;

		public EvaluationsController(MyDbContext context)
		{
			_context = context;
		}

		// GET: Evaluations
		public async Task<IActionResult> Index()
		{
			var myDbContext = _context.Evaluations.Include(e => e.Employee).Include(e => e.Manager);
			return View(await myDbContext.ToListAsync());
		}

		// GET: Evaluations/Details/5
		public async Task<IActionResult> Details(int? id)
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

		// GET: Evaluations/Create
		public IActionResult Create()
		{
			ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
			ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Id");
			return View();
		}

		// POST: Evaluations/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,EmployeeId,ManagerId,EvaluationsStatusEnum,Comments,DateEvaluated")] Evaluation evaluation)
		{
			if (ModelState.IsValid)
			{
				_context.Add(evaluation);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", evaluation.EmployeeId);
			ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Id", evaluation.ManagerId);
			return View(evaluation);
		}

		// GET: Evaluations/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var evaluation = await _context.Evaluations.FindAsync(id);
			if (evaluation == null)
			{
				return NotFound();
			}
			ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", evaluation.EmployeeId);
			ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Id", evaluation.ManagerId);
			return View(evaluation);
		}

		// POST: Evaluations/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,ManagerId,EvaluationsStatusEnum,Comments,DateEvaluated")] Evaluation evaluation)
		{
			if (id != evaluation.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(evaluation);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!EvaluationExists(evaluation.Id))
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
			ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", evaluation.EmployeeId);
			ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Id", evaluation.ManagerId);
			return View(evaluation);
		}

		// GET: Evaluations/Delete/5
		public async Task<IActionResult> Delete(int? id)
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

		// POST: Evaluations/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var evaluation = await _context.Evaluations.FindAsync(id);
			if (evaluation != null)
			{
				_context.Evaluations.Remove(evaluation);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool EvaluationExists(int id)
		{
			return _context.Evaluations.Any(e => e.Id == id);
		}
	}
}
