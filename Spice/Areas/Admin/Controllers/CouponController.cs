using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Spice.Utility.SD.ManagerUser)]
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if(files.Count > 0)
                {
                    byte[] p1 = null; 
                    using(var fs1 = files[0].OpenReadStream())
                    {
                        using(var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }
                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var couponDb = await _db.Coupon.FindAsync(id);

            if(couponDb == null)
            {
                return NotFound();
            }

            return View(couponDb);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if(coupon.Id == 0)
            {
                return NotFound();
            }

            var couponFromDb = await _db.Coupon.Where(c => c.Id == coupon.Id).FirstOrDefaultAsync();

            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;

                if(files.Count > 0)
                {
                    byte[] p1 = null;

                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;

                }
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.Name = coupon.Name;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.CouponType = coupon.CouponType;
                couponFromDb.IsActive = coupon.IsActive;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET - Detail
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var couponDb = await _db.Coupon.FindAsync(id);

            if (couponDb == null)
            {
                return NotFound();
            }

            return View(couponDb);

        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var couponDb = await _db.Coupon.FirstOrDefaultAsync(m => m.Id == id);

            if(couponDb == null)
            {
                return NotFound();
            }

            return View(couponDb);
        }


        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
           
                var couponFromDb = await _db.Coupon.FirstOrDefaultAsync(c => c.Id == id);
                _db.Coupon.Remove(couponFromDb);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
          
        }
            

    }
}