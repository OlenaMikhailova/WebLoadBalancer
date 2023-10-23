using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebLoadBalancer.Models;

namespace WebLoadBalancer.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationContext _context;

        public UserController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(_context.Users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var web_user = await _context.Users
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (web_user == null)
            {
                return NotFound();
            }

            return View(web_user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("user_id,email,user_password,username")] web_user web_user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(web_user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(web_user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var web_user = await _context.Users.FindAsync(id);
            if (web_user == null)
            {
                return NotFound();
            }
            return View(web_user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("user_id,email,user_password,username")] web_user web_user)
        {
            if (id != web_user.user_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(web_user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!web_userExists(web_user.user_id))
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
            return View(web_user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var web_user = await _context.Users
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (web_user == null)
            {
                return NotFound();
            }

            return View(web_user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationContext.Users'  is null.");
            }
            var web_user = await _context.Users.FindAsync(id);
            if (web_user != null)
            {
                _context.Users.Remove(web_user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool web_userExists(int id)
        {
          return (_context.Users?.Any(e => e.user_id == id)).GetValueOrDefault();
        }
    }
}
