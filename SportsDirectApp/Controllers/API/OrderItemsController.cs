using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SportsDirectApp.Common;
using SportsDirectApp.Data;
using SportsDirectApp.Models;

namespace SportsDirectApp.Controllers.API
{
    [Produces("application/json")]
    [Route("api/OrderItems")]
    [Authorize]
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Common.Config _config;

        public OrderItemsController(ApplicationDbContext context, IOptions<Common.Config> config)
        {
            _context = context;
            _config = config.Value;
        }

        // GET: api/OrderItems
        [HttpGet]
        public IEnumerable<OrderItem> GetOrderItem()
        {
            return _context.OrderItem;
        }

        [HttpGet("ItemsByOrderId/{id}/{sort}")]
        public IEnumerable<OrderItem> GetOrderItemsByOrderId([FromRoute] int id, string sort)
        {
            IQueryable<OrderItem> result = null;

            switch (sort)
            {

                case ("idDesc"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderByDescending(i => i.Id);
                    break;
                case ("price"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderBy(i => i.Price);
                    break;
                case ("priceDesc"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderByDescending(i => i.Price);
                    break;
                case ("date"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderBy(i => i.DateCreated);
                    break;
                case ("dateDesc"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderByDescending(i => i.DateCreated);
                    break;
                case ("createdBy"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderBy(i => i.CreatedBy);
                    break;
                case ("createdByDesc"):
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderByDescending(i => i.CreatedBy);
                    break;
                default:
                    result = _context.OrderItem.Where(i => i.OrderId == id).OrderBy(i => i.Id);
                    break;
            }

            return result;
        }

        // GET: api/OrderItems/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderItem = await _context.OrderItem.SingleOrDefaultAsync(m => m.Id == id);

            if (orderItem == null)
            {
                return NotFound();
            }

            return Ok(orderItem);
        }

        // GET: api/OrderItems/5
        [HttpGet("CalculationHtml/{orderId}")]
        [Produces("text/html")]
        public ContentResult GetCalculationHtml([FromRoute] int orderId)
        {
            var content = string.Empty;

            if (orderId > 0)
            {
                var order = _context.Order.Include(i => i.OrderItems)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order?.OrderItems != null && order.OrderItems.Any())
                {
                        var tecaj = GetTecaj(order.Currency);

                        var calcCollection = GetCalculationCollection(order, tecaj);

                        try
                        {
                            content = calcCollection.CreateHTMLContent();
                        }
                        catch (Exception ex)
                        {
                            content = ex.Message;
                        }
                }
                else
                {
                    content = "<h1>NO ITEMS - NO CALCULATION!</h1>";
                }
            }

            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
            };
        }

        // PUT: api/OrderItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderItem([FromRoute] int id, [FromBody] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return BadRequest();
            }

            var order = _context.Order.FirstOrDefault(o => o.Id == orderItem.OrderId);

            if (order.IsOpen)
            {
                var orderItemOld = _context.OrderItem.FirstOrDefault(i => i.Id == id);

                if (orderItemOld != null)
                {
                    orderItemOld.Amount = orderItem.Amount;
                    orderItemOld.DateModified = orderItem.DateModified;
                    orderItemOld.Description = orderItem.Description;
                    orderItemOld.ModifiedBy = orderItem.ModifiedBy;
                    orderItemOld.Price = orderItem.Price;
                    orderItemOld.Size = orderItem.Size;
                    orderItemOld.Url = orderItem.Url;


                    ModelState.Clear();
                    TryValidateModel(orderItemOld);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }


                    _context.Entry(orderItemOld).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderItemExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return NoContent();

                }
                else
                {
                    return BadRequest("Item does not exist!");
                }
            }

            return BadRequest("Order is closed!");

        }

        // POST: api/OrderItems
        [HttpPost]
        public async Task<IActionResult> PostOrderItem([FromBody] OrderItem orderItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = _context.Order.FirstOrDefault(o => o.Id == orderItem.OrderId);

            if (order.IsOpen)
            {
                _context.OrderItem.Add(orderItem);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetOrderItem", new { id = orderItem.Id }, orderItem);
            }

            return BadRequest("Order is closed!");
        }

        // DELETE: api/OrderItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderItem = await _context.OrderItem.SingleOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItem.Remove(orderItem);
            await _context.SaveChangesAsync();

            return Ok(orderItem);
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItem.Any(e => e.Id == id);
        }

        #region PrivateMethods
        private UserCalculationCollection GetCalculationCollection(Order order, decimal tecaj)
        {
            var itemsGrouped = order.OrderItems.GroupBy(i => i.CreatedBy).ToList();

            var orderTotal = itemsGrouped.Sum(g => g.Sum(i => i.Amount * i.Price));
            var collection = new UserCalculationCollection(orderTotal, tecaj, order.Currency, order.Shipping);

            foreach (var userGroup in itemsGrouped)
            {
                var totalForUser = userGroup.Sum(i => i.Price * i.Amount);
                var shippingParticipationPercentage = totalForUser / orderTotal * 100;
                var userCalc = new UserCalculation(userGroup.Key, totalForUser, shippingParticipationPercentage);

                foreach (var item in userGroup)
                {
                    userCalc.OrderItems.Add(item);
                }

                collection.UserCalculations.Add(userCalc);
            }

            return collection;
        }
        
        private decimal GetTecaj(string currencyName)
        {
            decimal tecaj = 1m;
            if (currencyName == Constants.Currency.HRK) return tecaj;

            var apiUrl = $"http://api.hnb.hr/tecajn/v1?valuta={currencyName}";
            var request = (HttpWebRequest)WebRequest.Create(apiUrl);
            var responseJson = "";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            responseJson = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception("Get tecaj request failed!");
                }
            }

            if (!string.IsNullOrEmpty(responseJson))
            {
                responseJson = responseJson.Replace("[", "").Replace("]", "");

                try
                {
                    JObject jObject = JObject.Parse(responseJson);
                    tecaj = decimal.Parse((string)jObject.SelectToken($"['{_config.HNBApiTecaj}']"));
                }
                catch (Exception)
                {
                    throw new Exception("Get tecaj request failed!");
                }
            }

            return tecaj;
        }
        #endregion
    }
}