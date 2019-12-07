using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;

        public CartController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        [BindProperty]
        public OrderDetailsCart DetailsCart { get; set; }
        public async Task<IActionResult> Index()
        {
            DetailsCart = new OrderDetailsCart()
            {
                orderHeader = new OrderHeader()
            };

            DetailsCart.orderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if(cart != null)
            {
                DetailsCart.listCart = cart.ToList();
            }

            foreach(var list in DetailsCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.id == list.MenuItemId);
                DetailsCart.orderHeader.OrderTotal = DetailsCart.orderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);

                if(list.MenuItem.Description.Length > 100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            DetailsCart.orderHeader.OrderTotalOriginal = DetailsCart.orderHeader.OrderTotal;

            if(HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                DetailsCart.orderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == DetailsCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                DetailsCart.orderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, DetailsCart.orderHeader.OrderTotalOriginal);
            }

            return View(DetailsCart);
        }

        public IActionResult AddCoupon()
        {
            if(DetailsCart.orderHeader.CouponCode == null)
            {
                DetailsCart.orderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, DetailsCart.orderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            if(cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32("ssCartCount", cnt);
            } else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32("ssCartCount", cnt);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            DetailsCart = new OrderDetailsCart()
            {
                orderHeader = new OrderHeader()
            };

            DetailsCart.orderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);

            if(cart != null)
            {
                DetailsCart.listCart = cart.ToList();
            }

            foreach(var list in DetailsCart.listCart)
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.id == list.MenuItemId);
                DetailsCart.orderHeader.OrderTotal = DetailsCart.orderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                
            }

            DetailsCart.orderHeader.OrderTotalOriginal = DetailsCart.orderHeader.OrderTotal;
            DetailsCart.orderHeader.PickupName = applicationUser.Name;
            DetailsCart.orderHeader.PhoneNumber = applicationUser.PhoneNumber;
            DetailsCart.orderHeader.PickUpTime = DateTime.Now;

            if(HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                DetailsCart.orderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == DetailsCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                DetailsCart.orderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, DetailsCart.orderHeader.OrderTotalOriginal);
            }
            return View(DetailsCart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            DetailsCart.listCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            DetailsCart.orderHeader.PaymentStatus = SD.PaymentStatusPending;
            DetailsCart.orderHeader.OrderDate = DateTime.Now;
            DetailsCart.orderHeader.UserId = claim.Value;
            DetailsCart.orderHeader.Status = SD.StatusPending;
            DetailsCart.orderHeader.PickUpTime = Convert.ToDateTime(DetailsCart.orderHeader.PickUpDate.ToShortDateString() + " " + DetailsCart.orderHeader.PickUpTime.ToShortTimeString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            _db.OrderHeader.Add(DetailsCart.orderHeader);
            await _db.SaveChangesAsync();

            DetailsCart.orderHeader.OrderTotalOriginal = 0;

            foreach(var item in DetailsCart.listCart)
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = DetailsCart.orderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                DetailsCart.orderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetail.Add(orderDetails);
            }

            if(HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                DetailsCart.orderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == DetailsCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                DetailsCart.orderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, DetailsCart.orderHeader.OrderTotalOriginal);
            } else
            {
                DetailsCart.orderHeader.OrderTotal = DetailsCart.orderHeader.OrderTotalOriginal;
            }
            DetailsCart.orderHeader.CouponCodeDiscount = DetailsCart.orderHeader.OrderTotalOriginal - DetailsCart.orderHeader.OrderTotal;
            await _db.SaveChangesAsync();

            _db.ShoppingCart.RemoveRange(DetailsCart.listCart);
            HttpContext.Session.SetInt32("ssCartCount", 0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(DetailsCart.orderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID : " + DetailsCart.orderHeader.Id,
                SourceId = stripeToken
            };

            var service = new ChargeService();
            Charge charge = service.Create(options);

            if(charge.BalanceTransactionId == null)
            {
                DetailsCart.orderHeader.Status = SD.PaymentStatusRejected;
            } 
            else
            {
                DetailsCart.orderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if(charge.Status.ToLower() == "succeeded")
            {
                // email for successful order
                await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email, "Spice - Order Created " + DetailsCart.orderHeader.Id.ToString(), "Order has been submitted successfully.");

                DetailsCart.orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                DetailsCart.orderHeader.Status = SD.StatusSubmitted;
            } 
            else
            {
                DetailsCart.orderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }

            await _db.SaveChangesAsync();
           // return RedirectToAction("Index", "Home");
              return RedirectToAction("Confirm", "Order", new { id = DetailsCart.orderHeader.Id });
        }

        

    }
}