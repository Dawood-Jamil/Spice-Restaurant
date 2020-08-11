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
    [Area("Admin")]
    public class CoupanController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoupanController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupan.ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Coupan coupan)
        {
            if (ModelState.IsValid)
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
                    coupan.Picture = p1;
                }

                _db.Coupan.Add(coupan);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var coupanFromDb =await _db.Coupan.SingleOrDefaultAsync(c => c.Id == id);
            return View(coupanFromDb);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditPost(int? id,Coupan coupan)
        {
            if (id == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                var coupanFromDb =await _db.Coupan.SingleOrDefaultAsync(c => c.Id == id);
                coupanFromDb.Name = coupan.Name;
                coupanFromDb.CouponType = coupan.CouponType;
                coupanFromDb.Discount = coupan.Discount;
                coupanFromDb.MinimumAmount = coupan.MinimumAmount;
                coupanFromDb.isActive = coupan.isActive;

                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
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
                    coupanFromDb.Picture = p1;
                }
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupan);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var coupanFromDb = await _db.Coupan.SingleOrDefaultAsync(c => c.Id == id);
            return View(coupanFromDb);
            
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var coupanFromDb = await _db.Coupan.SingleOrDefaultAsync(c => c.Id == id);
            return View(coupanFromDb);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null)
                return NotFound();

            var coupanFromDb = await _db.Coupan.SingleOrDefaultAsync(c => c.Id == id);
            _db.Coupan.Remove(coupanFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
