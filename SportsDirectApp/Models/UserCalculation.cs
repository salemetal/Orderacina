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

        public int ItemsAmount => OrderItems.Sum(i => i.Amount);

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
        public string Currency { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal Shipping { get; set; }
        public List<UserCalculation> UserCalculations { get; set; } = new List<UserCalculation>();

        public decimal OrderTotalHRK => OrderTotal * Tecaj;
        public decimal OrderTotalWithShipping => OrderTotal + Shipping;
        public decimal OrderTotalWithShippingHRK => OrderTotalWithShipping * Tecaj;
        public decimal ShippingHRK => Shipping * Tecaj;

        public int NoOfItems => UserCalculations.Sum(i => i.OrderItems.Count);

        public UserCalculationCollection(decimal orderTotal, decimal tecaj, string currency, decimal? shipping)
        {
            OrderTotal = orderTotal;
            Tecaj = tecaj;
            Currency = currency;
            Shipping = shipping ?? 0;
        }

        public string CreateHTMLContent()
        {
            var content = new StringBuilder();

            content.Append(CreateGroupTable());
            content.Append(CreateUserTables());

            return content.ToString();
        }


        #region Private Methods
        public string CreateGroupTable()
        {
            var table = new StringBuilder();

            var tecajInfo = Currency == Constants.Currency.HRK ? string.Empty : $"-Tečaj: {Tecaj}";

            table.Append($"<table class=\"table\"><caption>All Together - Currency: {Currency} {tecajInfo}</caption><tr>")
                .Append("<th>Username</th>")
                .Append("<th>Number of items</th>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total {Currency}</th>");

            table.Append("<th style=\"text-align:right\">Total HRK</th>")
                .Append("<th style=\"text-align:right\">Shipping %</th>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Shipping {Currency}</th>");

            table.Append("<th style=\"text-align:right\">Shipping HRK</th>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total + Shipping {Currency}</th>");

            table.Append("<th style=\"text-align:right\">Total + Shipping HRK</th></tr>");

            foreach (var userCalc in UserCalculations)
            {
                var shippingParticipation = Shipping * userCalc.ShippingParticipationPercentage / 100;
                var shippingParticipationHRK = Shipping * userCalc.ShippingParticipationPercentage / 100 * Tecaj;
                var totalWithShipping = shippingParticipation + userCalc.Total;
                var totalWithShippingHRK = (shippingParticipation + userCalc.Total) * Tecaj;

                table.Append($"<tr><td>{userCalc.Username}</td>")
                    .Append($"<td>{userCalc.OrderItems.Count()}</td>");

                if (Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{userCalc.Total:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{(userCalc.Total * Tecaj):0.00}</td>")
                .Append($"<td style=\"text-align:right\">{userCalc.ShippingParticipationPercentage:0.00} %</td>");

                if (Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{shippingParticipation:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{shippingParticipationHRK:0.00}</td>");

                if (Currency != Constants.Currency.HRK)
                    table.Append($"<td style=\"text-align:right\">{totalWithShipping:0.00}</td>");

                table.Append($"<td style=\"text-align:right\">{totalWithShippingHRK:0.00}</td></tr>");
            }

            table.Append($"<tr>")
                .Append("<td></td>")
                .Append($"<td>{NoOfItems}</td>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{OrderTotal:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{OrderTotalHRK:0.00}</td>")
            .Append($"<td></td>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{Shipping:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{ShippingHRK:0.00}</td>");

            if (Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{OrderTotalWithShipping:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{OrderTotalWithShippingHRK:0.00}</td></tr>")
                .Append("</table>");

            return table.ToString();
        }

        public string CreateUserTables()
        {
            var tables = new StringBuilder();

            foreach (var userCalc in UserCalculations)
            {
                tables.Append(CreateUserTable(userCalc));
            }

            return tables.ToString();
        }

        public string CreateUserTable(UserCalculation userCalc)
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

            if (Currency != Constants.Currency.HRK)
                table.Append($"<th style=\"text-align:right\">Total {Currency}</th>");

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

                if (Currency != Constants.Currency.HRK)
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

            if (Currency != Constants.Currency.HRK)
                table.Append($"<td style=\"text-align:right\">{userCalc.Total:0.00}</td>");

            table.Append($"<td style=\"text-align:right\">{(userCalc.Total * Tecaj):0.00}</td></tr>")
            .Append("</table>");

            return table.ToString();
        } 
        #endregion
    }


}

