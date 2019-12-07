using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return View(await _db.ApplicationUser.Where(u => u.Id != claims.Value).ToListAsync());
        }

        public async Task<IActionResult> Lock(string id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if(applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.LockoutEnd = DateTime.Now.AddYears(100);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UnLock(string id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var ApplicationDbUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if(ApplicationDbUser == null)
            {
                return NotFound();
            }

            ApplicationDbUser.LockoutEnd = DateTime.Now;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}