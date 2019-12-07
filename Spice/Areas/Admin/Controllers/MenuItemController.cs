using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webhostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webhostingEnvironment)
        {
            _db = db;
            _webhostingEnvironment = webhostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        // GET - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["MenuItem.SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            // Work on the image saving section

            string webRootPath = _webhostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.id);

            if(files.Count > 0)
            {
                // files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using(var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.id + extension;
            }
            else
            {
                // no file was uploaded, so use default
                menuItemFromDb.Image = @"\images\" + SD.DefaultFoodImage;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        // GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if(MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["MenuItem.SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            // Work on the image saving section

            string webRootPath = _webhostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.id);

            if(files.Count > 0)
            {
                // New Image has been  uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                // Delete the original file
                var originalImageName = menuItemFromDb.Image.TrimStart('\\');
                if(originalImageName != "images\\" + SD.DefaultFoodImage)
                {
                    var imagePath = Path.Combine(webRootPath, originalImageName);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                using (var filesstream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesstream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.id + extension_new;

            }

            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }


        // GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }


        // GET - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }


        // POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            string webRootPath = _webhostingEnvironment.WebRootPath;
           

            var menuItemDb = await _db.MenuItem.FindAsync(id);

            var originalImageName = menuItemDb.Image.TrimStart('\\');
            if (originalImageName != "images\\" + SD.DefaultFoodImage)
            {
                var imagePath = Path.Combine(webRootPath, originalImageName);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _db.MenuItem.Remove(menuItemDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}