using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using Stripe;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }

        public async Task<IActionResult> Index()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),

            };

            detailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            if(cart.Count> 0)
            {
                detailCart.listCart = cart;
            }

            foreach(var list  in detailCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MeuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                if(list.MenuItem.Description != null)
                {
                    list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);

                    if (list.MenuItem.Description.Length > 100)
                    {
                        list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                    }
                }
              
            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;

            if(HttpContext.Session.GetString(SD.ssCoupanCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCoupanCode);
                var coupanFromDb = await _db.Coupan.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(coupanFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            return View(detailCart);
        }

        public IActionResult AddCoupon()
        {
            if(detailCart.OrderHeader.CouponCode == null)
            {
                detailCart.OrderHeader.CouponCode = "";

            }
            HttpContext.Session.SetString(SD.ssCoupanCode, detailCart.OrderHeader.CouponCode);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(SD.ssCoupanCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),

            };

            detailCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser =await _db.ApplicationUser.Where(u => u.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            if (cart.Count > 0)
            {
                detailCart.listCart = cart;
            }

            foreach (var list in detailCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MeuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);

            }
            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal;
            detailCart.OrderHeader.PickupName = applicationUser.Name;
            detailCart.OrderHeader.PhoneNumber = applicationUser.Phone;
            detailCart.OrderHeader.PickUpTime = DateTime.Now;




            if (HttpContext.Session.GetString(SD.ssCoupanCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCoupanCode);
                var coupanFromDb = await _db.Coupan.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(coupanFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            return View(detailCart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            detailCart.listCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailCart.OrderHeader.OrderDate = DateTime.Now;
            detailCart.OrderHeader.UserId = claim.Value;
            detailCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailCart.OrderHeader.PickUpTime.ToShortTimeString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailCart.OrderHeader.OrderTotalOriginal = 0;


            foreach (var item in detailCart.listCart)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MeuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MeuItemId,
                    OrderId = detailCart.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                detailCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.orderDetails.Add(orderDetails);

            }

            if (HttpContext.Session.GetString(SD.ssCoupanCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCoupanCode);
                var couponFromDb = await _db.Coupan.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotalOriginal;
            }
            detailCart.OrderHeader.CouponCodeDiscount = detailCart.OrderHeader.OrderTotalOriginal - detailCart.OrderHeader.OrderTotal;

            _db.ShoppingCart.RemoveRange(detailCart.listCart);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(detailCart.OrderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID : " + detailCart.OrderHeader.Id,
                Source = stripeToken

            };
            ////var service = new ChargeService();
            ////Charge charge = service.Create(options);

            //if (charge.BalanceTransactionId == null)
            //{
            //    detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            //}
            //else
            //{
            //    detailCart.OrderHeader.TransactionId = charge.BalanceTransactionId;
            //}

            //if (charge.Status.ToLower() == "succeeded")
            //{
            //    detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
            //    detailCart.OrderHeader.Status = SD.StatusSubmitted;
            //}
            //else
            //{
            //    detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            //}

            await _db.SaveChangesAsync();
          //  return RedirectToAction("Index", "Home");
            return RedirectToAction("Confirm", "Order", new { id = detailCart.OrderHeader.Id });

        }

    }
}