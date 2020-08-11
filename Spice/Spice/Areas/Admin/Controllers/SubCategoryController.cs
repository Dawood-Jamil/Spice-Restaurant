using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    public class SubCategoryController : Controller
    {

        [TempData]
        public string StatusMessage { get; set; }

        private readonly ApplicationDbContext _db;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var Categories = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(Categories);
        }

        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(m => m.Name).Distinct().ToListAsync()

            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryEsists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryEsists.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category exists under" + doesSubCategoryEsists.First().Category.Name + "category.Please Use another Name";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }

            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);

        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories  = await _db.SubCategory.Where(s => s.CategoryId == id).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                NotFound();
            }
            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(m => m.Name).Distinct().ToListAsync()

            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (ModelState.IsValid)
            {
                var doesSubCategoryEsists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryEsists.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category exists under" + doesSubCategoryEsists.First().Category.Name + "category.Please Use another Name";
                }
                else
                {

                    subCategory.Name = model.SubCategory.Name;
                    subCategory.CategoryId = model.SubCategory.CategoryId;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            modelVM.SubCategory.Id = id;
            return View(modelVM);

        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                NotFound();
            }
            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(m => m.Name).Distinct().ToListAsync()

            };

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                NotFound();
            }
            SubCategoryAndCategoryViewModel viewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(s => s.Name).Select(m => m.Name).Distinct().ToListAsync()

            };

            return View(viewModel);
        }
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                NotFound();
            }
            _db.SubCategory.Remove(subCategory);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
