using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportsDirectApp.Common;

namespace SportsDirectApp.Models
{
    public class UserCalculation
    {
        public string Username { get; set; }
        public decimal Total { get; set; }
        public decimal ShippingParticipationPercentage { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        private int? _itemsAmount;
        public int ItemsAmount
        {
            get
            {
                if (_itemsAmount == null)
                {
                    _itemsAmount = OrderItems.Sum(i => i.Amount);
                }

                return (int)_itemsAmount;
            }
        }

        public UserCalculation(string username, decimal total, decimal shippingParticipationPercentage)
        {
            Username = username;
            Total = total;
            ShippingParticipationPercentage = shippingParticipationPercentage;
        }
    }

    public class UserCalculationCollection
    {
        public decimal Tecaj { get; set; }
        public Order Order { get; set; }
        public List<UserCalculation> UserCalculations { get; set; } = new List<UserCalculation>();

        public decimal OrderTotalHRK => Order.Total * Tecaj;
        public decimal OrderTotalWithShipping => Order.Total + Order.Shipping ?? 0;
        public decimal OrderTotalWithShippingHRK => OrderTotalWithShipping * Tecaj;
        public decimal ShippingHRK => Order.Shipping ?? 0 * Tecaj;

        public int NoOfItems => UserCalculations.Sum(i => i.OrderItems.Count);

        public UserCalculationCollection(decimal tecaj, Order order)
        {
            Order = order;
            Tecaj = tecaj;
        }

        public void Init()
        {
            var itemsGrouped = Order.OrderItems.GroupBy(i => i.CreatedBy).ToList();

            var orderTotal = itemsGrouped.Sum(g => g.Sum(i => i.Amount * i.Price));

            foreach (var userGroup in itemsGrouped)
            {
                var totalForUser = userGroup.Sum(i => i.Price * i.Amount);
                var shippingParticipationPercentage = totalForUser / orderTotal * 100;
                var userCalc = new UserCalculation(userGroup.Key, totalForUser, shippingParticipationPercentage);

                foreach (var item in userGroup)
                {
                    userCalc.OrderItems.Add(item);
                }

                UserCalculations.Add(userCalc);
            }
        }

        #region Public Methods
        public string CreateHTMLContent()
        {
            var content = new StringBuilder();

            if (UserCalculations != null && UserCalculations.Any())
            {
                content.Append(CreateGroupTable());
                content.Append(CreateUserTables());
            }

            return content.ToString();
        }

        public string CreateBillTable()
        {
            var htmlTable = new StringBuilder();

            if (UserCalculations.Any())
            {
                htmlTable.Append("<html><head>");
                htmlTable.Append(GetEmailBillTableCss());
                htmlTable.Append("</head><body>");
                htmlTable.Append(CreateGroupTable(false));
                htmlTable.Append("</body></html>");
            }

            return htmlTable.ToString();
        }

        #endregion
        #region Private Methods
        private string CreateGroupTable(bool putCaption = true)
        {
            var table = new StringBuilder();

            var tecajInfo = Order.Currency == Constants.Currency.HRK ? string.Empty : $"-Tečaj: {Tecaj}";

            var caption = $"<caption>All Together - Currency: {Order.Currency} {tecajInfo}</caption>";

            table.Append($"<table class=\"table\">");
            if (putCaption) table.Append(caption);

            table.Append("<thead><tr><th>Username</th>")
            .Append("<th>Number of items</th>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total {Order.Currency}</th>");

            table.Append("<th style=\"text-align:right\">Total HRK</th>")
                .Append("<th style=\"text-align:right\">Shipping %</th>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Shipping {Order.Currency}</th>");

            table.Append("<th style=\"text-align:right\">Shipping HRK</th>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total + Shipping {Order.Currency}</th>");

            table.Append("<th style=\"text-align:right\">Total + Shipping HRK</th></tr></thead>");

            foreach (var userCalc in UserCalculations)
            {
                var shippingParticipation = Order.Shipping ?? 0 * userCalc.ShippingParticipationPercentage / 100;
                var shippingParticipationHRK = Order.Shipping ?? 0 * userCalc.ShippingParticipationPercentage / 100 * Tecaj;
                var totalWithShipping = shippingParticipation + userCalc.Total;
                var totalWithShippingHRK = (shippingParticipation + userCalc.Total) * Tecaj;

                table.Append($"<tr><td>{userCalc.Username}</td>")
                    .Append($"<td>{userCalc.OrderItems.Count()}</td>");

                if (Order.Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{userCalc.Total:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{(userCalc.Total * Tecaj):0.00}</td>")
                .Append($"<td style=\"text-align:right\">{userCalc.ShippingParticipationPercentage:0.00} %</td>");

                if (Order.Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{shippingParticipation:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{shippingParticipationHRK:0.00}</td>");

                if (Order.Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{totalWithShipping:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{totalWithShippingHRK:0.00}</td></tr>");
            }

            table.Append($"<tr>")
                .Append("<td></td>")
                .Append($"<td>{NoOfItems}</td>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{Order.Total:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{OrderTotalHRK:0.00}</td>")
            .Append($"<td></td>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{ Order.Shipping ?? 0:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{ShippingHRK:0.00}</td>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{OrderTotalWithShipping:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{OrderTotalWithShippingHRK:0.00}</td></tr>")
                .Append("</table>");

            return table.ToString();
        }

        private string CreateUserTables()
        {
            var tables = new StringBuilder();

            foreach (var userCalc in UserCalculations)
            {
                tables.Append(CreateUserTable(userCalc));
            }

            return tables.ToString();
        }

        private string CreateUserTable(UserCalculation userCalc)
        {
            var table = new StringBuilder();

            table.Append($"<table class=\"table\"><caption>{userCalc.Username}</caption><tr>")
                .Append("<th>Id</th>")
                .Append("<th>Date</th>")
                .Append("<th>Size</th>")
                .Append("<th>Description</th>")
                .Append("<th>Url</th>")
                .Append("<th style=\"text-align:right\">Price</th>")
                .Append("<th style=\"text-align:right\">Amount</th>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total {Order.Currency}</th>");

            table.Append("<th style=\"text-align:right\">Total HRK</th></tr>");

            foreach (var item in userCalc.OrderItems)
            {
                table.Append($"<tr><td>{item.Id}</td>")
                    .Append($"<td>{item.DateCreated:dd.MM.yyyy.}</td>")
                    .Append($"<td>{item.Size}</td>")
                    .Append($"<td>{item.Description}</td>")
                    .Append($"<td><a href=\"{item.Url}\">{item.Url}</a></td>")
                    .Append($"<td style=\"text-align:right\">{item.Price:0.00}</td>")
                    .Append($"<td style=\"text-align:right\">{item.Amount}</td>");

                if (Order.Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{item.Total:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{(item.Total * Tecaj):0.00}</td></tr>");
            }

            table.Append($"<tr><td></td>")
                .Append("<td></td>")
                .Append("<td></td>")
                .Append("<td></td>")
                .Append("<td></td>")
                .Append("<td></td>")
                .Append($"<td style=\"text-align:right\">{userCalc.ItemsAmount}</td>");

            if (Order.Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{userCalc.Total:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{(userCalc.Total * Tecaj):0.00}</td></tr>")
            .Append("</table>");

            return table.ToString();
        }

        private string GetEmailBillTableCss()
        {
            return
            @"<style>
                        .table {
                border: solid 1px #DDEEEE;
                border-collapse: collapse;
                border-spacing: 0;
                font: normal 13px Arial, sans-serif;
            }
            .table thead th {
                background-color: #DDEFEF;
                border: solid 1px #DDEEEE;
                color: #336B6B;
                padding: 10px;
                text-align: left;
                text-shadow: 1px 1px 1px #fff;
            }
            .table tbody td {
                border: solid 1px #DDEEEE;
                color: #333;
                padding: 10px;
                text-shadow: 1px 1px 1px #fff;
            }
                      </style>";
        }
        #endregion
    }


}

