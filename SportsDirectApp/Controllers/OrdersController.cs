using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SportsDirectApp.Common;
using SportsDirectApp.Data;
using SportsDirectApp.Models;

namespace SportsDirectApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Smtp _smtp;
        private readonly Common.Config _config;


        public OrdersController(ApplicationDbContext context, IOptions<Smtp> smtp, IOptions<Common.Config> config)
        {
            _context = context;
            _smtp = smtp.Value;
            _config = config.Value;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Order.OrderByDescending(o => o.Id)
                .Include(o => o.Shop)
                .ToListAsync());
        }

        //Toggle order status
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var order = await _context.Order.SingleOrDefaultAsync(o => o.Id == id);
            order.IsOpen = !order.IsOpen;

            order.SetEditProperties(HttpContext.User.Identity.Name);

            ModelState.Clear();
            TryValidateModel(order);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            return View(order);
        }

        // GET: Shops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Shop)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Items(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            var shopList = GetShopList();
            ViewBag.Shops = shopList;

            var currencyList = GetCurrencyList();
            ViewBag.Currencies = currencyList;

            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsOpen,ShopId, Currency, Shipping")] Order order)
        {
            order.SetCreateProperties(HttpContext.User.Identity.Name);
            ModelState.Clear();
            TryValidateModel(order);
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                Shop shop = _context.Shop.FirstOrDefault(s => s.Id == order.ShopId);

                if (shop != null && _config.NewOrderNodificationEnabled)
                {
                    await SendNewOrderMail(order, shop);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            var shopList = GetShopList();
            ViewBag.Shops = shopList;

            var currencyList = GetCurrencyList();
            ViewBag.Currencies = currencyList;

            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order orderEdit)
        {
            if (id != orderEdit.Id)
            {
                return NotFound();
            }

            var order = await _context.Order.SingleOrDefaultAsync(o => o.Id == id);

            order.Name = orderEdit.Name;
            order.Description = orderEdit.Description;
            order.IsOpen = orderEdit.IsOpen;
            order.ShopId = orderEdit.ShopId;
            order.Currency = orderEdit.Currency;
            order.Shipping = orderEdit.Shipping;

            order.SetEditProperties(HttpContext.User.Identity.Name);

            ModelState.Clear();
            TryValidateModel(order);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            return View(order);
        }

        // GET: Orders1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .SingleOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.SingleOrDefaultAsync(m => m.Id == id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        public ActionResult Calculation(int? id)
        {
            if (id > 0)
            {
                return View(id);
            }

            return BadRequest();
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }

        #region Private Methods
        private List<SelectListItem> GetShopList()
        {
            var shopListDb = _context.Shop.OrderBy(s => s.Name).ToList();

            var shopList = shopListDb
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            shopList.Insert(0, new SelectListItem { Text = "- Select Shop -", Value = "" });
            return shopList;
        }

        private List<SelectListItem> GetCurrencyList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem {Text = Constants.Currency.HRK},
                new SelectListItem {Text = Constants.Currency.EUR},
                new SelectListItem {Text = Constants.Currency.GBP},
                new SelectListItem {Text = Constants.Currency.USD}
            };

            return items;
        }

        private async Task SendNewOrderMail(Order order, Shop shop)
        {
            IEnumerable<string> emails = _context.Users.ToList().Where(u => u.UserName != "admin").Select(u => u.Email);
            StringBuilder body = new StringBuilder();
            body.Append("<b>New order has been created!</b>" + "</br>" + "</br>");
            body.Append("Shop: " + "<b>" + shop.Name + "</b>" + "</br>");
            body.Append("Order name: " + "<b>" + order.Name + "</b>" + "</br>");
            body.Append("Created by: " + "<b>" + order.CreatedBy + "</b>" + "</br> </br>");
            body.Append("<b>Visit orderačina here:</b>" + "</br>" + "<a href=" + _config.NewOrderLink + order.Id + " target=\"_blank\">" + _config.NewOrderLink + order.Id + "</a>");

            var smtp = Utility.GetMailClient(_smtp);
            var message = new MailMessage()
            {
                Subject = "New Orderačina!",
                Body = body.ToString(),
                From = new MailAddress(_smtp.Address, _smtp.Username),
            };
            foreach (var item in emails)
            {
                message.To.Add(item);
            }
            message.IsBodyHtml = true;
            await smtp.SendMailAsync(message);
        }
        #endregion
    }
}
